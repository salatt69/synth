using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ProjectSynth.Hologram
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ProjectileNetworkTransform))]
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileStickOnImpactByNormal : NetworkBehaviour, IProjectileImpactBehavior
    {
        [Header("Stick Rules")]
        [Range(-1f, 1f)]
        public float minGroundNormalY = 0.65f;

        public bool ignoreCharacters = true;
        public bool ignoreWorld = false;

        [Header("Pose")]
        public bool alignNormals = true;
        public bool invertNormal = false;

        [Header("FX")]
        public string stickSoundString = "";
        public ParticleSystem[] stickParticleSystem;
        public UnityEvent stickEvent;

        // Similar API surface to PSOI
        public Transform stuckTransform { get; private set; }
        public CharacterBody stuckBody { get; private set; }
        public bool stuck => hitMode != HitMode.None;

        // --- networking state (server writes, clients read) ---
        private enum HitMode : sbyte { None = -1, World = -2, Hurtbox = 0 }

        // victim for Hurtbox mode only (netId so it resolves on all clients)
        [SyncVar] private NetworkInstanceId syncVictimNetId;

        [SyncVar] private sbyte syncHitMode = (sbyte)HitMode.None;
        [SyncVar] private sbyte syncHurtboxIndex = -1;

        // attachment pose (for Hurtbox mode)
        [SyncVar] private Vector3 syncLocalPosition;
        [SyncVar] private Quaternion syncLocalRotation;

        // world pose (for World mode)
        [SyncVar] private Vector3 syncWorldPosition;
        [SyncVar] private Quaternion syncWorldRotation;

        // FX/event trigger (increment to re-trigger on clients)
        [SyncVar] private int syncStickFxNonce;

        private Rigidbody rb;
        private ProjectileController pc;

        private int lastSeenFxNonce;

        private HitMode hitMode => (HitMode)syncHitMode;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            pc = GetComponent<ProjectileController>();

            // NOTE: This is unusual (loading another prefab's particle system reference),
            // but I kept your behavior. Prefer assigning particle systems in prefab.
            try
            {
                var s = Addressables
                    .LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiMine.prefab")
                    .WaitForCompletion();

                if (s)
                {
                    var ring = s.transform.Find("Ring");
                    if (ring)
                    {
                        stickParticleSystem = new ParticleSystem[]
                        {
                            ring.GetComponent<ParticleSystem>()
                        };
                    }
                }
            }
            catch { /* ignore addressables load failures */ }
        }

        private void OnDisable()
        {
            // If server despawns / disables, clear state.
            if (NetworkServer.active)
                Detach();
        }

        [Server]
        public void Detach()
        {
            syncVictimNetId = NetworkInstanceId.Invalid;
            syncHitMode = (sbyte)HitMode.None;
            syncHurtboxIndex = -1;

            stuckTransform = null;
            stuckBody = null;

            // don’t touch fx nonce here; detach isn’t “stick”
            UpdateStickingServerAndClient();
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!enabled) return;

            // ✅ Server decides sticking; clients only observe SyncVars.
            if (!NetworkServer.active) return;

            TryStick(impactInfo.collider, impactInfo.estimatedImpactNormal);
        }

        [Server]
        private bool TryStick(Collider hitCollider, Vector3 impactNormal)
        {
            if (stuck) return false;
            if (!hitCollider) return false;

            // --- character / hurtbox ---
            HurtBox hb = hitCollider.GetComponent<HurtBox>();
            if (hb != null)
            {
                if (ignoreCharacters) return false;

                var hc = hb.healthComponent;
                if (!hc) return false;

                // don't stick to owner
                if (pc && pc.owner && hc.gameObject == pc.owner) return false;

                if (!PassesNormalGate(impactNormal)) return false;

                var victimNi = hc.gameObject.GetComponent<NetworkIdentity>();
                if (!victimNi)
                    return false; // must be networked to resolve on clients

                syncVictimNetId = victimNi.netId;
                syncHitMode = (sbyte)HitMode.Hurtbox;
                syncHurtboxIndex = (sbyte)hb.indexInGroup;

                ApplyPoseOnStick_Hurtbox(hitCollider.transform, impactNormal);
                FireStickFx_ServerPulse();

                return true;
            }

            // --- world ---
            if (ignoreWorld) return false;

            if (!PassesNormalGate(impactNormal)) return false;

            syncVictimNetId = NetworkInstanceId.Invalid;
            syncHitMode = (sbyte)HitMode.World;
            syncHurtboxIndex = -1;

            ApplyPoseOnStick_World(hitCollider.transform, impactNormal);
            FireStickFx_ServerPulse();

            return true;
        }

        private bool PassesNormalGate(Vector3 n)
        {
            if (n == Vector3.zero) return false;
            float y = n.normalized.y;
            return y >= minGroundNormalY;
        }

        [Server]
        private void ApplyPoseOnStick_Hurtbox(Transform hitTransform, Vector3 impactNormal)
        {
            // Align now (server authoritatively decides rotation)
            if (alignNormals && impactNormal != Vector3.zero)
            {
                Vector3 up = invertNormal ? -impactNormal : impactNormal;
                up.Normalize();

                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, up);

                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(Vector3.forward, up);

                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(Vector3.right, up);

                forward.Normalize();
                transform.rotation = Quaternion.LookRotation(forward, up);
            }

            // Store attachment pose relative to the transform we hit (clients will resolve to same model/hurtbox)
            syncLocalPosition = hitTransform.InverseTransformPoint(transform.position);
            syncLocalRotation = Quaternion.Inverse(hitTransform.rotation) * transform.rotation;

            UpdateStickingServerAndClient();
        }

        [Server]
        private void ApplyPoseOnStick_World(Transform hitTransform, Vector3 impactNormal)
        {
            // Align now (server decides)
            if (alignNormals && impactNormal != Vector3.zero)
            {
                Vector3 up = invertNormal ? -impactNormal : impactNormal;
                up.Normalize();

                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, up);

                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(Vector3.forward, up);

                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(Vector3.right, up);

                forward.Normalize();
                transform.rotation = Quaternion.LookRotation(forward, up);
            }

            // For world stick, do NOT rely on a victim object. Just store the final world pose.
            syncWorldPosition = transform.position;
            syncWorldRotation = transform.rotation;

            UpdateStickingServerAndClient();
        }

        [Server]
        private void FireStickFx_ServerPulse()
        {
            // increment => clients detect change and run fx once
            syncStickFxNonce++;

            // also run immediately on server/host
            FireStickFx_Local();
        }

        private void FireStickFx_Local()
        {
            if (stickParticleSystem != null)
            {
                for (int i = 0; i < stickParticleSystem.Length; i++)
                    if (stickParticleSystem[i]) stickParticleSystem[i].Play();
            }

            if (!string.IsNullOrEmpty(stickSoundString))
                Util.PlaySound(stickSoundString, gameObject);

            stickEvent?.Invoke();
        }

        private void FixedUpdate()
        {
            UpdateStickingServerAndClient();

            // FX pulse replication (clients + host)
            if (lastSeenFxNonce != syncStickFxNonce)
            {
                lastSeenFxNonce = syncStickFxNonce;

                // If nonce changed and we are currently stuck, play the fx/event locally.
                // (Server already played it in FireStickFx_ServerPulse(), but host client will also hit this path;
                // if you want to avoid double-play on host, guard with `if (!NetworkServer.active)`.)
                if (stuck && !NetworkServer.active)
                    FireStickFx_Local();
            }
        }

        private void UpdateStickingServerAndClient()
        {
            bool isStuckNow = stuck;

            if (isStuckNow)
            {
                if (hitMode == HitMode.World)
                {
                    // ✅ world stick: independent of victim network object
                    transform.SetPositionAndRotation(syncWorldPosition, syncWorldRotation);
                }
                else if (hitMode == HitMode.Hurtbox)
                {
                    // Resolve victim root object from netId
                    if (!stuckTransform)
                    {
                        GameObject victimObj = null;

                        if (syncVictimNetId != NetworkInstanceId.Invalid)
                        {
                            if (NetworkServer.active)
                            {
                                // server can resolve directly by searching spawned objects too,
                                // but ClientScene works on host; easiest is just use ClientScene universally when available.
                                victimObj = ClientScene.FindLocalObject(syncVictimNetId);
                                if (!victimObj)
                                {
                                    // fallback on server: try spawned dictionary
                                    var ni = NetworkServer.FindLocalObject(syncVictimNetId);
                                    victimObj = ni;
                                }
                            }
                            else
                            {
                                victimObj = ClientScene.FindLocalObject(syncVictimNetId);
                            }
                        }

                        if (victimObj)
                        {
                            // default stick to victim root
                            stuckTransform = victimObj.transform;

                            // Try refine to HurtBox transform for better attachment
                            stuckBody = victimObj.GetComponent<CharacterBody>();

                            if (syncHurtboxIndex >= 0)
                            {
                                var modelLocator = victimObj.GetComponent<ModelLocator>();
                                if (modelLocator && modelLocator.modelTransform)
                                {
                                    var hbg = modelLocator.modelTransform.GetComponent<HurtBoxGroup>();
                                    if (hbg && hbg.hurtBoxes != null &&
                                        syncHurtboxIndex < hbg.hurtBoxes.Length &&
                                        hbg.hurtBoxes[syncHurtboxIndex])
                                    {
                                        stuckTransform = hbg.hurtBoxes[syncHurtboxIndex].transform;
                                    }
                                }
                            }
                        }
                    }

                    if (stuckTransform)
                    {
                        transform.SetPositionAndRotation(
                            stuckTransform.TransformPoint(syncLocalPosition),
                            alignNormals ? (stuckTransform.rotation * syncLocalRotation) : transform.rotation
                        );
                    }
                }
            }

            // Server controls physics mode like PSOI
            if (NetworkServer.active)
            {
                if (rb && rb.isKinematic != isStuckNow)
                {
                    if (isStuckNow)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;

                        rb.detectCollisions = false;
                        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                        rb.isKinematic = true;
                    }
                    else
                    {
                        rb.detectCollisions = true;
                        rb.isKinematic = false;
                        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    }
                }

                // Safety: if victim vanished, unstick (hurtbox mode only)
                if (isStuckNow && hitMode == HitMode.Hurtbox)
                {
                    if (syncVictimNetId == NetworkInstanceId.Invalid)
                    {
                        Detach();
                    }
                    else
                    {
                        // if it can't be resolved, also detach
                        var obj = NetworkServer.FindLocalObject(syncVictimNetId);
                        if (!obj) Detach();
                    }
                }
            }
        }
    }
}
