using EntityStates;
using ProjectSynth.Components;
using ProjectSynth.Mod;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Weapon
{
    public class LeapTowardsDiva : BaseSkillState
    {
        public float upwardVelocity = 70.0f;
        public float velocityTowardsDiva = 12.0f;
        public float leapForce = 50.0f;

        private DivaTracker tracker;
        private Transform beacon;
        private Vector3 divaDirection;

        public override void OnEnter()
        {
            base.OnEnter();

            tracker = characterBody ? characterBody.GetComponent<DivaTracker>() : null;
            if (!tracker)
            {
                Log.Error($"Couldn't find DivaTracker on {characterBody?.gameObject}! [{this}] aborted.");
                outer.SetNextStateToMain();
                return;
            }

            if (!tracker.TryGetBestTarget(out beacon) || !beacon)
            {
                outer.SetNextStateToMain();
                return;
            }

            Vector3 leapTo = beacon.position;
            bool canLeap = tracker.CanLeapTo(leapTo, out _, out float dist);

            if (isAuthority && !canLeap)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (NetworkServer.active && !canLeap)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (isAuthority)
            {
                if (characterBody.isSprinting)
                    velocityTowardsDiva /= characterBody.sprintingSpeedMultiplier;
                else
                    characterBody.isSprinting = true;

                Vector3 toBeacon = beacon.position - characterBody.transform.position;
                divaDirection = new Vector3(toBeacon.x, 0.0f, toBeacon.z).normalized;

                Vector3 a = divaDirection.normalized * velocityTowardsDiva * moveSpeedStat;
                Vector3 b = Vector3.up * upwardVelocity;

                characterMotor.Motor.ForceUnground(0.1f);
                characterMotor.ApplyForce((a + b) * leapForce);

                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
