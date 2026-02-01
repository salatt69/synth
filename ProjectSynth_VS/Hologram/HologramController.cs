using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Hologram
{
    public class HologramController : MonoBehaviour
    {
        public SkillDef expoShift;
        public GenericSkill hologramSkillSlot;

        private NetworkInstanceId projectileID;
        private NetworkInstanceId hologramID;
        private bool overrideApplied;
        private CharacterBody body;

        void Awake()
        {
            body = GetComponent<CharacterBody>();
            if (!hologramSkillSlot && body && body.skillLocator)
            {
                hologramSkillSlot = body.skillLocator.secondary;
            }
        }   

        void FixedUpdate()
        {
            if (!NetworkServer.active) return;

            if (HasTarget)
            {
                EnsureOverride();
            }
            else
            {
                RemoveOverride();
            }
        }

        private void EnsureOverride()
        {
            if (overrideApplied) return;
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
            return NetworkServer.active ? NetworkServer.FindLocalObject(id) : null;
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

            if (!NetworkServer.active) return null;

            var proj = projectileID != NetworkInstanceId.Invalid ? NetworkServer.FindLocalObject(projectileID) : null;
            if (proj)
            {
                targetIsProjectile = true;
                return proj.transform;
            }

            var holo = hologramID != NetworkInstanceId.Invalid ? NetworkServer.FindLocalObject(hologramID) : null;
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
                bool _;
                return GetTargetTransform(out _) != null;
            }
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
            Vector3 to = GetLosPoint(target);

            Vector3 dir = to - from;
            float dist = dir.magnitude;
            if (dist <= 0.001f) return true;
            dir /= dist;

            int mask = LayerIndex.world.mask;

            // If we hit world geometry before the point => blocked
            if (Physics.Raycast(from, dir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                // "Near-end forgiveness": if the hit is basically at the target, allow it
                return hit.distance >= dist - 0.20f;
            }

            return true;
        }

        public Vector3 GetLosPoint(Transform target)
        {
            // Prefer collider bounds (aim near the top so we don't ray into the ground)
            Collider col = target.GetComponentInChildren<Collider>();
            if (col)
            {
                Bounds b = col.bounds;
                return b.center + Vector3.up * (b.extents.y * 0.9f);
            }

            // Fallback: above pivot
            return target.position + Vector3.up * 1.25f;
        }
        #endregion
    }
}