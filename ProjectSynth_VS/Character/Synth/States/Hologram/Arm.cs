using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    internal class Arm : BaseDivaState
    {
        public static float duration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();
            Chat.AddMessage("Arming hologram...");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Arm.duration <= base.fixedAge)
            {
                outer.SetNextState(new Lure());
            }
        }
    }
}
