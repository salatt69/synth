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
        private EntityStateMachine mainSateMachine;
        private Transform projectileTransform;
        private ProjectileStickOnImpactByNormal stickOnImpact;
        private bool deployed;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            GetMainStateMachine();
            if (!animator)
            {
                Log.Warning($"{this}: No animator found!");
            }
        }

        private void GetMainStateMachine()
        {
            if (mainSateMachine && stickOnImpact) return;

            ProjectileGhostController ghostController = GetComponent<ProjectileGhostController>();
            if (!ghostController) return;

            projectileTransform = ghostController.authorityTransform;
            if (!projectileTransform) return;

            if (!mainSateMachine)
                mainSateMachine = EntityStateMachine.FindByCustomName(projectileTransform.gameObject, "Main");

            if (!stickOnImpact)
                stickOnImpact = projectileTransform.GetComponent<ProjectileStickOnImpactByNormal>();
        }

        private void Update()
        {
            if (!animator) return;

            // Keep trying until authorityTransform exists (ghost often initializes before it)
            if (!projectileTransform || !stickOnImpact) GetMainStateMachine();

            if (!deployed && stickOnImpact && stickOnImpact.stuck)
            {
                deployed = true;
                animator.SetTrigger("Deploy");
            }
        }
    }
}
