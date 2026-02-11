using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace ProjectSynth.Character.Synth.States.Secondary
{
    public class Diva : BaseSkillState
    {
        public static float BaseDuration = 0.3f;
        public static float ProjectileSpeed = 60f;

        private float duration;
        private Animator animator;
        private GameObject nade;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!isAuthority) return;

            this.duration = BaseDuration / base.attackSpeedStat;
            this.animator = base.GetModelAnimator();

            if (animator)
            {
                base.PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", this.duration);
            }

            nade = SynthAssets.proj_Diva;

            Ray aimRay = base.GetAimRay();

            Vector3 dir = aimRay.direction;
            dir += Vector3.up * 0.1f;

            FireProjectileInfo info = new()
            {
                projectilePrefab = nade,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(dir.normalized),
                owner = base.gameObject,
                target = null,
                useSpeedOverride = true,
                speedOverride = ProjectileSpeed,
                useFuseOverride = false,
                damage = this.damageStat,
                force = 0f,
                crit = RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                damageTypeOverride = DamageSource.Secondary
            };

            ProjectileManager.instance.FireProjectile(info);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
