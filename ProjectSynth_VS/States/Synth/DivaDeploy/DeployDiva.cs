using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace ProjectSynth.States.Synth.DivaDeploy
{
    public class DeployDiva : BaseSkillState
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

            duration = BaseDuration / this.attackSpeedStat;
            animator = this.GetModelAnimator();

            if (animator)
            {
                this.PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", duration);
            }

            nade = SynthAssets.proj_Diva;

            Ray aimRay = GetAimRay();

            Vector3 dir = aimRay.direction;
            dir += Vector3.up * 0.1f;

            FireProjectileInfo info = new()
            {
                projectilePrefab = nade,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(dir.normalized),
                owner = this.gameObject,
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
