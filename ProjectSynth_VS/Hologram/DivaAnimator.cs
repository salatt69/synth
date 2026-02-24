using ProjectSynth.Character.Synth.States.Hologram;
using ProjectSynth.Core;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectSynth.Hologram
{
    public class DivaAnimator : MonoBehaviour
    {
        private Animator animator;
        private EntityStateMachine armingStateMachine;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        private void Start()
        {
            GetArmingStateMachine();
            if (!animator)
            {
                Log.Warning($"{this}: No animator found!");
            }
        }

        private void GetArmingStateMachine()
        {
            if (armingStateMachine) return;

            armingStateMachine = EntityStateMachine.FindByCustomName(transform.gameObject, "Arming");
        }

        private bool IsArmed()
        {
            EntityStateMachine entityStateMachine = armingStateMachine;
            return (((entityStateMachine?.state) is BaseDivaArmingState baseDivaArmingState) ? baseDivaArmingState.ShockFieldRadius : 0f) > 1f;
        }

        private void Update()
        {
            GetArmingStateMachine();
            if (IsArmed())
            {
                animator.SetTrigger("Deploy");
            }
        }
    }
}
