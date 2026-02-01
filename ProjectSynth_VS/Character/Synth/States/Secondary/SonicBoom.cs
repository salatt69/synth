using EntityStates;
using ProjectSynth.Character.Synth.Content.Items;
using ProjectSynth.Metronome;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Character.Synth.States.Secondary
{
    public class SonicBoom : BaseSkillState, IMetronomeListener
    {
        public float duration = 0.3f;
        public float initialDashSpeedCoefficient = 18f;
        public float finalDashSpeedCoefficient = 1.0f;

        private float dashSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private Ray aimRay;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!isAuthority) return;

            aimRay = GetAimRay();
            forwardDirection = aimRay.direction;

            RecalculateDashSpeed();

            if (characterMotor) characterMotor.velocity = forwardDirection * dashSpeed;

            Vector3 vel = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - vel;

            // anim, sound
            // buff if needed

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
            RecalculateDashSpeed();

            if (characterDirection) characterDirection.forward = forwardDirection;

            // FOV Lerp here

            Vector3 norm = (transform.position - previousPosition).normalized;
            if (characterMotor && characterDirection && norm != Vector3.zero)
            {
                Vector3 vec = norm * dashSpeed;
                float what = Mathf.Max(Vector3.Dot(vec, forwardDirection), 0.0f);
                vec = forwardDirection * what;

                characterMotor.velocity = vec;
            }
            previousPosition = transform.position;

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void RecalculateDashSpeed()
        {
            dashSpeed = moveSpeedStat * Mathf.Lerp(initialDashSpeedCoefficient, finalDashSpeedCoefficient, fixedAge / duration);
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();

            characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }

        public void ApplyBoost()
        {
        }
    }
}
