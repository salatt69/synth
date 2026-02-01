using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Character.Synth.States.Override
{
    public class ExpoShift : BaseSkillState
    {
        public float dashDuration = 0.18f;
        public float yOffset = 1.0f;
        public float airMomentumMultiplier = 0.3f;

        private Vector3 startPos;
        private Vector3 destPos;
        private Vector3 savedVelocity;

        private bool targetIsProjectile;
        private bool canDash;

        private ProjectSynth.Hologram.HologramController controller;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }

            controller = characterBody.GetComponent<ProjectSynth.Hologram.HologramController>();
            if (!controller)
            {
                outer.SetNextStateToMain();
                return;
            }

            Transform target = controller.GetTargetTransform(out targetIsProjectile);
            if (!target)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (!controller.HasLineOfSightToTarget(characterBody, target))
            {
                outer.SetNextStateToMain();
                return;
            }

            startPos = transform.position;

            // Also use the same "safe point" for destination (not pivot-in-ground)
            destPos = controller.GetLosPoint(target) + Vector3.up * yOffset;

            savedVelocity = characterMotor ? characterMotor.velocity : Vector3.zero;

            if (characterMotor) characterMotor.velocity = Vector3.zero;

            canDash = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!isAuthority || !canDash)
            {
                outer.SetNextStateToMain();
                return;
            }

            float t = Mathf.Clamp01(age / Mathf.Max(0.001f, dashDuration));
            float eased = t * t * (3f - 2f * t); // smoothstep

            Vector3 desired = Vector3.LerpUnclamped(startPos, destPos, eased);

            if (characterMotor)
            {
                characterMotor.Motor.SetPosition(desired);
                characterMotor.velocity = Vector3.zero;
            }
            else
            {
                transform.position = desired;
            }

            if (t >= 1f)
            {
                if (characterMotor)
                {
                    characterMotor.velocity = targetIsProjectile
                        ? savedVelocity * airMomentumMultiplier
                        : Vector3.zero;
                }

                if (NetworkServer.active)
                    controller.ConsumeTargetAndClear();

                outer.SetNextStateToMain();
            }
        }
    }
}
