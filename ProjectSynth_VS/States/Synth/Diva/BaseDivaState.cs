using EntityStates;
using ProjectSynth.Components;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Diva
{
    public class BaseDivaState : BaseState
    {
        private protected EntityStateMachine ArmingStateMachine { get; private set; }
        private protected ProjectileStickOnImpactByNormal StickOnImpact { get; private set; }
        protected virtual bool ShouldRevertToWaitForStickOnSurfaceLost
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
            ArmingStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Arming");
            StickOnImpact = GetComponent<ProjectileStickOnImpactByNormal>();

            // TODO: play sound
            Util.PlaySound("", gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && ShouldRevertToWaitForStickOnSurfaceLost && !StickOnImpact.stuck)
            {
                outer.SetNextState(new WaitForStick());
            }
        }
    }
}
