using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.States.Synth.Diva
{
    public class DivaArmingUnarmed : BaseDivaArmingState
    {
        public override void OnEnter()
        {
            PathToChildToEnable = "";
            ShockFieldRadius = 0.0f;

            base.OnEnter();
        }
    }
}
