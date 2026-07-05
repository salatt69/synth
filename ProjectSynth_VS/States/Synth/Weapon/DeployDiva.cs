using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace ProjectSynth.States.Synth.Weapon
{
    public class DeployDiva : BaseSkillState
    {
        public static float BaseDuration = 0.3f;
        public static float ProjectileSpeed = 60f;

        public GameObject projectile = SynthAssets.proj_Diva;

        private float duration;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!isAuthority) return;

            duration = BaseDuration / attackSpeedStat;
            animator = GetModelAnimator();

            if (animator)
            {
                PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", duration);
            }

            Ray aimRay = GetAimRay();

            Vector3 dir = aimRay.direction;
            dir += Vector3.up * 0.1f;

            FireProjectileInfo info = new()
            {
                projectilePrefab = projectile,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(dir.normalized),
                owner = gameObject,
                target = null,
                useSpeedOverride = true,
                speedOverride = ProjectileSpeed,
                useFuseOverride = false,
                damage = damageStat,
                force = 0f,
                crit = RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                damageTypeOverride = DamageSource.Secondary
            };

            ProjectileManager.instance.FireProjectile(info);

            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
