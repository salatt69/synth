using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    internal class WaitForStick : BaseDivaState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                ArmingStateMachine.SetState(new DivaArmingUnarmed());
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && IsStuck)
            {
                outer.SetNextState(new Arm());
            }
        }
    }
}
