using ProjectSynth.Character.Synth.Content;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    public class DivaArmingArmed : BaseDivaArmingState
    {
        public override void OnEnter()
        {
            PathToChildToEnable = "DivaVisuals/Hologram";
            ShockFieldRadius = GetComponent<ProjectileSphereTargetFinder>().lookRange;

            base.OnEnter();
        }

        public override void OnExit()
        {
            Explode();
            base.OnExit();
        }

        private void Explode()
        {
            var dmg = GetComponent<ProjectileDamage>().damage;

            DamageTypeCombo damageType = new()
            {
                damageType = DamageType.AOE,
                damageSource = DamageSource.Secondary,
                damageTypeExtended = DamageTypeExtended.Generic
            };

            new BlastAttack
            {
                radius = ShockFieldRadius,
                baseDamage = dmg * SynthStaticValues.divaExplosionCoeficient,
                damageType = damageType,
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = base.Owner,
                teamIndex = TeamIndex.Player,
                position = transform.position,
                procCoefficient = 1f,
            }.Fire();
            if (ExplosionPrefab)
            {
                EffectManager.SpawnEffect(ExplosionPrefab, new EffectData
                {
                    origin = transform.position,
                    scale = ShockFieldRadius
                }, false);
            }
        }
    }
}
