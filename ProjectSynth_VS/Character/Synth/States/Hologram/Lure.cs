using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    internal class Lure : BaseDivaState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Chat.AddMessage("Luring...");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}
