using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using RoR2;
using RoR2.HudOverlay;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSynth.Components
{
    public class SynthSurvivorController : MonoBehaviour
    {
        public GameObject overlayPrefab;
        public string childLocatorEntry = "CrosshairExtras";

        private OverlayController overlayController;

        void OnEnable()
        {
            Log.Info("Wait for overlay decision... (1 frame)");
            StartCoroutine(EnsureOverlay());
        }

        void OnDisable()
        {
            if (overlayController != null)
            {
                //overlayController.onInstanceAdded -= OnOverlayInstanceAdded;
                //overlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
                HudOverlayManager.RemoveOverlay(overlayController);
            }
        }

        IEnumerator EnsureOverlay()
        {
            yield return null;

            overlayPrefab = DeciedeOverlay();

            OverlayCreationParams overlayParams = new()
            {
                prefab = overlayPrefab,
                childLocatorEntry = childLocatorEntry
            };
            overlayController = HudOverlayManager.AddOverlay(gameObject, overlayParams);
            //overlayController.onInstanceAdded += OnOverlayInstanceAdded;
            //overlayController.onInstanceRemove += OnOverlayInstanceRemoved;
        }

        private void OnOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {

        }

        private void OnOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {

        }

        private GameObject DeciedeOverlay()
        {
            var body = GetComponent<CharacterBody>();
            bool hasMetro = SynthPassive.IsMetro(body);
            Log.Info($"Overlay decided! Has metronome: {hasMetro}");
            return hasMetro ? SynthAssets.synthMetroOverlay : SynthAssets.synthRushOverlay;
        }
    }
}
