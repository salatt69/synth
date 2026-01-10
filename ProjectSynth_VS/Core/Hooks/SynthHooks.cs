using ProjectSynth.Character.Synth.Content;
using RoR2;
using UnityEngine.Networking;

namespace ProjectSynth.Core.Hooks
{
    internal class SynthHooks
    {
        public void Initialize()
        {
            On.RoR2.Run.FixedUpdate += HookFixedUpdate;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
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
