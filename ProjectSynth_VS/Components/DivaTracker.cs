using ProjectSynth.Character.Synth.Content;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Networking;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.Components
{
    public class DivaTracker : MonoBehaviour
    {
        public struct ConsumeOwnedBeaconMessage : INetMessage
        {
            public NetworkInstanceId bodyNetId;

            public readonly void Serialize(NetworkWriter writer) => writer.Write(bodyNetId);
            public void Deserialize(NetworkReader reader) => bodyNetId = reader.ReadNetworkId();

            public readonly void OnReceived()
            {
                if (!NetworkServer.active) return;

                GameObject bodyObj = Util.FindNetworkObject(bodyNetId);
                if (!bodyObj) return;

                var tracker = bodyObj.GetComponent<DivaTracker>();
                if (!tracker) return;

                tracker.ConsumeOwnedBeaconServer();
            }
        }

        public float maxTeleportDistance = 75f;
        public LayerMask losMask;
        public SkillDef blinkSkillDef;
        public GenericSkill overrideSlot;
        public GameObject indicatorPrefab;

        private CharacterBody body;
        private SkillLocator skillLocator;

        private ProjectileMarker cachedBeacon;
        private Indicator beaconIndicator;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            skillLocator = GetComponent<SkillLocator>();

            Bootstrap();

            beaconIndicator = new Indicator(gameObject, indicatorPrefab);
        }

        private void OnDestroy()
        {
            beaconIndicator?.DestroyVisualizer();
        }

        private void Update()
        {
            if (losMask.value == 0 || !blinkSkillDef || !indicatorPrefab || !overrideSlot)
            {
                Bootstrap();
            }

            if (!body || !body.hasAuthority) return;

            cachedBeacon = FindOwnedBeacon();

            bool hasBeacon = cachedBeacon != null;
            bool canUse = overrideSlot && overrideSlot.stock > 0;

            UpdateIndicator(hasBeacon && canUse);

            if (hasBeacon)
                EnsureOverrideOn();
            else
                MaybeUnsetOverride();
        }

        private void Bootstrap()
        {
            if (losMask.value == 0) losMask = LayerIndex.world.mask;

            if (!blinkSkillDef)
                blinkSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Virtual Deviation Teleport"));

            if (!overrideSlot && skillLocator) overrideSlot = skillLocator.secondary;

            if (!indicatorPrefab)
                indicatorPrefab = SynthAssets.divaIndicator;
        }

        private ProjectileMarker FindOwnedBeacon()
        {
            var list = InstanceTracker.GetInstancesList<ProjectileMarker>();
            if (list == null || list.Count == 0) return null;

            ProjectileMarker best = null;

            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                if (!m) continue;

                var owner = m.GetOwner();
                if (!owner) continue;

                // MP-safe ownership filter:
                if (owner != gameObject) continue;

                // Optional prefer stuck ones
                var stick = m.GetComponent<ProjectileStickOnImpactByNormal>();
                bool isStuck = stick && stick.stuck;

                if (best == null)
                {
                    best = m;
                    continue;
                }

                var bestStick = best.GetComponent<ProjectileStickOnImpactByNormal>();
                bool bestStuck = bestStick && bestStick.stuck;

                if (isStuck && !bestStuck)
                    best = m;
            }

            return best;
        }

        private void EnsureOverrideOn()
        {
            if (!overrideSlot || !blinkSkillDef) return;
            overrideSlot.SetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void MaybeUnsetOverride()
        {
            if (!overrideSlot || !blinkSkillDef) return;
            overrideSlot.UnsetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void ClearClientUX()
        {
            cachedBeacon = null;

            if (beaconIndicator != null)
            {
                beaconIndicator.targetTransform = null;
                beaconIndicator.active = false;
                beaconIndicator.UpdateVisualizer();
            }

            // local override cleanup
            if (overrideSlot && blinkSkillDef)
                overrideSlot.UnsetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void ConsumeOwnedBeaconServer()
        {
            if (!NetworkServer.active) return;

            var beacon = FindOwnedBeacon();
            if (beacon)
            {
                NetworkServer.Destroy(beacon.gameObject);
            }
        }

        #region indicator
        private void UpdateIndicator(bool shouldShow)
        {
            if (beaconIndicator == null) return;

            if (!shouldShow || !cachedBeacon)
            {
                beaconIndicator.targetTransform = null;
                beaconIndicator.active = false;
                beaconIndicator.UpdateVisualizer();
                return;
            }

            if (!IsLookingAt(cachedBeacon.transform.position))
            {
                beaconIndicator.targetTransform = null;
                beaconIndicator.active = false;
                beaconIndicator.UpdateVisualizer();
                return;
            }

            var t = cachedBeacon.transform;

            beaconIndicator.targetTransform = t;
            beaconIndicator.active = true;
            beaconIndicator.UpdateVisualizer();

            Color c = GetTeleportColor(t.position);
            SetIndicatorColor(beaconIndicator, c);
        }

        private Color GetTeleportColor(Vector3 targetPos)
        {
            if (!body) return Color.gray;

            float dist = Vector3.Distance(body.corePosition, targetPos);
            if (dist > maxTeleportDistance) return Color.gray;

            bool blocked;
            float d;
            bool ok = CanTeleportTo(targetPos, out blocked, out d);

            if (!ok && blocked) return Color.red;
            if (!ok) return Color.gray;
            return Color.green;
        }

        private void SetIndicatorColor(Indicator indicator, Color c)
        {
            if (indicator?.visualizerInstance)
            {
                var sr = indicator.visualizerInstance.GetComponentInChildren<SpriteRenderer>();
                if (sr) sr.color = c;

                var mr = indicator.visualizerInstance.GetComponentInChildren<MeshRenderer>();
                if (mr && mr.material) mr.material.color = c;
            }
        }
        #endregion

        #region api
        public void ConsumeCurrentTarget()
        {
            if (body && body.hasAuthority)
            {
                ClearClientUX();
            }

            if (NetworkServer.active)
            {
                ConsumeOwnedBeaconServer();
                return;
            }

            if (NetworkClient.active && body && body.hasAuthority)
            {
                var ni = body.GetComponent<NetworkIdentity>();
                if (!ni) return;

                new ConsumeOwnedBeaconMessage { bodyNetId = ni.netId }.Send(NetworkDestination.Server);
            }
        }

        public bool CanTeleportTo(Vector3 pos, out bool blocked, out float dist)
        {
            blocked = false;
            dist = 999f;
            if (!body) return false;

            Vector3 from = body.corePosition;
            dist = Vector3.Distance(from, pos);
            if (dist > maxTeleportDistance) return false;

            Vector3 dir = (pos - from);
            float len = dir.magnitude;
            if (len > 0.001f)
            {
                dir /= len;

                if (Physics.Raycast(from, dir, out RaycastHit hit, dist, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    bool allowed = hit.distance >= dist - 0.20f;
                    if (!allowed) blocked = true;
                    return allowed;
                }
            }

            return true;
        }

        private bool IsLookingAt(Vector3 targetPos)
        {
            if (!body) return false;

            Vector3 direction = targetPos - body.inputBank.aimOrigin;
            direction.y = 0;
            if (direction.magnitude < 0.001f) return false;

            Vector3 forward = body.inputBank.aimDirection;
            forward.y = 0;
            if (forward.magnitude < 0.001f) return false;

            float dot = Vector3.Dot(direction.normalized, forward.normalized);
            return dot > 0.9f;
        }

        public bool TryGetBestTarget(out Transform t)
        {
            if (cachedBeacon)
            {
                t = cachedBeacon.GetTransform();
                return true;
            }

            t = null;
            return false;
        }
        #endregion
    }
}
