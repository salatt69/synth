using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Hologram
{
    public class ExpoTracker : MonoBehaviour
    {
        public float maxTeleportDistance = 75f;
        public LayerMask losMask;
        public SkillDef blinkSkillDef;
        public GenericSkill overrideSlot;
        public GameObject indicatorPrefab;

        private CharacterBody body;
        private SkillLocator skillLocator;
        private GameObject projectile;
        private GameObject hologram;
        private Indicator projectileIndicator;
        private Indicator hologramIndicator;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            skillLocator = GetComponent<SkillLocator>();

            if (losMask.value == 0) losMask = LayerIndex.world.mask;
            if (!overrideSlot && skillLocator) overrideSlot = skillLocator.secondary;

            if (!indicatorPrefab)
                indicatorPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Engi/EngiShieldRetractIndicator.prefab").WaitForCompletion();

            BootstrapIfNeeded();

            projectileIndicator = new Indicator(gameObject, indicatorPrefab);
            hologramIndicator = new Indicator(gameObject, indicatorPrefab);
        }

        private void OnDestroy()
        {
            projectileIndicator?.DestroyVisualizer();
            hologramIndicator?.DestroyVisualizer();
        }

        private void Update()
        {
            BootstrapIfNeeded();

            // Clean destroyed refs
            if (projectile && !projectile) projectile = null;
            if (hologram && !hologram) hologram = null;

            UpdateIndicators();
        }

        private void BootstrapIfNeeded()
        {
            if (!overrideSlot && skillLocator) overrideSlot = skillLocator.secondary;

            if (!blinkSkillDef)
            {
                blinkSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Expo-Shift"));
            }
        }

        public void RegisterProjectile(GameObject proj)
        {
            projectile = proj;
            EnsureOverrideOn();
        }

        public void RegisterHologram(GameObject holo)
        {
            hologram = holo;
            EnsureOverrideOn();
        }

        public void ClearProjectileIf(GameObject proj)
        {
            if (projectile == proj) projectile = null;
            MaybeUnsetOverride();
        }

        public void ClearHologramIf(GameObject holo)
        {
            if (hologram == holo) hologram = null;
            MaybeUnsetOverride();
        }

        private void EnsureOverrideOn()
        {
            if (!overrideSlot || !blinkSkillDef) return;

            overrideSlot.SetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void MaybeUnsetOverride()
        {
            if (!overrideSlot || !blinkSkillDef) return;

            if (!projectile && !hologram)
            {
                overrideSlot.UnsetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public bool TryGetBestTarget(out Transform t, out bool isProjectile)
        {
            if (projectile)
            {
                t = projectile.transform;
                isProjectile = true;
                return true;
            }

            if (hologram)
            {
                t = hologram.transform;
                isProjectile = false;
                return true;
            }

            t = null;
            isProjectile = false;
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
                if (Physics.Raycast(from, dir, len, losMask, QueryTriggerInteraction.Ignore))
                {
                    blocked = true;
                    return false;
                }
            }
            return true;
        }

        public void ConsumeAndDestroyTarget(bool usedProjectile)
        {
            if (usedProjectile)
            {
                if (projectile) Destroy(projectile);
                projectile = null;
            }
            else
            {
                if (hologram) Destroy(hologram);
                hologram = null;
            }

            if (overrideSlot && blinkSkillDef)
                overrideSlot.UnsetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void UpdateIndicators()
        {
            if (projectileIndicator == null || hologramIndicator == null) return;

            Transform projT = projectile ? projectile.transform : null;
            Transform holoT = hologram ? hologram.transform : null;

            projectileIndicator.targetTransform = projT;
            hologramIndicator.targetTransform = holoT;

            projectileIndicator.active = projT != null;
            hologramIndicator.active = holoT != null;

            projectileIndicator.UpdateVisualizer();
            hologramIndicator.UpdateVisualizer();

            if (projT) SetIndicatorColor(projectileIndicator, GetTeleportColor(projT.position));
            if (holoT) SetIndicatorColor(hologramIndicator, GetTeleportColor(holoT.position));
        }

        private Color GetTeleportColor(Vector3 targetPos)
        {
            if (!body) return Color.gray;

            Vector3 from = body.corePosition;
            float dist = Vector3.Distance(from, targetPos);

            if (dist > maxTeleportDistance) return Color.gray;

            Vector3 dir = (targetPos - from);
            float len = dir.magnitude;
            if (len <= 0.001f) return Color.green;

            dir /= len;

            if (Physics.Raycast(from, dir, len, losMask, QueryTriggerInteraction.Ignore))
                return Color.red;

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
