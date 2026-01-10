using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Character.Synth.Content.Items;
using ProjectSynth.Metronome;
using RoR2.Projectile;
using UnityEngine;

namespace ProjectSynth.Character.Synth.States.Primary
{
    public class ThirtyNineMusic : GenericProjectileBaseState, IMetronomeBoostable
    {
        public static float BaseDuration = 0.7f;
        public static float DamageCoefficient = SynthStaticValues.thirtNineMusicDamageCoefficient;

        private bool fireDouble;

        public override void OnEnter()
        {
            projectilePrefab = SynthAssets.thirtyNineMusicProjectile;

            baseDuration = BaseDuration;
            damageCoefficient = DamageCoefficient;

            fireDouble = false;

            base.OnEnter();

            if (PassiveItems.HasMetronomePassive(characterBody))
            {
                var metro = characterBody.GetComponent<MetronomeComponent>();
                if (metro == null)
                    return;

                // 1) Attempt start
                metro.StartMetronomeSequence();

                // 2) Attempt consume
                if (metro.CanConsumeCharge())
                {
                    ApplyBoost();
                    metro.IncreaseMaxBounces();
                }
            }

            // you will get some stats boosted and "Encore" debuff applied to an enemy you hit,
            // if you have a metronome passive AND you hit the window during a sequence
            //if (MetronomeSequenceManager.TryConsumeCharge()) 
            //    ApplyMetronomeBoost();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void FireProjectile()
        {
            if (!isAuthority) return;

            Ray aimRay = GetAimRay();
            Vector3 right = Vector3.Cross(Vector3.up, aimRay.direction).normalized;

            float offset = 1.2f;

            FireProjectileInfo info = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                owner = gameObject,
                damage = damageStat * damageCoefficient,
                force = force,
                crit = RollCrit(),
                rotation = Quaternion.LookRotation(aimRay.direction),
                position = aimRay.origin,
            };

            if (!fireDouble)
            {
                ProjectileManager.instance.FireProjectile(info);
                return;
            }

            info.position = aimRay.origin - right * offset;
            ProjectileManager.instance.FireProjectile(info);

            info.position = aimRay.origin + right * offset;
            ProjectileManager.instance.FireProjectile(info);
        }



        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            base.ModifyProjectileInfo(ref fireProjectileInfo);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void PlayAnimation(float duration)
        {
            if (GetModelAnimator())
            {
                PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", duration);
            }
        }

        public void ApplyBoost()
        {
            fireDouble = true;

            damageCoefficient = SynthStaticValues.boosted_thirtNineMusicDamageCoefficient;
        }
    }
}