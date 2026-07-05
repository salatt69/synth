using EntityStates;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using RoR2;
using ProjectSynth.Character.Synth.Content;

namespace ProjectSynth.States.Synth.Weapon
{
    public class GroundSlam : BaseSkillState
    {
        public float groundStuckDuration = 0.3f;
        public float lowSlamClamp = -1.0f;
        public float highSlamClamp = -0.75f;
        public float explosionRadius = 10.0f;
        public GameObject explosionPrefab = SynthAssets.vfx_stunningPerformance;

        private float lastMoveSpeed;
        private float landedTime;
        private bool shouldExplode;

        public override void OnEnter()
        {
            base.OnEnter();

            Vector3 aimRay = GetAimRay().direction.normalized;
            Vector3 directionClamped = new(aimRay.x, Mathf.Clamp(aimRay.y, lowSlamClamp, highSlamClamp), aimRay.z);

            characterMotor.ApplyForce(directionClamped * 9000.0f, true, true);

            if (isAuthority)
            {
                characterMotor.onMovementHit += OnMovementHit;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && characterMotor)
            {
                if (shouldExplode || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround))
                {
                    //if (explosionSound)
                    //{
                    //    //EffectManager.SimpleSoundEffect();
                    //}
                    if (explosionPrefab)
                    {
                        EffectManager.SpawnEffect(explosionPrefab, new EffectData
                        {
                            origin = characterBody.footPosition,
                            scale = explosionRadius,
                            rotation = Quaternion.identity
                        }, true);
                    }
                    new BlastAttack()
                    {
                        attacker = gameObject,
                        baseDamage = damageStat * SynthStaticValues.GroundSlamDamageCoefficient,
                        baseForce = 800.0f,
                        //bonusForce
                        crit = RollCrit(),
                        //damageType
                        falloffModel = BlastAttack.FalloffModel.None,
                        procCoefficient = 1,
                        radius = explosionRadius,
                        position = characterBody.footPosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        //impactEffect
                        teamIndex = teamComponent.teamIndex
                    }.Fire();

                    landedTime = fixedAge;
                    shouldExplode = false;
                    lastMoveSpeed = characterBody.moveSpeed;
                }

                if (characterMotor.Motor.GroundingStatus.IsStableOnGround)
                {
                    characterBody.SetAimTimer(groundStuckDuration);
                    characterMotor.velocity *= 0.0f;
                    characterBody.moveSpeed = 0.0f;

                    if (fixedAge - landedTime >= groundStuckDuration)
                    {
                        characterBody.moveSpeed = lastMoveSpeed;
                        outer.SetNextStateToMain();
                    }
                }
            }
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            shouldExplode = true;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
