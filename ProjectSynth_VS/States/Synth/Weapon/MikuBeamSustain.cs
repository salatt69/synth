using EntityStates;
using EntityStates.VoidSurvivor.Weapon;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using R2API;
using RoR2;
using SyncLib.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Weapon
{
    public class MikuBeamSustain : BaseSkillState
    {
        public MikuBeamSustain() { }
        public MikuBeamSustain(CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle)
        {
            this.cameraParamsOverrideHandle = cameraParamsOverrideHandle;
        }

        public GameObject beamVfxPrefab = SynthAssets.vfx_mikuBeamEffect;
        public double baseDuration = MusicSync.BeatInterval * 12f;
        public float encoreInflictChance = 0.5f;
        public float maxDistance = 135f;
        public float damageCoefficientPerSeconds = SynthStaticValues.mikuBeamDamageCoefficient;
        public float TickRate => 24f - (float)baseDuration;

        private GameObject beamVfxInstance;
        private double duration;
        private float beamStopwatch;
        private bool hasBegunBeaming;
        private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / this.attackSpeedStat;
            //this.PlayAnimation(this.animationLayerName, this.animationEnterStateName);
            //Util.PlaySound(this.enterSoundString, base.gameObject);
            base.characterBody.SetAimTimer((float)duration + 1f);
            Log.Warning($"TickRate = {TickRate}");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.fixedAge += base.GetDeltaTime();
            if (!hasBegunBeaming)
            {
                hasBegunBeaming = true;
                //this.PlayAnimation(this.animationLayerName, this.animationEnterStateName);
                //Util.PlaySound(this.enterSoundString, base.gameObject);
                beamVfxInstance = UnityEngine.Object.Instantiate(beamVfxPrefab);
                beamVfxInstance.transform.SetParent(this.characterBody.aimOriginTransform, false);
                FireBeam();
            }
            if (beamVfxInstance)
            {
                Vector3 point = base.GetAimRay().GetPoint(maxDistance);
                if (Util.CharacterRaycast(base.gameObject, base.GetAimRay(), out RaycastHit raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
                {
                    point = raycastHit.point;
                }
                beamVfxInstance.transform.forward = point - beamVfxInstance.transform.position;
            }
            if (hasBegunBeaming)
            {
                beamStopwatch += Time.deltaTime;
                float beamCountdown = 1f / TickRate / this.attackSpeedStat;
                if (beamStopwatch > beamCountdown)
                {
                    beamStopwatch -= beamCountdown;
                    FireBeam();
                }
                if (base.isAuthority && base.characterMotor.velocity.y <= 0)
                {
				    float num = base.characterMotor.velocity.y;
				    num = Mathf.MoveTowards(num, -1, 60f * base.GetDeltaTime());
				    base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, num, base.characterMotor.velocity.z);
                }
            }
            if (this.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void FireBeam()
        {
            Ray aimRay = base.GetAimRay();
            if (base.isAuthority)
            {
                BulletAttack ba = new()
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    damage = damageCoefficientPerSeconds * this.damageStat / TickRate,
                    force = 20f,
                    muzzleName = "SwingCenter",
                    //hitEffectPrefab = ,
                    isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
                    radius = 1.5f,
                    falloffModel = BulletAttack.FalloffModel.None,
                    stopperMask = LayerIndex.world.mask,
                    procCoefficient = 1f,
                    maxDistance = this.maxDistance,
                    smartCollision = true,
                    damageType = DamageType.Generic,
                    allowTrajectoryAimAssist = false
                };
                ba.damageType.damageSource = DamageSource.Special;

                if (Util.CheckRoll(encoreInflictChance, base.characterBody.master))
                {
                    ba.damageType.AddModdedDamageType(SynthDamageTypes.Encore);
                }

                ba.Fire();
            }
        }

        public override void OnExit()
        {
            EndHoverParamsOverride(1.0f);
            if (beamVfxInstance)
            {
                VfxKillBehavior.KillVfxObject(beamVfxInstance);
            }
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.Slow80);
                base.characterBody.RemoveBuff(RoR2Content.Buffs.ElephantArmorBoost);
            }
            //this.PlayAnimation(this.animationLayerName, this.animationExitStateName);
            //Util.PlaySound(this.exitSoundString, base.gameObject);
            base.OnExit();
        }

        protected void EndHoverParamsOverride(float transitionDuration)
        {
            if (cameraParamsOverrideHandle.isValid)
            {
                base.cameraTargetParams.RemoveParamsOverride(cameraParamsOverrideHandle, transitionDuration);
                cameraParamsOverrideHandle = default;
            }
        }
    }
}
