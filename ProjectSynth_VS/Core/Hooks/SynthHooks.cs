using ProjectSynth.Character.Synth.Content;
using R2API;
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
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }

        private void HookFixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);

            if (!NetworkServer.active) return;

            EncoreManager.Process();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SynthBuffs.ArmorBuff))
            {
                args.armorAdd += 300;
            }
            if (sender.HasBuff(SynthBuffs.EncoreDebuff))
            {
                args.moveSpeedMultAdd -= 0.5f;
                args.attackSpeedMultAdd -= 0.5f;
            }
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            if (!report.damageInfo.HasModdedDamageType(SynthDamageTypes.EncoreDamage)) return;

            CharacterBody victim = report.victimBody;
            CharacterBody attacker = report.attackerBody;

            if (!victim || !attacker) return;

            victim.AddBuff(SynthBuffs.EncoreDebuff);
            EncoreManager.Start(victim, attacker, 0f);
        }
    }
}
