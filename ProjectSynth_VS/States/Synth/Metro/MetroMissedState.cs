using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.States.Synth.Metro
{
    public class MetroMissedState : BaseMetroState
    {
        private int enterBeatIndex;
        private const int WaitForBeats = 1;

        public override void OnEnter()
        {
            base.OnEnter();

            enterBeatIndex = metro.beatIndex;
            //metro.missedSpeedMult
            //metro.missedStartedThisFrame
        }

        public override void Update()
        {
            base.Update();
            if (metro.beatIndex - enterBeatIndex >= WaitForBeats)
            {
                outer.SetNextState(new MetroWaitForInputState());
            }
        }
    }
}
