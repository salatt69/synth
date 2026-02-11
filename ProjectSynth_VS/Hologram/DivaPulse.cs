using SyncLib.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectSynth.Hologram
{
    public class DivaPulse : MonoBehaviour
    {
        public ParticleSystem particleSystem;

        private void Update()
        {
            if (MusicSync.OnBeat())
            {
                particleSystem.Emit(1);
            }
        }
    }
}
