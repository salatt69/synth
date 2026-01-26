using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Character.Synth.Content.Items;
using ProjectSynth.Core.Patches;
using R2API;
using RoR2;
using SyncLib.API;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.Core.Hooks
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

            if (!NetworkServer.active) return;

            EncoreManager.Process();
        }

        private void HookUpdate(On.RoR2.Run.orig_Update orig, Run self)
        {
            if (MusicSync.OnBeat())
            {
                Chat.AddMessage($"{Random.Range(10000, 100000)}");
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory?.GetItemCountEffective(Passive.Metronome) >= 1)
            {
                float virtualSpeed = VirtualAttackSpeedManager.Get(sender);

                args.critTotalMult = virtualSpeed * SynthStaticValues.CRIT_CHANCE_PER_ATTACK_SPEED;
            }
            if (sender.inventory?.GetItemCountEffective(Passive.Another) >= 1)
            {
            }
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
            if (report.damageInfo.HasModdedDamageType(SynthDamageTypes.EncoreDamage))
            {
                CharacterBody victim = report.victimBody;
                CharacterBody attacker = report.attackerBody;

                if (!victim || !attacker) return;

                victim.AddBuff(SynthBuffs.EncoreDebuff);
                EncoreManager.Start(victim, attacker, 0f);
            }
        }
    }
}
