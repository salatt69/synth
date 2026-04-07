using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Diva
{
    internal class Arm : BaseDivaState
    {
        public static float duration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();

            //TODO: play animation
            //PlayAnimation("", "", "", duration);
        }

        public override void Update()
        {
            base.Update();
            if (NetworkServer.active && duration <= fixedAge)
            {
                outer.SetNextState(new StunningPerformance());
            }
        }
    }
}
