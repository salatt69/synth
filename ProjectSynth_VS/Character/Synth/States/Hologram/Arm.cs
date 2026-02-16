using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    internal class Arm : BaseDivaState
    {
        public static float duration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();

            //TODO: play animation
            //PlayAnimation("", "", "", duration);

            Chat.AddMessage($"Playing arm anim for {duration}s");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && duration <= fixedAge)
            {
                outer.SetNextState(new Lure());
            }
        }
    }
}
