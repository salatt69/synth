using EntityStates;
using ProjectSynth.Mod;
using RoR2;
using SyncLib.API;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth
{
    public class RollingGirl : BaseCharacterMain
    {
        public float BaseDuration => (float)MusicSync.BeatInterval * (maxBeatCount + 1);
        public int maxBeatCount = 8;
        public float upwardForceMagnitude = 50.0f;
        public float awayForceMagnitude = 10.0f;
        public float speedMultiplier = 1.5f;
        public float massThreshold = 0f;
        public float carryOffsetDistance = 2.0f;
        public float pullStrength = 20f;
        public float pullMinDistance = 4f;

        public float radius = 10f;
        public float forceCoefficientAtEdge = 0.5f;
        public float forceMagnitude = -4500.0f;
        public float damping = 0.5f;

        private float duration;
        private int beatCount;
        private Vector3 idealDirection;
        private readonly List<HurtBox> victimsStruck = [];
        private readonly List<HurtBox> pull = [];
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

            Log.Warning($"Entered '{this}'!");

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
                        if (!pull.Contains(victimsStruck[i]))
                        {
                            pull.Add(victimsStruck[i]);
                        }
                    }
                }

                Transform pullCenter = base.FindModelChild("BlackholeCenter");

                if (pull.Count > 0)
                {
                    for (int i = 0; i < pull.Count; i++)
                    {
                        HurtBox victimHurtBox = pull[i];
                        HealthComponent healthComponent = victimHurtBox.healthComponent;
                        if (healthComponent && healthComponent.body && victimHurtBox.transform && pullCenter && NetworkServer.active)
                        {
                            CharacterMotor characterMotor = healthComponent.body.characterMotor;
                            Vector3 a = victimHurtBox.transform.position - pullCenter.position;
                            float num = 1f - Mathf.Clamp(a.magnitude / radius, 0f, 1f - forceCoefficientAtEdge);
                            a = a.normalized * forceMagnitude * (1f - num);
                            Vector3 a2 = Vector3.zero;
                            float d = 0f;
                            if (characterMotor)
                            {
                                a2 = characterMotor.velocity;
                                d = characterMotor.mass;
                            }
                            else
                            {
                                Rigidbody rigidbody = healthComponent.body.rigidbody;
                                if (rigidbody)
                                {
                                    a2 = rigidbody.velocity;
                                    d = rigidbody.mass;
                                }
                            }
                            a2.y += Physics.gravity.y * Time.fixedDeltaTime;
                            healthComponent.TakeDamageForce(a - a2 * damping * d * num, true, false);
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
            EndCameraParamsOverride(0.5f);
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
            return InterruptPriority.Frozen;
        }
    }
}
