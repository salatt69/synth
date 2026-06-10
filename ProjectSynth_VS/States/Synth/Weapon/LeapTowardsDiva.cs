using EntityStates;
using ProjectSynth.Components;
using ProjectSynth.Mod;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Weapon
{
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

            Vector3 teleportPos = beacon.position;
            bool canTeleport = tracker.CanTeleportTo(teleportPos, out _, out float dist);

            if (isAuthority && !canTeleport)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (NetworkServer.active && !canTeleport)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (isAuthority)
            {
                characterBody.isSprinting = true;
                leapForce = baseLeapForce;

                Vector3 toBeacon = beacon.position - characterBody.transform.position;
                float toBeaconY = Mathf.Clamp(toBeacon.y, -rangeY, rangeY);
                directionToDiva = new Vector3(toBeacon.x, toBeaconY, toBeacon.z).normalized;
                float distanceFactor = Mathf.Clamp(dist / 75f, 0.50f, 1.0f);
                Vector3 a = directionToDiva.normalized * baseVelocityTowardsDiva * moveSpeedStat * distanceFactor;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(directionToDiva.x, 0f, directionToDiva.z).normalized * forwardVelocity;

                characterMotor.Motor.ForceUnground(0.1f);
                characterMotor.ApplyForce((a + b + b2) * leapForce);

                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
