using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace ProjectSynth.Hologram
{
    // Put this on the projectile prefab (the one you fire / that can become the teleport target)
    // It will notify the OWNER's HologramController on the SERVER when the projectile spawns.
    public class HologramProjectileReporter : MonoBehaviour
    {
        private ProjectileController pc;

        private void Awake()
        {
            pc = GetComponent<ProjectileController>();
        }

        private void Start()
        {
            if (!pc || !pc.owner) return;

            CharacterBody body = pc.owner.GetComponent<CharacterBody>();
            if (!body) return;

            HologramController controller = body.GetComponent<HologramController>();
            if (!controller) return;

            controller.SetProjectile(gameObject);
        }
    }
}
