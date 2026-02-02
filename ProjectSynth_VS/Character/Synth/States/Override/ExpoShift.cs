using EntityStates;
using ProjectSynth.Hologram;
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
        private bool targetIsProjectile;
        private bool canDash;

        private Hologram.HologramController controller;

        public override void OnEnter()
        {
            base.OnEnter();

            controller = characterBody.GetComponent<HologramController>();
            Transform target = controller.GetTargetTransform(out targetIsProjectile);
            if (!target) { outer.SetNextStateToMain(); return; }

            // client UX gate
            if (isAuthority && !controller.AllowedToTeleport)
            {
                outer.SetNextStateToMain();
                return;
            }

            Vector3 from = characterBody.inputBank ? characterBody.inputBank.aimOrigin : characterBody.corePosition;
            Vector3 to = controller.GetLOSPoint(target);
            float dist = Vector3.Distance(from, to);

            bool inRange = controller.IsValidDistance(dist);
            bool hasLos = controller.HasLineOfSightToTarget(characterBody, target);

            if (!(inRange && hasLos))
            {
                outer.SetNextStateToMain();
                return;
            }

            startPos = transform.position;
            destPos = controller.GetLOSPoint(target) + Vector3.up * yOffset;
            canDash = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!isAuthority || !canDash) return;

            float t = Mathf.Clamp01(age / Mathf.Max(0.001f, dashDuration));
            float eased = t * t * (3f - 2f * t);

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
                if (NetworkServer.active)
                {
                    controller.ConsumeTargetAndClear();
                }
                outer.SetNextStateToMain();
            }
        }
    }
}
