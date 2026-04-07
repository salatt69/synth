using ProjectSynth.Character.Synth.Content;
using R2API;
using RoR2;
using SyncLib.API;
using UnityEngine;
using UnityEngine.Networking;
using ProjectSynth.States;
using EntityStates;
using R2API.Networking;

namespace ProjectSynth.Mod.Hooks
{
    internal class SynthHooks
    {
        public void Initialize()
        {
            On.RoR2.Run.FixedUpdate += HookFixedUpdate;
            On.RoR2.Run.Update += HookUpdate;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }

        private void HookFixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);
        }

        private void HookUpdate(On.RoR2.Run.orig_Update orig, Run self)
        {
            //if (MusicSync.OnBeat())
            //{
            //    Chat.AddMessage($"{MusicSync.BPM:0.0}");
            //}

            if (!NetworkServer.active) return;

            EncoreRuntime.Process();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            if (report == null) return;

            if (report.damageInfo.HasModdedDamageType(SynthDamageTypes.Encore))
            {
                CharacterBody victim = report.victimBody;
                CharacterBody attacker = report.attackerBody;

                int count = victim.GetBuffCount(SynthBuffs.Encore.buffIndex);
                victim.SetBuffCount(SynthBuffs.Encore.buffIndex, count + 2);
                EncoreRuntime.TryStartSequence(victim, attacker);
            }
            if (report.damageInfo.HasModdedDamageType(SynthDamageTypes.CultureShock))
            {
                CharacterBody victim = report.victimBody;

                victim?.GetComponent<SetStateOnHurt>()?.SetCustomState(
                    EntityStateCatalog.GetStateIndex(typeof(CultureShockState)),
                    EntityStates.InterruptPriority.Stun
                    );
            }
        }
    }
}
