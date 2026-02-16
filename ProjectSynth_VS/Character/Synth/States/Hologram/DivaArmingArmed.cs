using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    public class DivaArmingArmed : BaseDivaArmingState
    {
        public override void OnEnter()
        {
            pathToChildToEnable = "DivaVisuals/Hologram";
            triggerRadius = 10.0f;

            base.OnEnter();
        }
    }
}
