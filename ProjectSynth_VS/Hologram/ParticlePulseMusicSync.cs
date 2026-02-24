using ProjectSynth.Core;
using RoR2;
using SyncLib.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectSynth.Hologram
{
    public class ParticlePulseMusicSync : MonoBehaviour
    {
        public enum SyncType
        {
            OnBeat = 0,
            OnBar = 1,
            OnCustomBar = 2,
            OnNthBeat = 3,
            OnEntry = 4,
            OnExit = 5,
        }

        public ParticleSystem particleSystem;
        public SyncType syncType;

        private Func<bool> cachedSyncFunction;
        private SyncType lastSyncType;

        private void Awake()
        {
            bool flag1 = particleSystem.main.maxParticles >= 4;
            bool flag2 = particleSystem.emission.burstCount >= 1;
            bool flag3 = particleSystem.emission.rateOverTime.constant <= 0;
            bool flag4 = particleSystem.emission.rateOverDistance.constant <= 0;

            if (!flag1 || !flag2 || !flag3 || !flag4)
            {
                Log.Warning($"{this} -> For ParticlePulseMusicSync to work properly, make sure following conditions are met:" +
                    "\n [particleSystem.main.maxParticles >= 4]" +
                    "\n [particleSystem.emission.burstCount >= 1]" +
                    "\n [particleSystem.emission.rateOverTime.constant <= 0]" +
                    "\n [particleSystem.emission.rateOverDistance.constant <= 0]" +
                    "\nNot following them will not break the component, you just might not see what you expected to see. But if you know what you are doing, then i apologise.");
            }

            BuildDelegate();
        }

        private void Update()
        {
            if (syncType != lastSyncType)
            {
                BuildDelegate();
            }

            if (cachedSyncFunction.Invoke())
            {
                particleSystem.Emit(1);
            }
        }

        private void BuildDelegate()
        {
            switch (syncType)
            {
                case SyncType.OnBeat:
                    cachedSyncFunction = MusicSync.OnBeat;
                    break;
                case SyncType.OnBar:
                    cachedSyncFunction = MusicSync.OnBar;
                    break;
                case SyncType.OnCustomBar:
                    cachedSyncFunction = MusicSync.OnCustomBar;
                    break;
                case SyncType.OnNthBeat:
                    cachedSyncFunction = () => MusicSync.OnNthBeat(4);
                    break;
                case SyncType.OnEntry:
                    cachedSyncFunction = MusicSync.OnEntry;
                    break;
                case SyncType.OnExit:
                    cachedSyncFunction = MusicSync.OnExit;
                    break;
            }
            lastSyncType = syncType;
        }
    }
}