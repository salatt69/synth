using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.Hologram
{
    public class HologramController : NetworkBehaviour
    {
        public SkillDef expoShift;
        public GenericSkill hologramSkillSlot;
        public float maxTeleportDistance;
        [SyncVar] private NetworkInstanceId projectileID;
        [SyncVar] private NetworkInstanceId hologramID;
        private bool overrideApplied;
        private bool allowedToTeleport;
        private CharacterBody body;

        private Indicator indicatorInstance;
        private GameObject indicatorPrefab;
        private Transform cachedVisualizerTransform;
        private SpriteRenderer indicatorSprite;

        private Color validColor, invalidColor, tooFarColor;

        void Awake()
        {
            body = GetComponent<CharacterBody>();
            if (!hologramSkillSlot && body && body.skillLocator)
                hologramSkillSlot = body.skillLocator.secondary;

            if (!expoShift)
                expoShift = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Expo-Shift"));

            if (maxTeleportDistance <= 0f)
                maxTeleportDistance = 75f;

            indicatorPrefab = Addressables
                .LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiMissileTrackingIndicator.prefab")
                .WaitForCompletion();

            invalidColor = Color.red;
            tooFarColor = Color.gray;
        }

        void FixedUpdate()
        {
            if (HasTarget) EnsureOverride();
            else RemoveOverride();

            // client
            if (NetworkClient.active && IsLocalBody())
            {
                UpdateIndicatorClient();
            }
        }

        private void EnsureOverride()
        {
            if (overrideApplied) return;

            if (!body) body = GetComponent<CharacterBody>();
            if (!hologramSkillSlot && body && body.skillLocator)
                hologramSkillSlot = body.skillLocator.secondary;

            if (!expoShift)
                expoShift = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Expo-Shift"));

            if (!hologramSkillSlot || !expoShift) return;

            hologramSkillSlot.SetSkillOverride(this, expoShift, GenericSkill.SkillOverridePriority.Contextual);
            overrideApplied = true;
        }

        private void RemoveOverride()
        {
            if (!overrideApplied) return;
            if (!hologramSkillSlot || !expoShift) return;

            hologramSkillSlot.UnsetSkillOverride(this, expoShift, GenericSkill.SkillOverridePriority.Contextual);
            overrideApplied = false;
        }

        private static GameObject FindNetObject(NetworkInstanceId id)
        {
            if (id == NetworkInstanceId.Invalid) return null;

            if (NetworkServer.active)
            {
                return NetworkServer.FindLocalObject(id);
            }

            if (NetworkClient.active)
            {
                return ClientScene.FindLocalObject(id);
            }

            return null;
        }

        private void UpdateIndicatorClient()
        {
            // 1) draw only when teleport can actually be used
            if (!hologramSkillSlot || hologramSkillSlot.stock <= 0)
            {
                if (indicatorInstance != null) indicatorInstance.active = false;
                AllowedToTeleport = false;
                return;
            }

            Transform target = GetTargetTransform(out bool isProj);
            if (!target)
            {
                if (indicatorInstance != null) indicatorInstance.active = false;
                AllowedToTeleport = false;
                return;
            }

            // Create once
            if (indicatorInstance == null)
            {
                indicatorInstance = new Indicator(gameObject, indicatorPrefab);
                indicatorInstance.active = true;
                cachedVisualizerTransform = null; // force sprite reacquire
            }

            // Keep it alive and pointed at target
            indicatorInstance.targetTransform = target;
            indicatorInstance.active = true;

            // 2) Re-acquire the sprite renderer if visualizer got recreated
            if (indicatorInstance.visualizerTransform != cachedVisualizerTransform)
            {
                cachedVisualizerTransform = indicatorInstance.visualizerTransform;

                indicatorSprite = null;
                if (cachedVisualizerTransform)
                {
                    var core = cachedVisualizerTransform.Find("Base Container/Base Core");
                    if (core) indicatorSprite = core.GetComponent<SpriteRenderer>();

                    // Refresh default "valid" color from prefab visuals
                    if (indicatorSprite) validColor = indicatorSprite.color;
                }
            }

            if (!indicatorSprite) return;

            // Compute LOS + distance
            Vector3 from = body.inputBank ? body.inputBank.aimOrigin : body.corePosition;
            Vector3 to = GetLOSPoint(target);
            float dist = Vector3.Distance(from, to);

            bool inRange = IsValidDistance(dist);
            bool hasLos = HasLineOfSightToTarget(body, target);

            // Apply
            Color c = hasLos ? validColor : invalidColor;
            if (inRange)
            {
                c.a = 1f;
            }
            else
            {
                c = tooFarColor;
                c.a = 0.4f;
            }
            indicatorSprite.color = c;

            AllowedToTeleport = inRange && hasLos;
        }

        private bool IsLocalBody()
        {
            return body && body.hasEffectiveAuthority && body.isPlayerControlled;
        }


        #region api
        public void SetProjectile(GameObject proj)
        {
            if (!NetworkServer.active) return;
            projectileID = proj ? proj.GetComponent<NetworkIdentity>().netId : NetworkInstanceId.Invalid;
            hologramID = NetworkInstanceId.Invalid;
            EnsureOverride();
        }
        
        public void SetHologram(GameObject hologram)
        {
            if (!NetworkServer.active) return;
            hologramID = hologram ? hologram.GetComponent<NetworkIdentity>().netId : NetworkInstanceId.Invalid;
            projectileID = NetworkInstanceId.Invalid;
            EnsureOverride();
        }

        public Transform GetTargetTransform(out bool targetIsProjectile)
        {
            targetIsProjectile = false;

            var proj = FindNetObject(projectileID);
            if (proj)
            {
                targetIsProjectile = true;
                return proj.transform;
            }

            var holo = FindNetObject(hologramID);
            if (holo)
            {
                targetIsProjectile = false;
                return holo.transform;
            }

            return null;
        }

        public bool HasTarget
        {
            get
            {
                return GetTargetTransform(out _) != null;
            }
        }

        public bool AllowedToTeleport
        {
            get => allowedToTeleport;
            private set => allowedToTeleport = value;
        }

        public void ConsumeTargetAndClear()
        {
            if (!NetworkServer.active) return;

            var proj = FindNetObject(projectileID);
            if (proj)
            {
                NetworkServer.Destroy(proj);
                projectileID = NetworkInstanceId.Invalid;
            }

            var holo = FindNetObject(hologramID);
            if (holo)
            {
                NetworkServer.Destroy(holo);
                hologramID = NetworkInstanceId.Invalid;
            }

            RemoveOverride();
        }
        
        public bool HasLineOfSightToTarget(CharacterBody body, Transform target)
        {
            if (!body || !target) return false;

            Vector3 from = body.inputBank ? body.inputBank.aimOrigin : body.corePosition;
            Vector3 to = GetLOSPoint(target);

            Vector3 dir = to - from;
            float dist = dir.magnitude;
            if (dist <= 0.001f) return true;
            dir /= dist;

            int mask = LayerIndex.world.mask;

            // if hit world geometry before the point - blocked
            if (Physics.Raycast(from, dir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                // if the hit is basically at the target, allow it
                return hit.distance >= dist - 0.20f;
            }

            return true;
        }

        public Vector3 GetLOSPoint(Transform target)
        {
            // Prefer collider bounds (aim near the top so we don't ray into the ground)
            Collider col = target.GetComponentInChildren<Collider>();
            if (col)
            {
                Bounds b = col.bounds;
                return b.center + Vector3.up * (b.extents.y * 0.9f);
            }

            return target.position + Vector3.up * 1.25f;
        }

        public bool IsValidDistance(float magnitude)
        {
            return magnitude <= maxTeleportDistance;
        }

        #endregion
    }
}