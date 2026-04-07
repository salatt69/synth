using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using ProjectSynth.Modules.BaseContent.BaseStates.Metro;
using ProjectSynth.States.Synth.Metro;
using R2API;
using RoR2;
using SyncLib.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectSynth.States.Synth.Weapon
{
    public class TNM : BaseMetroSkillState
    {
        public float baseDuration = 1f;
        public float damageCoefficient = SynthStaticValues.thirtyNineMusicDamageCoefficient;
        public GameObject muzzleflashEffectPrefab = SynthAssets.vfx_tnmMuzzleFlash;
        public GameObject tracerEffectPrefab = SynthAssets.vfx_tnmTracer;
        public string muzzle = "SwingCenter";

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            ResetDuration();
            FireBullet();
        }

        private void FireBullet()
        {
            Ray aimRay = base.GetAimRay();
            //base.PlayAnimation(this.animationLayerName, this.animationStateName, this.animationPlaybackRateParam, this.duration, 0f);
            base.AddRecoil(-1f, -2f, -0.5f, 0.5f);
            base.StartAimMode(aimRay, duration, false);
            //Util.PlaySound(this.attackSoundString, base.gameObject);
            if (muzzleflashEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, muzzle, false);
            }
            if (base.isAuthority)
            {
                BulletAttack ba = new()
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    muzzleName = muzzle,
                    maxDistance = 100.0f,
                    minSpread = 0.0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    radius = 1.0f,
                    falloffModel = BulletAttack.FalloffModel.None,
                    smartCollision = true,
                    damage = damageCoefficient * this.damageStat,
                    procCoefficient = 1.0f,
                    force = 1000.0f,
                    isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
                    damageType = DamageType.Generic,
                    tracerEffectPrefab = tracerEffectPrefab,
                    //hitEffectPrefab = this.hitEffectPrefab,
                    trajectoryAimAssistMultiplier = 0.75f
                };
                ba.damageType.damageSource = DamageSource.Primary;

                if (IsMetronomeHit)
                {
                    ba.damageType.AddModdedDamageType(SynthDamageTypes.Encore);
                }

                ba.Fire();
            }
            base.characterBody.AddSpreadBloom(0.5f);
            activatorSkillSlot.DeductStock(1);

            Log.Warning(age);
        }

        private void ResetDuration()
        {
            duration = baseDuration / attackSpeedStat;
        }

        public override void Update()
        {
            base.Update();

            if (IsMetronomeHit && inputBank.skill1.down && activatorSkillSlot.stock > 0)
            {
                if (MusicSync.OnBeat() && !inputBank.skill1.justPressed && age > 0.05f)
                {
                    FireBullet();
                }
            }
            else
            {
                ResetDuration();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnMetronomeHit(BaseMetroState metroState)
        {
            base.OnMetronomeHit(metroState);
            baseDuration = activatorSkillSlot.stock * (float)MusicSync.BeatInterval + 1.0f;
            Log.Warning($"Metronome hit! New BaseDuration = {baseDuration}");
        }

        public override void OnMetronomeMiss(BaseMetroState metroState)
        {
            base.OnMetronomeMiss(metroState);
        }
    }
}
