using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Hologram
{
    public class HologramSpawnBehavior : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject objectToSpawn;

        private Vector3 impactPoint;
        private Vector3 impactNormal;
        private int groundImpactCount;
        private readonly float force = 20f;

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (NetworkServer.active)
            {
                this.impactPoint = impactInfo.estimatedPointOfImpact;
                this.impactNormal = impactInfo.estimatedImpactNormal;
                Collider collider = impactInfo.collider;

                HurtBox component = collider.GetComponent<HurtBox>();
                if (component) return;

                Chat.AddMessage($"Normal: {impactNormal}");
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
                        Destroy(gameObject);
                    }
                }

                if (impactNormal.y < 0.65f) return;

                if (this.objectToSpawn)
                {
                    Instantiate(objectToSpawn, impactPoint, rot);
                    Destroy(gameObject);
                }
            }
        }
    }
}