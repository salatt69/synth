using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.Hologram
{
    public class DivaTracker : MonoBehaviour
    {
        public float maxTeleportDistance = 75f;
        public LayerMask losMask;
        public SkillDef blinkSkillDef;
        public GenericSkill overrideSlot;
        public GameObject indicatorPrefab;

        private CharacterBody body;
        private SkillLocator skillLocator;

        private DivaMarker cachedBeacon;
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

            // Only the local player needs indicator logic
            if (!body || !body.hasAuthority) return;

            // Discover our beacon each frame (cheap via InstanceTracker)
            cachedBeacon = FindOwnedBeacon();

            UpdateIndicator();

            // If you want override to be client-driven for now (SP-style),
            // you can do it here. In MP, server should be truth,
            // BUT you asked to keep it SP-like.
            if (cachedBeacon)
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
                indicatorPrefab = Addressables.LoadAssetAsync<GameObject>(
                    "RoR2/Junk/Engi/EngiShieldRetractIndicator.prefab").WaitForCompletion();
        }

        // -------- Beacon discovery (no SyncVars) --------
        private DivaMarker FindOwnedBeacon()
        {
            // InstanceTracker returns a static list of active components of that type.
            var list = InstanceTracker.GetInstancesList<DivaMarker>();
            if (list == null || list.Count == 0) return null;

            DivaMarker best = null;

            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                if (!m) continue;

                var owner = m.GetOwner();
                if (!owner) continue;

                // This is the crucial MP-safe ownership filter:
                if (owner != gameObject) continue;

                // Optionally, prefer stuck/armed beacons:
                // If your beacon has your custom stick component, prefer stuck ones:
                var stick = m.GetComponent<ProjectileStickOnImpactByNormal>(); // or your new stick class
                bool isStuck = stick && stick.stuck;

                if (best == null)
                {
                    best = m;
                    continue;
                }

                var bestStick = best.GetComponent<ProjectileStickOnImpactByNormal>();
                bool bestStuck = bestStick && bestStick.stuck;

                if (isStuck && !bestStuck)
                {
                    best = m;
                }
                // else keep current
            }

            return best;
        }

        // -------- Override (SP-style) --------
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

        // -------- Public API for Blink SkillState --------
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

                // Near-end forgiveness like you wanted
                if (Physics.Raycast(from, dir, out RaycastHit hit, dist, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    bool allowed = hit.distance >= dist - 0.20f;
                    if (!allowed) blocked = true;
                    return allowed;
                }
            }

            return true;
        }

        public void ConsumeCurrentTarget()
        {
            if (!NetworkServer.active) return;

            // Find the owned beacon the same way you target it.
            var beacon = FindOwnedBeacon(); // your internal scan method that returns DivaMarker
            if (beacon)
            {
                NetworkServer.Destroy(beacon.gameObject);
            }

            if (overrideSlot && blinkSkillDef)
                overrideSlot.UnsetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        // -------- Indicator --------
        private void UpdateIndicator()
        {
            if (beaconIndicator == null) return;

            Transform t = cachedBeacon ? cachedBeacon.transform : null;

            beaconIndicator.targetTransform = t;
            beaconIndicator.active = t != null;
            beaconIndicator.UpdateVisualizer();

            if (t != null)
            {
                Color c = GetTeleportColor(t.position);
                SetIndicatorColor(beaconIndicator, c);
            }
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
    }
}
