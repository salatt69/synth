using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace ProjectSynth.Character.Synth.States.Secondary
{
    public class HoloNade : BaseSkillState
    {
        public static float BaseDuration = 0.3f;
        public static float BaseDelayDuration = 0.0f;
        public static float DamageCoefficient = 16f;
        public static float ProjectileSpeed = 60f;

        private float duration; 
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = BaseDuration / base.attackSpeedStat;
            this.animator = base.GetModelAnimator();

            if (animator)
            {
                base.PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", this.duration);
            }

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                Vector3 dir = aimRay.direction;
                dir += Vector3.up * 0.1f;

                FireProjectileInfo info = new()
                {
                    projectilePrefab = SynthAssets.proj_HoloNade,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(dir.normalized),
                    owner = base.gameObject,
                    target = null,
                    useSpeedOverride = true,
                    speedOverride = ProjectileSpeed,
                    useFuseOverride = false,
                    damage = DamageCoefficient * this.damageStat,
                    force = 0f,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    damageTypeOverride = DamageSource.Secondary
                };
                ProjectileManager.instance.FireProjectile(info);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}