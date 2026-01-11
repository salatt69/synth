using R2API;
using RoR2;
using RoR2.Items;

namespace ProjectSynth.Character.Synth.Content.Items
{
    internal class Passive : BaseItemBodyBehavior
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
            return body && body.inventory && body.inventory.GetItemCountEffective(Passive.Metronome) > 0;
        }
    }
}
