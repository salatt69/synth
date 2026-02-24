using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.Hologram
{
    public class CultureShockState : BaseState
    {
        public Material overlayMaterial = SynthAssets.mat_cultureShockOverlayMain;
        public GameObject culturallyShockedEffect = SynthAssets.vfx_culturallyShocked;

        public float shockDuration = 1.5f;
        public string enterSoundString = "";
        public string exitSoundString = "";
        public float healthFractionToForceExit = 0.1f;

        private Animator animator;
        private TemporaryOverlayInstance temporaryOverlayInstance;
        private GameObject culturallyShockedInstance;
        private float healthFraction;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayShockAnimation();

            if (overlayMaterial != null)
            {
                CharacterModel model = GetModelTransform()?.GetComponent<CharacterModel>();
                if (model != null)
                {
                    temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(gameObject);
                    temporaryOverlayInstance.duration = shockDuration;
                    temporaryOverlayInstance.originalMaterial = overlayMaterial;
                    temporaryOverlayInstance.AddToCharacterModel(model);
                }
            }
            culturallyShockedInstance = UnityEngine.Object.Instantiate(culturallyShockedEffect, transform);
            culturallyShockedInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = shockDuration;
            if (characterBody.healthComponent)
            {
                healthFraction = characterBody.healthComponent.combinedHealthFraction;
            }
            if (characterBody)
            {
                characterBody.isSprinting = false;
            }
            if (characterDirection)
            {
                characterDirection.moveVector = characterDirection.forward;
            }
            if (rigidbodyMotor)
            {
                rigidbodyMotor.moveVector = Vector3.zero;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float combinedHealthFraction = characterBody.healthComponent.combinedHealthFraction;

            if (isAuthority && (fixedAge > shockDuration || healthFraction - combinedHealthFraction > healthFractionToForceExit))
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if (temporaryOverlayInstance != null)
            {
                temporaryOverlayInstance.Destroy();
            }
            if (culturallyShockedInstance)
            {
                EntityState.Destroy(culturallyShockedInstance);
            }
            Util.PlaySound(exitSoundString, gameObject);
            base.OnExit();
        }

        private void PlayShockAnimation()
        {
            string layerName = "Flinch";
            int layerIndex = this.animator.GetLayerIndex(layerName);
            if (layerIndex >= 0)
            {
                this.animator.SetLayerWeight(layerIndex, 1f);
                this.animator.Play("FlinchStart", layerIndex);
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(shockDuration);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            shockDuration = reader.ReadSingle();
        }
    }
}
