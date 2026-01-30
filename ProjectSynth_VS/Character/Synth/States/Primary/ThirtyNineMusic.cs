using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Character.Synth.Content.Items;
using ProjectSynth.Metronome;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace ProjectSynth.Character.Synth.States.Primary
{
    public class ThirtyNineMusic : GenericProjectileBaseState, IMetronomeListener
    {
        public static float BaseDuration = 0.3f;
        public static float DamageCoefficient = SynthStaticValues.thirtyNineMusicDamageCoefficient;

        private bool fireDouble;

        private ProjectileDamage dmg;

        public override void OnEnter()
        {
            projectilePrefab = SynthAssets.proj_ThirtyNineMusic;
            dmg = projectilePrefab.GetComponent<ProjectileDamage>();

            dmg.damageType = DamageType.Generic;

            baseDuration = BaseDuration;
            damageCoefficient = DamageCoefficient;

            fireDouble = false;

            base.OnEnter();

            if (Passive.HasMetronomePassive(characterBody))
            {
                var metro = characterBody.GetComponent<MetronomeComponent>();
                if (metro == null) return;

                //metro.StartMetronomeSequence();
                
                if (metro.CanConsumeCharge())
                {
                    ApplyBoost();
                    //metro.IncreaseMaxBounces();
                }
            }
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
            
            dmg.damageType.AddModdedDamageType(SynthDamageTypes.EncoreDamage);

            //damageCoefficient = SynthStaticValues.boosted_thirtyNineMusicDamageCoefficient;
        }
    }
}