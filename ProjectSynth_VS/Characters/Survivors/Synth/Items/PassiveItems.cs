using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.Survivors.Synth
{
    internal class PassiveItems : BaseItemBodyBehavior
    {
        public static ItemDef Metronome;
        public static ItemDef Another;

        public static ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);

        public static void Initialize()
        {
            MetronomePassiveItemBehavior.CreateItem();
            AnotherPassiveItemBehavior.CreateItem();
        }

        public static bool HasMetronomePassive(CharacterBody body)
        {
            return body && body.inventory && body.inventory.GetItemCountEffective(PassiveItems.Metronome) > 0;
        }
    }
}
