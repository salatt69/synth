using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.States
{
    public class CultureShockState : ShockState
    {
        public override void OnEnter()
        {
            overlayMaterial = SynthAssets.mat_cultureShockOverlayMain;
            stunVfxPrefab = SynthAssets.vfx_cultureShock;

            shockDuration = 1.5f;
            enterSoundString = AssignRandomEnterSoundString();
            exitSoundString = "";
            healthFractionToForceExit = 0.1f;

            base.OnEnter();
        }

        private string AssignRandomEnterSoundString() 
        {
            return Sounds.CultureShockStart[UnityEngine.Random.Range(0, Sounds.CultureShockStart.Length)];
        }
    }
}
