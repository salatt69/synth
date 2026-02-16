using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    internal class Lure : BaseDivaState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                ArmingStateMachine.SetState(new DivaArmingArmed());
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}
