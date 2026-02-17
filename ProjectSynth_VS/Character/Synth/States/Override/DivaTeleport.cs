using EntityStates;
using ProjectSynth.Hologram;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Character.Synth.States.Override
{
    public class DivaTeleport : BaseSkillState
    {
        public float dashDuration = 0.18f;
        public float yOffset = 1.0f;

        private Vector3 startPos;
        private Vector3 destPos;

        private bool canDash;
        private bool consumed;

        private DivaTracker tracker;
        private Transform target;

        private bool blocked;
        private float dist;

        public override void OnEnter()
        {
            base.OnEnter();

            tracker = characterBody ? characterBody.GetComponent<DivaTracker>() : null;
            if (!tracker)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (!tracker.TryGetBestTarget(out target) || !target)
            {
                outer.SetNextStateToMain();
                return;
            }

            Vector3 to = target.position;
            bool ok = tracker.CanTeleportTo(to, out blocked, out dist);

            if (isAuthority && !ok)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (NetworkServer.active && !ok)
            {
                outer.SetNextStateToMain();
                return;
            }

            startPos = characterMotor ? characterMotor.Motor.TransientPosition : transform.position;
            canDash = true;
            consumed = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!isAuthority || !canDash)
            {
                outer.SetNextStateToMain();
                return;
            }

            // stop if target dies mid-dash
            if (!target)
            {
                outer.SetNextStateToMain();
                return;
            }

            destPos = target.position + Vector3.up * yOffset;

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

            if (t >= 1f && !consumed)
            {
                consumed = true;
                tracker?.ConsumeCurrentTarget();
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
