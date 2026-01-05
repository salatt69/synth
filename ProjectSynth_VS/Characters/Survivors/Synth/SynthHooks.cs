using ProjectSynth.Characters.Survivors.Synth.Components;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine.Networking;

namespace ProjectSynth.Survivors.Synth
{
    internal class SynthHooks
    {
        private static HUD hud = null;

        public void Initialize()
        {
            //On.RoR2.UI.HUD.Awake += HookCustomUI;
            On.RoR2.Run.FixedUpdate += HookFixedUpdate;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void HookCustomUI(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            hud = self;

            //MetronomeSequenceController.Init(hud);
        }

        private void HookFixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);

            if (!NetworkServer.active) return;

            EncoreSequenceManager.Process();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SynthBuffs.armorBuff))
            {
                args.armorAdd += 300;
            }
            if (sender.HasBuff(SynthBuffs.encoreDebuff))
            {
                args.moveSpeedMultAdd -= 0.5f;
                args.attackSpeedMultAdd -= 0.5f;
            }
        }
    }
}
