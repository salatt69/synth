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
            animator.SetBool("Ongoing", false);
        }

        private void Update()
        {
            if (!initialized)
                return;

            ResolveTarget();

            // to prevent logic from running w/o body and metronome, bc what's the point
            if (body == null || metronome == null) return;

            SprintingCrosshairOverride(body.isSprinting);

            switch (metronome.State)
            {
                case MetronomeState.Idle:
                    {
                        break;
                    }
                case MetronomeState.Sequence:
                    {
                        UpdateSequence();
                        break;
                    }
                default:
                    break;
            }
        }

        private void UpdateSequence()
        {
            Camera cam = (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera
                : null;

            Vector2 screenPointOnBeat = RectTransformUtility.WorldToScreenPoint(cam, onBeatIndicator.position);

            bool or = RectTransformUtility.RectangleContainsScreenPoint(window, screenPointOnBeat, cam) 
                    || animator.GetCurrentAnimatorStateInfo(7).IsName("animSynthCrosshairIndicatorOnBeatEnd");

            animator.SetBool("Inside", or);
            metronome.ReportWindowState(or);

            SequenceLoop();
        }

        private void SequenceLoop()
        {
            AnimatorStateInfo asi_0 = animator.GetCurrentAnimatorStateInfo(0);

            // TODO: i hate this piece of code
            if (asi_0.normalizedTime <= metronome.nextLoopTime)
                metronome.canLoop = true;

            if (metronome.canLoop)
            {
                if (asi_0.normalizedTime >= metronome.nextLoopTime)
                {
                    metronome.canLoop = false;
                    metronome.nextLoopTime++;
                    animator.Play("animSynthCrosshairSquarePulse", 6);
                    animator.Play("animSynthCrosshairIndicatorOnBeatEnd", 7);
                }
            
                if (asi_0.normalizedTime >= metronome.nextHalfLoopTime)
                {
                    metronome.nextHalfLoopTime++;
                    animator.Play("animSynthCrosshairIndicatorOffBeatEnd", 8);
                }
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

        private void HandleSequenceUpdate()
        {
            animator.SetBool("Ongoing", true);
            animator.SetFloat("SpeedMult", metronome.sequenceSpeedMultiplier);
        }

        private void HandleMusicChange()
        {
            animator.SetBool("Ongoing", false);
            animator.SetBool("Inside", false);
        }

        private void SubscribeToMetronome(MetronomeComponent m)
        {
            if (m == null) return;

            m.OnSequenceUpdate += HandleSequenceUpdate;
            m.OnSourceChanged += HandleMusicChange;
        }

        private void UnsubscribeFromMetronome(MetronomeComponent m)
        {
            if (m == null) return;

            m.OnSequenceUpdate -= HandleSequenceUpdate;
            m.OnSourceChanged -= HandleMusicChange;
        }

        #endregion events

    }
}
