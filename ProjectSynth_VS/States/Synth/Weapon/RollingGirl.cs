using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using RoR2;
using RoR2.Skills;
using SyncLib.API;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Weapon
{
    public class RollingGirl : BaseSkillState
    {
        public float BaseDuration => (float)MusicSync.BeatInterval * (maxBeatCount + 1);
        public int maxBeatCount = 3;
        public float speedMultiplier = 1.5f;

        public float radius = 10f;
        public float forceCoefficientAtEdge = 0.5f;
        public float forceMagnitude = -4500.0f;
        public float damping = 0.5f;

        private float duration;
        private int beatCount;
        private Vector3 idealDirection;
        private readonly List<HurtBox> victimsStruck = [];
        private readonly List<HurtBox> victimsToPull = [];
        private OverlapAttack attack;
        private HitBoxGroup rollingHitBoxGroup;

        private GameObject startEffectPrefab;
        private GameObject impactEffectPrefab;
        private Transform playerTransform;

        private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;
        private readonly CharacterCameraParams ccp = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandardTall.asset").WaitForCompletion();

        public override void OnEnter()
        {
            base.OnEnter();

            StartCameraParamsOverride(0.5f);

            duration = BaseDuration;
            playerTransform = base.transform;
            beatCount = 0;
            if (isAuthority)
            {
                UpdateDirection();
            }
            base.characterDirection.forward = idealDirection;
            if (startEffectPrefab && base.characterBody)
            {
                EffectManager.SpawnEffect(startEffectPrefab, new EffectData
                {
                    origin = base.characterBody.corePosition
                }, false);
            }
            if (NetworkServer.active)
            {
                // TODO: armor buff
            }
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                rollingHitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), element => element.groupName == "Rolling");
            }
            attack = new OverlapAttack
            {
                attacker = base.gameObject,
                teamIndex = base.GetTeam(),
                damage = 0.0f,
                hitBoxGroup = rollingHitBoxGroup,
                damageType = DamageType.Shock5s,
            };

            activatorSkillSlot.SetSkillOverride(this, SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Backflip")), GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                if (base.characterBody)
                {
                    base.characterBody.isSprinting = true;
                }
                UpdateDirection();
                if (base.characterDirection)
                {
                    base.characterDirection.moveVector = idealDirection;
                    if (base.characterMotor && !base.characterMotor.disableAirControlUntilCollision)
                    {
                        base.characterMotor.rootMotion += GetIdealVelocity() * base.GetDeltaTime();
                    }
                }
                if (attack.Fire(victimsStruck))
                {
                    for (int i = 0; i < victimsStruck.Count; ++i)
                    {
                        if (!victimsToPull.Contains(victimsStruck[i]))
                        {
                            victimsToPull.Add(victimsStruck[i]);
                        }
                    }
                }

                Transform pullCenter = base.FindModelChild("BlackholeCenter");

                if (victimsToPull.Count > 0)
                {
                    for (int i = 0; i < victimsToPull.Count; i++)
                    {
                        HurtBox victimHurtBox = victimsToPull[i];
                        HealthComponent healthComponent = victimHurtBox.healthComponent;
                        if (healthComponent && healthComponent.body && victimHurtBox.transform && pullCenter && NetworkServer.active)
                        {
                            CharacterMotor characterMotor = healthComponent.body.characterMotor;
                            Vector3 centerToVictim = victimHurtBox.transform.position - pullCenter.position;
                            float distanceFactor = 1f - Mathf.Clamp(centerToVictim.magnitude / radius, 0f, 1f - forceCoefficientAtEdge);
                            centerToVictim = centerToVictim.normalized * forceMagnitude * (1f - distanceFactor);
                            Vector3 victimVelocity = Vector3.zero;
                            float victimMass = 0f;
                            if (characterMotor)
                            {
                                victimVelocity = characterMotor.velocity;
                                victimMass = characterMotor.mass;
                            }
                            else
                            {
                                Rigidbody rigidbody = healthComponent.body.rigidbody;
                                if (rigidbody)
                                {
                                    victimVelocity = rigidbody.velocity;
                                    victimMass = rigidbody.mass;
                                }
                            }
                            victimVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;
                            healthComponent.TakeDamageForce(centerToVictim - victimVelocity * damping * victimMass * distanceFactor, true, false);
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (MusicSync.OnBeat())
            {
                beatCount++;
            }

            if (beatCount >= maxBeatCount || age > duration)
            {
                base.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (age >= duration)
            {
                EndCameraParamsOverride(0.5f);
            }
            else
            {
                EndCameraParamsOverride(1.0f);
            }

            activatorSkillSlot.UnsetSkillOverride(this, SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Backflip")), GenericSkill.SkillOverridePriority.Contextual);
        }

        private void UpdateDirection()
        {
            if (base.inputBank)
            {
                Vector2 moveVector2d = Util.Vector3XZToVector2XY(base.inputBank.moveVector);
                if (moveVector2d != Vector2.zero)
                {
                    moveVector2d.Normalize();
                    idealDirection = new Vector3(moveVector2d.x, 0f, moveVector2d.y).normalized;
                }
            }
        }

        private Vector3 GetIdealVelocity()
        {
            return base.characterDirection.forward * base.characterBody.moveSpeed * speedMultiplier;
        }

        private void StartCameraParamsOverride(float transitionDuration)
        {
            if (cameraParamsOverrideHandle.isValid) return;

            cameraParamsOverrideHandle = base.cameraTargetParams.AddParamsOverride(new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = ccp.data,
                priority = 1.0f
            }, transitionDuration);
        }

        private void EndCameraParamsOverride(float transitionDuration)
        {
            if (cameraParamsOverrideHandle.isValid)
            {
                base.cameraTargetParams.RemoveParamsOverride(cameraParamsOverrideHandle, transitionDuration);
                cameraParamsOverrideHandle = default;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
