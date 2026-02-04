using RoR2.Projectile;
using UnityEngine;

namespace ProjectSynth.Hologram
{
    public class ProjectileTag : MonoBehaviour
    {
        private ProjectileController pc;

        private void Awake()
        {
            pc = GetComponent<ProjectileController>();
        }

        private void Start()
        {
            if (pc && pc.owner)
            {
                var tracker = pc.owner.GetComponent<ExpoTracker>();
                if (tracker) tracker.RegisterProjectile(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (pc && pc.owner)
            {
                var tracker = pc.owner.GetComponent<ExpoTracker>();
                if (tracker) tracker.ClearProjectileIf(gameObject);
            }
        }
    }
}
