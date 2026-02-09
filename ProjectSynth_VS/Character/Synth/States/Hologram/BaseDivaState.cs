using EntityStates;
using ProjectSynth.Hologram;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    public class BaseDivaState : BaseState
    {
        protected ProjectileStickOnImpactByNormal StickOnImpact { get; private set; }
        protected virtual bool shouldStick
        {
            get
            {
                return false;
            }
        }
        protected virtual bool shouldRevertToWaitForStickOnSurfaceLost
        {
            get
            {
                return false;
            }
        }

        public bool IsStuck => StickOnImpact && StickOnImpact.stuck;

        public override void OnEnter()
        {
            base.OnEnter();
            StickOnImpact = base.GetComponent<ProjectileStickOnImpactByNormal>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && shouldRevertToWaitForStickOnSurfaceLost && !StickOnImpact.stuck)
            {
                outer.SetNextState(new WaitForStick());
            }
        }
    }
}
