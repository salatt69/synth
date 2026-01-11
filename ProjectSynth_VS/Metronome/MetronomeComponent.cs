using ProjectSynth.Core;
using RoR2;
using System;
using UnityEngine;

namespace ProjectSynth.Metronome
{
    public enum MetronomeState
    {
        Idle,
        Sequence,
        Cooldown
    }

    public class MetronomeComponent : MonoBehaviour
    {
        public MetronomeState state = MetronomeState.Idle;

        [Range(0.25f, 3.0f)]
        public float metronomeSequenceSpeedMultiplier = 1.0f;
        public float sequenceBaseCooldownTime = 1.5f;
        public float singleRechargeTime = 0.5f;

        public bool sequenceInProcess { get; private set; }

        // charges
        public int chargeCount;
        public int currentChargeIndex;
        public float nextRechargeTime;
        public int chargesToRecharge;
        public bool rechargeAnimStarted;
        public float rechargeTimeWithoutBase => chargeCount * singleRechargeTime;

        // bounce
        public int maxBounces;
        public int bounceCount;
        public int nextBounceTime;
        public bool canBounce;

        // cooldown
        public float nextAllowedTime;
        public float currentCooldownTime;

        // state
        bool wasInside;
        bool endEarly;

        public event Action OnSequenceStarted;
        public event Action OnSequenceEnded;
        public event Action<int> OnChargeConsumed;
        public event Action<int> OnChargeRestored;

        public CharacterBody Body { get; private set; }

        private void Awake()
        {
            Body = GetComponent<CharacterBody>();
            if (Body == null)
            {
                Log.Error($"{nameof(MetronomeComponent)}: CharacterBody missing");
                enabled = false;
            }
        }

        #region logic

        public bool ShouldEndSequence()
        {
            return bounceCount >= maxBounces || (endEarly && !canBounce);
        }

        public void ResetSequenceDefaults()
        {
            bounceCount = 0;
            canBounce = true;
            nextBounceTime = 1;

            endEarly = false;
            maxBounces = 4;

            rechargeAnimStarted = false;
            currentCooldownTime = sequenceBaseCooldownTime;
        }

        public void EnterCooldownState()
        {
            state = MetronomeState.Cooldown;
            sequenceInProcess = false;

            nextAllowedTime = Time.time + currentCooldownTime;
            nextRechargeTime = Time.time + singleRechargeTime;

            OnSequenceEnded?.Invoke();
        }

        public void UpdateCooldown()
        {
            if (chargesToRecharge <= 0)
            {
                state = MetronomeState.Idle;
                return;
            }

            if (Time.time < nextRechargeTime)
                return;

            RestoreSingleCharge();
            nextRechargeTime = Time.time + singleRechargeTime;
        }

        void RestoreSingleCharge()
        {
            int restoreIndex = currentChargeIndex + 1;
            if (restoreIndex >= chargeCount) return;

            currentChargeIndex++;
            chargesToRecharge--;

            OnChargeRestored?.Invoke(restoreIndex);
        }

        void ConsumeCharge()
        {
            if (currentChargeIndex < 0) return;

            int consumedChargeIndex = currentChargeIndex;

            currentChargeIndex--;
            chargesToRecharge++;
            currentCooldownTime += singleRechargeTime;

            OnChargeConsumed?.Invoke(consumedChargeIndex);

            if (currentChargeIndex < 0)
                endEarly = true;
        }

        #endregion

        #region ui state

        public void ReportWindowState(bool inside)
        {
            wasInside = inside;
        }

        public void ReportBounce()
        {
            if (state != MetronomeState.Sequence)
                return;

            bounceCount++;
        }

        #endregion

        #region skill api

        public bool StartMetronomeSequence()
        {
            if (state != MetronomeState.Idle) return false;

            if (Time.time < nextAllowedTime) return false;

            ResetSequenceDefaults();
            state = MetronomeState.Sequence;
            sequenceInProcess = true;

            OnSequenceStarted?.Invoke();

            return true;
        }

        public void IncreaseMaxBounces(int increment = 2)
        {
            maxBounces += increment;
        }

        public bool CanConsumeCharge()
        {
            if (wasInside) ConsumeCharge();
            return wasInside;
        }

        #endregion
    }
}