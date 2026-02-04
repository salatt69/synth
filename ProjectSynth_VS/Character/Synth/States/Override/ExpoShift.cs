using EntityStates;
using ProjectSynth.Hologram;
using RoR2;
using UnityEngine;

namespace ProjectSynth.Character.Synth.States.Override
{
    public class ExpoShift : BaseSkillState
    {
        public float dashDuration = 0.18f;
        public float yOffset = 1.0f;
        public float airMomentumMultiplier = 0.3f;

        private Vector3 startPos;
        private Vector3 destPos;
        private bool canDash;

        private ExpoTracker tracker;
        private Transform target;
        private bool usedProjectile;

        public override void OnEnter()
        {
            base.OnEnter();

            tracker = characterBody.GetComponent<ExpoTracker>();
            if (!tracker)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (!tracker.TryGetBestTarget(out target, out usedProjectile) || !target)
            {
                outer.SetNextStateToMain();
                return;
            }

            bool blocked;
            float dist;
            bool canTeleport = tracker.CanTeleportTo(target.position, out blocked, out dist);

            if (!canTeleport)
            {
                outer.SetNextStateToMain();
                return;
            }

            startPos = transform.position;
            destPos = target.position + Vector3.up * yOffset;
            canDash = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!canDash)
            {
                outer.SetNextStateToMain();
                return;
            }

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
                // SP: consume and destroy the target we teleported to, and unset override
                if (tracker) tracker.ConsumeAndDestroyTarget(usedProjectile);

                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
