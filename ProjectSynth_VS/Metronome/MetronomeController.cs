using ProjectSynth.Core;
using RoR2;
using SyncLib.API;
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

    // TODO: BIGGEST TODO OF ALL TODO's!!! Completly rewrite metronome behavior to use EntityState instead of MetronomeState
    public class MetronomeController : MonoBehaviour 
    {
        public MetronomeState State = MetronomeState.Idle;

        //[Range(0.25f, 3.0f)]
        [Range(0.25f, 6.0f)]
        public float sequenceSpeedMultiplier = 1.0f;
        public float tempoShiftSpeed = 0.1f;
        public float sequenceBaseCooldownTime = 1.5f;
        public float singleRechargeTime = 0.5f;

        // charges
        public int chargeCount;
        public int currentChargeIndex;
        public float nextRechargeTime;
        public int chargesToRecharge;
        public bool rechargeAnimStarted;
        public float RechargeTimeWithoutBase => chargeCount * singleRechargeTime;

        // bounce
        public int maxBounces;
        public int bounceCount;
        public int nextLoopTime;
        public float nextHalfLoopTime;
        public bool canLoop;

        // cooldown
        public float nextAllowedTime;
        public float currentCooldownTime;

        // state
        bool canConsume;

        // sync
        double _lastBeatTime = -1.0;
        double _beatPeriod = -1.0;
        bool _hasPeriod = false;

        float _lastEmittedSpeed = 1f;

        // sync tuning
        const double SmoothAlpha = 0.15;
        const double MinDeltaAbs = 0.10;
        const double MaxGapAbs = 2.00;

        const double MinDeltaRatio = 0.30;
        const double MaxGapRatio = 2.00;

        // events
        public event Action OnSequenceUpdate;
        public event Action OnSourceChanged;

        public CharacterBody Body { get; private set; }

        private void Awake()
        {
            Body = GetComponent<CharacterBody>();
            if (Body == null)
            {
                Log.Error($"{nameof(MetronomeController)}: CharacterBody missing");
                enabled = false;
            }

            ResetSequenceDefaults();
        }

        private void Update()
        {
            if (MusicSync.OnEntry())
            {
                ResetSequenceDefaults();
                // TODO: gotta think of another way i can sync it to a new song
                OnSourceChanged?.Invoke();
                State = MetronomeState.Idle;
            }
            SyncToMusic();
        }

        #region logic

        private void SyncToMusic()
        {
            if (!MusicSync.OnBeat()) return;

            double now = Time.time;

            // arm the clock, but don't compute a period yet
            if (_lastBeatTime < 0.0)
            {
                _lastBeatTime = now;
                return;
            }

            double delta = now - _lastBeatTime;

            // absolute duplicate protection (idk if its needed)
            if (delta < MinDeltaAbs) return;

            // if already have a period, use ratio-based checks too
            if (_hasPeriod && _beatPeriod > 0.0)
            {
                // relative duplicate protection (again, idk)
                if (delta < _beatPeriod * MinDeltaRatio) return;

                // relative gap detection (usually pause/resume)
                if (delta > _beatPeriod * MaxGapRatio)
                {
                    // accept the beat time, but do not update period/speed
                    _lastBeatTime = now;
                    _hasPeriod = false;
                    return;
                }
            }
            else
            {
                // if don't have a period yet, use absolute gap detection
                if (delta > MaxGapAbs)
                {
                    _lastBeatTime = now;
                    _hasPeriod = false;
                    return;
                }
            }

            // if have a plausible delta, update period
            if (!_hasPeriod || _beatPeriod <= 0.0)
            {
                _beatPeriod = delta;
                _hasPeriod = true;
            }
            else
            {
                _beatPeriod += (delta - _beatPeriod) * SmoothAlpha;
            }

            _lastBeatTime = now;

            double speed = 1.0 / _beatPeriod;

            _lastEmittedSpeed = (float)speed;
            sequenceSpeedMultiplier = _lastEmittedSpeed;
            OnSequenceUpdate?.Invoke();

            State = MetronomeState.Sequence;
        }

        public void ResetSequenceDefaults()
        {
            _lastBeatTime = 0.0;
            _hasPeriod = false;
            _beatPeriod = 0.0;

            bounceCount = 0;
            canLoop = true;
            nextLoopTime = 1;
            nextHalfLoopTime = 0.5f;

            maxBounces = 4;

            rechargeAnimStarted = false;
            currentCooldownTime = sequenceBaseCooldownTime;
        }

        #endregion

        #region ui state

        public void ReportWindowState(bool state)
        {
            canConsume = state;
        }

        #endregion

        #region skill api

        public bool CanConsumeCharge()
        {
            return canConsume;
        }

        #endregion
    }
}