using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    public class DivaArmingUnarmed : BaseDivaArmingState
    {
        public override void OnEnter()
        {
            pathToChildToEnable = "";
            triggerRadius = 0.0f;

            base.OnEnter();
        }
    }
}
