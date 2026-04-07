using EntityStates;
using ProjectSynth.Character.Synth.Content;
using R2API;
using RoR2;
using RoR2.Projectile;
using SyncLib.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;

namespace ProjectSynth.States.Synth.Diva
{
    public class StunningPerformance : BaseDivaState
    {
        private readonly GameObject stunningPerformancePrefab = SynthAssets.vfx_stunningPerformance;
        private float stunScale;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                ArmingStateMachine.SetState(new DivaArmingArmed());
            }

            var asm = ArmingStateMachine?.state as BaseDivaArmingState;
            stunScale = asm.ShockFieldRadius;
        }

        public override void Update()
        {
            base.Update();
            bool isClient = true;
            if (NetworkServer.active) isClient = false;

            if (MusicSync.OnCustomBar())
            {
                Fire(isClient);
            }
        }

        private void Fire(bool blank)
        {

            // Build a DamageTypeCombo and add the modded damage type properly.
            DamageTypeCombo damageCombo = new()
            {
                damageType = DamageType.Generic
            };
            // Extension method from DamageAPI operates on ref DamageTypeCombo
            damageCombo.AddModdedDamageType(SynthDamageTypes.CultureShock);

            // server stuns, client observes
            //var dmgType = blank ? DamageType.Silent : DamageType.Shock5s;

            new BlastAttack
            {
                radius = stunScale,
                baseDamage = 0f,
                damageType = damageCombo,
                falloffModel = BlastAttack.FalloffModel.None,
                attacker = gameObject,
                teamIndex = TeamIndex.Player,
                position = base.transform.position
            }.Fire();
            if (stunningPerformancePrefab)
            {
                EffectManager.SpawnEffect(stunningPerformancePrefab, new EffectData
                {
                    origin = base.transform.position,
                    scale = stunScale
                }, false);
            }
        }
    }
}
