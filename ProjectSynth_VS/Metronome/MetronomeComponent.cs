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

    public class MetronomeComponent : MonoBehaviour
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
                Log.Error($"{nameof(MetronomeComponent)}: CharacterBody missing");
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
            // Only do work when a beat happened
            if (!MusicSync.OnBeat()) return;

            double now = Time.time;

            // First beat: arm the clock, but don't compute a period yet
            if (_lastBeatTime < 0.0)
            {
                _lastBeatTime = now;
                return;
            }

            double delta = now - _lastBeatTime;

            // Always update last beat time at the end of beat processing
            // BUT: we might early-return for duplicate beats without updating it (see below)

            // Duplicate-beat protection (absolute)
            if (delta < MinDeltaAbs)
            {
                // ignore this beat entirely; do not update lastBeatTime
                // (prevents tiny delta -> insane speed)
                return;
            }

            // If we already have a period, use ratio-based checks too
            if (_hasPeriod && _beatPeriod > 0.0)
            {
                // Duplicate protection (relative)
                if (delta < _beatPeriod * MinDeltaRatio)
                {
                    return; // ignore duplicate
                }

                // Pause/resume gap detection (relative)
                if (delta > _beatPeriod * MaxGapRatio)
                {
                    // Treat as resync: accept the beat time, but DO NOT update period/speed
                    _lastBeatTime = now;
                    _hasPeriod = false; // require one more beat to re-lock
                    return;
                }
            }
            else
            {
                // If we don't have a period yet, use absolute gap detection
                if (delta > MaxGapAbs)
                {
                    _lastBeatTime = now;
                    _hasPeriod = false;
                    return;
                }
            }

            // We have a plausible delta: update period (initialize or smooth)
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

            // Emit only when we actually updated period
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