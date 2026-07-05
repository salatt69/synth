using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace ProjectSynth.States.Synth.Weapon
{
    public class Backflip : BaseSkillState
    {
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = 0.3f;

            Vector3 direction = GetAimRay().direction;
            Vector3 directionNoY = new(direction.x, 0f, direction.z);

            Vector3 a = directionNoY.normalized * -moveSpeedStat * 7.0f;
            Vector3 b = Vector3.up * 65.0f;

            characterMotor.Motor.ForceUnground(0.1f);
            characterMotor.ApplyForce((a + b) * 50.0f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
