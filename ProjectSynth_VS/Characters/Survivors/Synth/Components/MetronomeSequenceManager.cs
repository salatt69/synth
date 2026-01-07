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

        // Used by skills that both can start the sequence and can be boosted by it
        public static bool TryBeginMetronomeSequence(GameObject owner)
        {
            // no paassive, no metronome sequence
            if (!PassiveItems.HasMetronomePassive(owner.GetComponent<CharacterBody>())) return false;

            // finish ongoing sequence first, or try consuming charge
            if (controller.sequenceInProcess) return controller.CanConsumeCharge();

            controller?.StartMetronomeSequence(owner);

            return false;
        }

        // Used by skills that can't start the sequence, but can be boosted by it
        public static bool TryConsumeCharge()
        {
            return controller.sequenceInProcess ? controller.CanConsumeCharge() : false;
        }

        public static void ExtendDuration()
        {
            controller?.IncreaseMaxBounces();
        }

        public static void SetWindowHitResult(bool result) { windowHitResult = result; }
        public static bool GetWindowHitResult() { return windowHitResult; }
    }
}
