using ProjectSynth.Survivors.Synth;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSynth.Characters.Survivors.Synth.Components
{
    public class SynthCrosshairController : MonoBehaviour
    {
        enum MetronomeState 
        {
            Idle,
            Sequence,
            Cooldown
        }
        MetronomeState state = MetronomeState.Idle;

        [Range(0.25f, 3.0f)]
        public float metronomeSeqruenceSpeedMultiplier = 1.0f;
        public float sequenceBaseCooldownTime = 1.5f;
        public float singleRechargeTime = 0.5f;
        public bool sequenceInProcess { get; protected set; }

        RectTransform window;
        RectTransform onBeatIndicator;
        RectTransform offBeatIndicator;

        Transform chargesRoot;
        Transform[] charge;
        int chargeCount;
        int currentChargeIndex;
        float nextRechargeTime;
        int chargesToRecharge;
        bool rechargeAnimStarted;
        float rechargeTimeWithoutBase => chargeCount * singleRechargeTime;

        Image timer;
        Sprite sprintTimer;
        Sprite defaultTimer;

        Image walkCrosshair;
        RawImage sprintCrosshair;

        Animator animator;
        Canvas canvas;

        bool wasInside;
        bool initialized;
        bool endEarly;

        GameObject soundSource;
        CharacterBody body;

        int maxBounces;
        int bounceCount;
        int nextBounceTime;
        bool canBounce;

        float nextAllowedTime;
        float currentCooldownTime;

        private void Awake()
        {
            // UI references
            walkCrosshair = transform.Find("Center, Walk")?.GetComponent<Image>();
            sprintCrosshair = transform.Find("Center, Sprint")?.GetComponent<RawImage>();
            window = transform.Find("Window, L")?.GetComponent<RectTransform>();
            onBeatIndicator = transform.Find("Bracket, L/OnBeat, L")?.GetComponent<RectTransform>();
            offBeatIndicator = transform.Find("Bracket, L/OffBeat, L")?.GetComponent<RectTransform>();
            timer = transform.Find("Timer")?.GetComponent<Image>();
            chargesRoot = transform.Find("Charges");
            animator = GetComponent<Animator>();

            if (!walkCrosshair || !sprintCrosshair || !window || !onBeatIndicator 
                || !offBeatIndicator || !timer || !chargesRoot || !animator)
            {
                Log.Error($"{this}: Missing crosshair references on instance!");
                initialized = false;
            }

            defaultTimer = timer.sprite;
            sprintTimer = transform.Find("Timer/Sprint")?.GetComponent<Image>().sprite;

            // Canvas
            HUD hud = GetComponentInParent<HUD>();
            canvas = hud.mainContainer.GetComponentInParent<Canvas>();

            if (!canvas)
            {
                Log.Error($"{this}: Could not find parent Canvas from HUD!");
                initialized = false;
            }

            // CharacterBody
            body = hud?.targetBodyObject.GetComponent<CharacterBody>();

            if (!body) 
            { 
                Log.Error($"{this}: Could not find CharacterBody on parent HUD's target body object!");
                initialized = false;
            }

            // Activations
            chargeCount = chargesRoot.childCount;
            charge = new Transform[chargeCount];
            for (int i = 0; i < chargeCount; i++)
            {
                charge[i] = chargesRoot.GetChild(i);
            }
            currentChargeIndex = charge.Length - 1;

            initialized = true;
        }

        void Update()
        {
            if (!initialized) return;

            SprintingCrosshairOverride(body.isSprinting);

            switch (state) 
            {
                case MetronomeState.Sequence:
                    UpdateSequence();
                    break;

                case MetronomeState.Cooldown:
                    UpdateCooldown();
                    break;
            }
        }

        public void StartMetronomeSequence(GameObject owner)
        {
            if (Time.time < nextAllowedTime) return;

            ResetSequenceDefaults();

            state = MetronomeState.Sequence;
            sequenceInProcess = true;
            animator.SetBool("Ongoing", true);

            animator.SetFloat("SpeedMult", metronomeSeqruenceSpeedMultiplier);

            soundSource = owner;
            Util.PlaySound(Sounds.MetronomeSustain, soundSource, "SyncMusicToTempo", ProjectSynth.Modules.Math.CalculateSpeedToPitch(metronomeSeqruenceSpeedMultiplier));
        }

        public void IncreaseMaxBounces()
        {
            maxBounces += 2;
        }

        public bool CanConsumeCharge()
        {
            if (wasInside) ConsumeCharge();
            return wasInside;
        }

        private void UpdateSequence()
        {
            Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera
                : null;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, onBeatIndicator.position);

            bool inside = RectTransformUtility.RectangleContainsScreenPoint(window, screenPoint, cam);

            // i call it: THE BEST line of code i wrote in my entire life
            // it toggles the glow only when the indicator enters or exits the window.
            if (inside != wasInside) animator.SetBool("Inside", inside);

            wasInside = inside;

            SequenceLoop();

            if (ShouldEndSequence())
            {
                EnterCooldownState();
            }
        }

        private void SequenceLoop()
        {
            AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);

            // if new loop just started, allow bounce
            if (asi.normalizedTime <= nextBounceTime) canBounce = true;

            // when the loop ends, dissallow bounce until next loop and add bounceCount
            if (asi.normalizedTime >= nextBounceTime && canBounce)
            {
                canBounce = false;
                bounceCount++;
                nextBounceTime++;
            }
        }

        private void EnterCooldownState()
        {
            state = MetronomeState.Cooldown;
            sequenceInProcess = false;
            animator.SetBool("Ongoing", false);

            Util.PlaySound(Sounds.MetronomeSustainStop, soundSource);

            float timerAnimSpeed = 1.0f / currentCooldownTime;
            animator.SetFloat("RechargeTimerSpeedMult", timerAnimSpeed);
            animator.SetTrigger("StartTimer");

            nextAllowedTime = Time.time + currentCooldownTime;
            nextRechargeTime = Time.time + singleRechargeTime;
        }

        private void UpdateCooldown()
        {
            if (chargesToRecharge <= 0)
            {
                state = MetronomeState.Idle;
                return;
            }

            if (Time.time < nextRechargeTime) return;

            RestoreSingleCharge();

            nextRechargeTime = Time.time + singleRechargeTime;
        }

        private void SprintingCrosshairOverride(bool isSprinting)
        {
            this.walkCrosshair.enabled = !isSprinting;
            this.sprintCrosshair.enabled = isSprinting;

            this.timer.sprite = isSprinting ? this.sprintTimer : this.defaultTimer;
        }

        private bool ShouldEndSequence()
        {
            return bounceCount >= maxBounces || (endEarly && !canBounce);
        }

        private void ResetSequenceDefaults()
        {
            bounceCount = 0;
            canBounce = true;
            nextBounceTime = 1;

            endEarly = false;

            maxBounces = 4;

            rechargeAnimStarted = false;

            currentCooldownTime = sequenceBaseCooldownTime;
        }

        private void RestoreSingleCharge()
        {
            int restoreIndex = currentChargeIndex + 1;

            if (restoreIndex >= charge.Length) return;

            charge[restoreIndex].GetComponent<CanvasGroup>().alpha = 1.0f;

            if (!rechargeAnimStarted)
            {
                rechargeAnimStarted = true;
                float startFrom = (float)(chargeCount - chargesToRecharge) / chargeCount;
                float animSpeed = 1.0f / rechargeTimeWithoutBase;
                animator.SetFloat("RechargeSpeedMult", animSpeed);
                animator.Play("animSynthCrosshairRecharge", 4, startFrom);
            }

            currentChargeIndex++;
            chargesToRecharge--;

            Util.PlaySound(Sounds.MetronomeRecharge[restoreIndex], soundSource);
        }

        private void ConsumeCharge()
        {
            if (currentChargeIndex < 0) return;

            charge[currentChargeIndex].GetComponent<CanvasGroup>().alpha = 0;
            currentChargeIndex--;

            chargesToRecharge++;
            currentCooldownTime += singleRechargeTime;

            if (currentChargeIndex < 0) endEarly = true;
        }
    }
}
