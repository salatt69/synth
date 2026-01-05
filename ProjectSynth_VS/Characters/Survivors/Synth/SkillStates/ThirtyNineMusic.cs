using EntityStates;
using ProjectSynth.Survivors.Synth;
using ProjectSynth.Survivors.Synth.Interfaces;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Survivors.Synth.SkillStates
{
    public class ThirtyNineMusic : GenericProjectileBaseState, IMetronomeBoostable
    {
        public static float BaseDuration = 0.7f;
        public static float DamageCoefficient = SynthStaticValues.thirtNineMusicDamageCoefficient;

        public override void OnEnter()
        {
            projectilePrefab = SynthAssets.thirtyNineMusicProjectile;

            baseDuration = BaseDuration;
            damageCoefficient = DamageCoefficient;

            base.OnEnter();

            // you will get some stats boosted and "Encore" debuff applied to an enemy you hit,
            // if you have a metronome passive AND you hit the window during a sequence
            if (MetronomeSequenceManager.TryBeginMetronomeSequence(gameObject))
                ((IMetronomeBoostable)this).ApplyMetronomeBoost();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
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

        void IMetronomeBoostable.ApplyMetronomeBoost()
        {
            MetronomeSequenceManager.SetHitResult(true);

            damageCoefficient = SynthStaticValues.boosted_thirtNineMusicDamageCoefficient;
        }
    }
}