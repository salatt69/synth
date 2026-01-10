using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Core;
using ProjectSynth.Metronome;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSynth.Character.Synth.UI.Crosshair
{
    public class SynthCrosshairController : MonoBehaviour
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

        GameObject soundSource;
        GameObject currentBodyObject;
        CharacterBody body;
        MetronomeComponent metronome;

        HUD hud;

        private void Awake()
        {
            initialized = false;

            // dependencies
            hud = GetComponentInParent<HUD>();
            canvas = hud.mainContainer?.GetComponentInParent<Canvas>();
            animator = GetComponent<Animator>();
            window = transform.Find("Window, L")?.GetComponent<RectTransform>();
            onBeatIndicator = transform.Find("Bracket, L/OnBeat, L")?.GetComponent<RectTransform>();
            timer = transform.Find("Timer")?.GetComponent<Image>();
            chargesRoot = transform.Find("Charges");
            walkCrosshair = transform.Find("Center, Walk")?.GetComponent<Image>();
            sprintCrosshair = transform.Find("Center, Sprint")?.GetComponent<RawImage>();

            defaultTimer = timer.sprite;
            sprintTimer = transform.Find("Timer/Sprint")?.GetComponent<Image>()?.sprite;

            int chargeCount = chargesRoot.childCount;
            if (chargeCount <= 0)
            {
                Log.Error($"{nameof(SynthCrosshairController)}: Charges root has no children");
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
            // unsubscribe from whichever metronome it currently bound to
            if (metronome != null) UnsubscribeFromMetronome(metronome);
        }

        private void Update()
        {
            if (!initialized)
                return;

            ResolveTarget();

            // to prevent logic from running w/o body and metronome, bc this is the whole point
            if (body == null || metronome == null) return;

            SprintingCrosshairOverride(body.isSprinting);

            switch (metronome.state)
            {
                case MetronomeComponent.MetronomeState.Sequence:
                    UpdateSequence();
                    break;

                case MetronomeComponent.MetronomeState.Cooldown:
                    metronome.UpdateCooldown();
                    break;
            }
        }

        private void UpdateSequence()
        {
            Camera cam = (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera
                : null;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, onBeatIndicator.position);
            bool inside = RectTransformUtility.RectangleContainsScreenPoint(window, screenPoint, cam);

            animator.SetBool("Inside", inside);
            metronome.ReportWindowState(inside);

            SequenceLoop();

            if (metronome.ShouldEndSequence()) metronome.EnterCooldownState();
        }

        private void SequenceLoop()
        {
            AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);

            if (asi.normalizedTime <= metronome.nextBounceTime)
                metronome.canBounce = true;

            if (asi.normalizedTime >= metronome.nextBounceTime && metronome.canBounce)
            {
                metronome.canBounce = false;
                metronome.ReportBounce();
                metronome.nextBounceTime++;
            }
        }

        private void SprintingCrosshairOverride(bool isSprinting)
        {
            if (walkCrosshair) walkCrosshair.enabled = !isSprinting;
            if (sprintCrosshair) sprintCrosshair.enabled = isSprinting;

            if (timer) timer.sprite = (isSprinting && sprintTimer) ? sprintTimer : defaultTimer;
        }

        private void ResolveTarget()
        {
            GameObject newBodyObject = hud.targetBodyObject;

            // when respawned by seeker or chosen as a survivor to spawn as in Aerolt, this can be null.
            if (newBodyObject == null) return;

            if (newBodyObject == currentBodyObject) return;

            // if new body found, unbound all events
            if (metronome != null) UnsubscribeFromMetronome(metronome);

            currentBodyObject = newBodyObject;

            // if new body doesn't have CharacterBody, set all to null and return
            // this should never happen, but who knows
            body = currentBodyObject.GetComponent<CharacterBody>();
            if (body == null)
            {
                Log.Warning($"{nameof(SynthCrosshairController)}: How tf it doesnt have a CharacterBody?!");
                metronome = null;
                soundSource = null;
                return;
            }

            soundSource = body.gameObject;

            // same thing here, should never happen, but who knows
            metronome = body.GetComponent<MetronomeComponent>();
            if (metronome == null)
            {
                Log.Warning($"{nameof(SynthCrosshairController)}: Target body has no MetronomeComponent");
                return;
            }

            // init metronome charge config for THIS body
            metronome.chargeCount = charge.Length;
            metronome.currentChargeIndex = charge.Length - 1;

            SubscribeToMetronome(metronome);
        }

        #region events

        private void HandleSequenceStarted()
        {
            animator.SetBool("Ongoing", true);
            animator.SetFloat("SpeedMult", metronome.metronomeSequenceSpeedMultiplier);

            if (soundSource)
            {
                Util.PlaySound(
                    Sounds.MetronomeSustain,
                    soundSource,
                    "SyncMusicToTempo",
                    Modules.Math.CalculateSpeedToPitch(metronome.metronomeSequenceSpeedMultiplier)
                );
            }
        }

        private void HandleSequenceEnded()
        {
            animator.SetBool("Ongoing", false);

            if (soundSource)
                Util.PlaySound(Sounds.MetronomeSustainStop, soundSource);

            float timerAnimSpeed = 1.0f / metronome.currentCooldownTime;
            animator.SetFloat("RechargeTimerSpeedMult", timerAnimSpeed);
            animator.SetTrigger("StartTimer");
        }

        private void HandleChargeConsumed(int index)
        {
            if (charge == null || index < 0 || index >= charge.Length)
                return;

            var cg = charge[index].GetComponent<CanvasGroup>();
            if (cg) cg.alpha = 0.0f;
        }

        private void HandleChargeRestored(int restoreIndex)
        {
            if (charge == null || restoreIndex < 0 || restoreIndex >= charge.Length) return;

            var cg = charge[restoreIndex].GetComponent<CanvasGroup>();
            if (cg) cg.alpha = 1.0f;

            if (!metronome.rechargeAnimStarted)
            {
                metronome.rechargeAnimStarted = true;

                float startFrom = (float)(charge.Length - metronome.chargesToRecharge) / charge.Length;
                float animSpeed = 1.0f / metronome.rechargeTimeWithoutBase;

                animator.SetFloat("RechargeSpeedMult", animSpeed);
                animator.Play("animSynthCrosshairRecharge", 4, startFrom);
            }

            if (soundSource) Util.PlaySound(Sounds.MetronomeRecharge[restoreIndex], soundSource);
        }

        private void SubscribeToMetronome(MetronomeComponent m)
        {
            if (m == null) return;

            m.OnSequenceStarted += HandleSequenceStarted;
            m.OnChargeConsumed += HandleChargeConsumed;
            m.OnChargeRestored += HandleChargeRestored;
            m.OnSequenceEnded += HandleSequenceEnded;
        }

        private void UnsubscribeFromMetronome(MetronomeComponent m)
        {
            if (m == null) return;

            m.OnSequenceStarted -= HandleSequenceStarted;
            m.OnChargeConsumed -= HandleChargeConsumed;
            m.OnChargeRestored -= HandleChargeRestored;
            m.OnSequenceEnded -= HandleSequenceEnded;
        }

        #endregion events

    }
}
