using ProjectSynth.Mod;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSynth.Components
{
    public class SynthOverlayController : MonoBehaviour
    {
        RectTransform window;
        RectTransform onBeatIndicator;

        Transform chargesRoot;
        Transform[] charge;

        Image timer;
        Sprite sprintTimer;
        Sprite defaultTimer;

        Image walkCrosshair;
        RawImage sprintCrosshair;

        Animator animator;
        Canvas canvas;

        bool initialized;

        GameObject currentBodyObject;
        CharacterBody body;

        SynthMetroRuntime metro;

        HUD hud;

        private float lastPhase;
        private bool beatSynced;
        private bool pendingOngoing;

        private void Awake()
        {
            initialized = false;

            hud = GetComponentInParent<HUD>();
            canvas = hud.mainContainer?.GetComponentInParent<Canvas>();
            animator = GetComponent<Animator>();

            window = transform.Find("Window, L")?.GetComponent<RectTransform>();
            onBeatIndicator = transform.Find("Bracket, L/OnBeat, L")?.GetComponent<RectTransform>();

            timer = transform.Find("Timer")?.GetComponent<Image>();
            chargesRoot = transform.Find("Charges");

            walkCrosshair = transform.Find("Center, Walk")?.GetComponent<Image>();
            sprintCrosshair = transform.Find("Center, Sprint")?.GetComponent<RawImage>();

            defaultTimer = timer ? timer.sprite : null;
            sprintTimer = transform.Find("Timer/Sprint")?.GetComponent<Image>()?.sprite;

            if (!chargesRoot)
            {
                Log.Error($"{nameof(SynthOverlayController)}: Charges root missing");
                enabled = false;
                return;
            }

            int chargeCount = chargesRoot.childCount;
            if (chargeCount <= 0)
            {
                Log.Error($"{nameof(SynthOverlayController)}: Charges root has no children");
                enabled = false;
                return;
            }

            charge = new Transform[chargeCount];
            for (int i = 0; i < chargeCount; i++)
                charge[i] = chargesRoot.GetChild(i);

            initialized = true;
        }

        private void OnDisable()
        {
            if (animator)
            {
                animator.SetBool("Ongoing", false);
                animator.SetBool("Inside", false);
            }

            if (metro)
            {
                metro.cooldownStartedThisFrame = false;
            }

            beatSynced = false;
        }

        private void Update()
        {
            if (!initialized) return;

            ResolveTarget();

            if (body == null || metro == null) return;

            //SprintingCrosshairOverride(body.isSprinting);

            if (metro.ongoing && !animator.GetBool("Ongoing"))
            {
                pendingOngoing = true;
            }
            else if (!metro.ongoing)
            {
                animator.SetBool("Ongoing", false);
                pendingOngoing = false;
                beatSynced = false;
            }

            animator.SetFloat("SpeedMult", metro.speedMult);

            SequenceLoopVisual();

            ApplyOneShotSignals();
        }

        private void ApplyOneShotSignals()
        {
            if (metro.cooldownStartedThisFrame)
            {
                animator.SetFloat("RechargeTimerSpeedMult", metro.cooldownSpeedMult);

                animator.ResetTrigger("StartTimer");
                animator.SetTrigger("StartTimer");

                metro.cooldownStartedThisFrame = false;
            }
        }

        private void SequenceLoopVisual()
        {
            float phase = metro.beatPhase01;
            bool beatJustWrapped = phase < lastPhase;

            if (pendingOngoing && beatJustWrapped)
            {
                animator.SetBool("Ongoing", true);
                pendingOngoing = false;
                TriggerBeatAnimations(phase);
            }

            bool ongoing = animator.GetBool("Ongoing");

            if (phase < lastPhase && ongoing)
            {
                TriggerBeatAnimations(phase);
            }

            lastPhase = phase;
        }

        private void TriggerBeatAnimations(float phase)
        {
            animator.Play("animSynthCrosshairSquarePulse", 6, 0f);
            animator.Play("animSynthCrosshairIndicatorOnBeatMove", 1, 0f);
            animator.Play("animSynthCrosshairIndicatorOnBeatEnd", 7, 0f);
        }

        private void SprintingCrosshairOverride(bool isSprinting)
        {
            if (walkCrosshair) walkCrosshair.enabled = !isSprinting;
            if (sprintCrosshair) sprintCrosshair.enabled = isSprinting;

            if (timer) timer.sprite = (isSprinting && sprintTimer) ? sprintTimer : defaultTimer;
        }

        private void ResolveTarget()
        {
            GameObject newBodyObject = hud ? hud.targetBodyObject : null;
            if (newBodyObject == null) return;
            if (newBodyObject == currentBodyObject) return;

            currentBodyObject = newBodyObject;

            body = currentBodyObject.GetComponent<CharacterBody>();
            if (body == null)
            {
                Log.Warning($"{nameof(SynthOverlayController)}: Target has no CharacterBody");
                if (metro) metro.body = null;
                return;
            }

            // bind new body
            if (metro == null)
            {
                metro = body.gameObject?.GetComponent<SynthMetroRuntime>();
                if (!metro) metro = body.gameObject?.AddComponent<SynthMetroRuntime>();
            }

            metro.body = body;

            animator.SetBool("Ongoing", false);
            animator.SetBool("Inside", false);

            beatSynced = false;
        }
    }
}