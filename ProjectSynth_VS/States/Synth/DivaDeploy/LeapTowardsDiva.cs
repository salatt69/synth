using EntityStates;
using ProjectSynth.Components;
using ProjectSynth.Mod;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.DivaDeploy
{
    // TODO: figure out how to make sprinting leap to as far as it is now
    public class LeapTowardsDiva : BaseSkillState
    {
        public float upwardVelocity = 30.0f;
        public float forwardVelocity = 4.0f;
        public float baseVelocityTowardsDiva = 8.0f;
        public float baseLeapForce = 100.0f;
        public float rangeY = 15.0f;

        private DivaTracker tracker;
        private Transform beacon;
        private Vector3 directionToDiva;
        private float leapForce;

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

            Vector3 to = beacon.position;
            bool ok = tracker.CanTeleportTo(to, out _, out float dist);

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

            if (base.isAuthority)
            {
                characterBody.isSprinting = true;
                leapForce = baseLeapForce;

                Vector3 toBeacon = beacon.position - characterBody.transform.position;
                float toBeaconY = Mathf.Clamp(toBeacon.y, -rangeY, rangeY);
                directionToDiva = new Vector3(toBeacon.x, toBeaconY, toBeacon.z).normalized;
                float distanceFactor = Mathf.Clamp(dist / 75f, 0.50f, 1.0f);
                Vector3 a = directionToDiva.normalized * baseVelocityTowardsDiva * base.moveSpeedStat * distanceFactor;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(directionToDiva.x, 0f, directionToDiva.z).normalized * forwardVelocity;

                characterMotor.Motor.ForceUnground(0.1f);
                characterMotor.ApplyForce((a + b + b2) * leapForce);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //if (!isAuthority || !canDash)
            //{
            //    outer.SetNextStateToMain();
            //    return;
            //}

            //// stop if target dies mid-flight
            //if (!beacon)
            //{
            //    outer.SetNextStateToMain();
            //    return;
            //}

            //destPos = beacon.position + Vector3.up * yOffset;

            //float t = Mathf.Clamp01(age / Mathf.Max(0.001f, dashDuration));
            //float eased = t * t * (3f - 2f * t);
            //Vector3 desired = Vector3.LerpUnclamped(startPos, destPos, eased);

            //if (characterMotor)
            //{
            //    characterMotor.Motor.SetPosition(desired);
            //    characterMotor.velocity = Vector3.zero;
            //}
            //else
            //{
            //    transform.position = desired;
            //}

            //if (t >= 1f && !consumed)
            //{
            //    //consumed = true;
            //    //tracker?.ConsumeCurrentTarget();
            //    outer.SetNextStateToMain();
            //}
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
