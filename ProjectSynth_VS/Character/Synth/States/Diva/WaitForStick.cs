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
        private ParticleSystem particleSystem;

        public override void OnEnter()
        {
            base.OnEnter();
            particleSystem = transform.Find("DivaVisuals/Trail/Particles").GetComponent<ParticleSystem>();

            if (NetworkServer.active)
            {
                ArmingStateMachine.SetState(new DivaArmingUnarmed());
            }
        }

        public override void Update()
        {
            base.Update();

            if (NetworkServer.active)
            {
                var psm = particleSystem.main;
                psm.loop = !IsStuck;
                if (IsStuck)
                {
                    outer.SetNextState(new Arm());
                }
            }
        }
    }
}
