using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    public class BaseDivaArmingState : BaseState
    {
        private Transform enabledChild;

        public string pathToChildToEnable;
        public float triggerRadius;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!string.IsNullOrEmpty(pathToChildToEnable))
            {
                enabledChild = transform.Find(pathToChildToEnable);
                enabledChild?.gameObject.SetActive(true);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            enabledChild?.gameObject.SetActive(false);
        }
    }
}
