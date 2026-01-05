using ProjectSynth.Characters.Survivors.Synth.Components;
using ProjectSynth.Survivors.Synth;
using R2API;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSynth.Survivors.Synth
{
    [RequireComponent(typeof(SynthCrosshairController))]
    public class MetronomeSequenceManager : MonoBehaviour
    {
        private static SynthCrosshairController controller;
        private static bool windowHitResult;

        void Awake()
        {
            controller = GetComponent<SynthCrosshairController>();

            if (!controller) Log.Error($"{this}: Couldn't find a SynthCrosshairController reference!");
        }

        public static bool TryBeginMetronomeSequence(GameObject owner)
        {
            // no paassive, no metronome sequence
            if (!PassiveItems.HasMetronomePassive(owner.GetComponent<CharacterBody>())) return false;

            // finish ongoing qte first
            if (controller.sequenceInProcess) return controller.TryConsumeCharge();

            controller?.StartMetronomeSequence(owner);

            return false;
        }

        public static void ExtendDuration()
        {
            controller?.IncreaseMaxBounces();
        }

        public static void SetHitResult(bool hitResult) { windowHitResult = hitResult; }
        public static bool GetHitResult() { return windowHitResult; }
    }
}
