using R2API;
using RoR2;
using RoR2.Items;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth.Content
{
    public class SynthPassive
    {
        public static ItemDef MetroPassiveItem { get; protected set; }
        public static ItemDef AnotherPassiveItem { get; protected set; }

        public static ItemDisplayRuleDict displayRules = new(null);

        // TODO: Make it prettier, like SkillDefInfo
        public static void Initialize()
        {
            var displayRules = new ItemDisplayRuleDict(null);
            var pickupModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            var pickupIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();

            MetroPassiveItem = ScriptableObject.CreateInstance<ItemDef>();
            MetroPassiveItem.name = "SynthMetroPassiveItem";
            MetroPassiveItem.deprecatedTier = ItemTier.NoTier;
            MetroPassiveItem._itemTierDef = null;
            MetroPassiveItem.nameToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_METRO_NAME";
            MetroPassiveItem.pickupToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_METRO_PICKUP";
            MetroPassiveItem.descriptionToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_METRO_DESCRIPTION";
            MetroPassiveItem.loreToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_METRO_LORE";
            MetroPassiveItem.unlockableDef = null;
            MetroPassiveItem.pickupModelPrefab = pickupModel;
            MetroPassiveItem.pickupIconSprite = pickupIcon;
            MetroPassiveItem.isConsumed = false;
            MetroPassiveItem.hidden = true;
            MetroPassiveItem.canRemove = false;
            ItemAPI.Add(new CustomItem(MetroPassiveItem, displayRules));

            AnotherPassiveItem = ScriptableObject.CreateInstance<ItemDef>();
            AnotherPassiveItem.name = "SynthAnotherPassiveItem";
            AnotherPassiveItem.deprecatedTier = ItemTier.NoTier;
            AnotherPassiveItem._itemTierDef = null;
            AnotherPassiveItem.nameToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_ANOTHER_NAME";
            AnotherPassiveItem.pickupToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_ANOTHER_PICKUP";
            AnotherPassiveItem.descriptionToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_ANOTHER_DESCRIPTION";
            AnotherPassiveItem.loreToken = SynthSurvivor.SYNTH_PREFIX + "PASSIVE_ITEM_ANOTHER_LORE";
            AnotherPassiveItem.unlockableDef = null;
            AnotherPassiveItem.pickupModelPrefab = pickupModel;
            AnotherPassiveItem.pickupIconSprite = pickupIcon;
            AnotherPassiveItem.isConsumed = false;
            AnotherPassiveItem.hidden = true;
            AnotherPassiveItem.canRemove = false;
            ItemAPI.Add(new CustomItem(AnotherPassiveItem, displayRules));
        }

        public static bool IsMetro(CharacterBody body)
        {
            return body && body?.inventory && body.inventory?.GetItemCountEffective(SynthPassive.MetroPassiveItem) > 0;
        }
    }
}
