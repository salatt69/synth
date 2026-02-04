using ProjectSynth.Core;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Hologram
{
    public class HologramSpawnBehavior : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject hologramPrefab;

        private Vector3 impactPoint;
        private Vector3 impactNormal;
        private int groundImpactCount;
        private readonly float force = 20f;

        private ProjectileController projectileController;

        private void Awake()
        {
            projectileController = GetComponent<ProjectileController>();
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!hologramPrefab) return;

            this.impactPoint = impactInfo.estimatedPointOfImpact;
            this.impactNormal = impactInfo.estimatedImpactNormal;
            Collider collider = impactInfo.collider;

            HurtBox component = collider.GetComponent<HurtBox>();
            if (component) return;

            var rot = Quaternion.FromToRotation(Vector3.up, impactNormal);

            groundImpactCount++;
            if (groundImpactCount > 3)
            {
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.AddForce(impactNormal * force, ForceMode.Impulse);
                }
                else
                {
                    NetworkServer.Destroy(gameObject);
                }
            }

            if (impactNormal.y < 0.65f) return;

            GameObject hologram = Instantiate(hologramPrefab, impactPoint, rot);

            var notify = hologram.GetComponent<HologramLifetimeNotify>();
            if (notify && projectileController && projectileController.owner)
                notify.owner = projectileController.owner;

            NetworkServer.Spawn(hologram);

            try
            {
                if (projectileController && projectileController.owner)
                {
                    var tracker = projectileController.owner.GetComponent<ExpoTracker>();
                    if (tracker) tracker.RegisterHologram(hologram);
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"[{this}] failed to notify owner: {e}");
            }

            NetworkServer.Destroy(gameObject);
        }
    }
}
