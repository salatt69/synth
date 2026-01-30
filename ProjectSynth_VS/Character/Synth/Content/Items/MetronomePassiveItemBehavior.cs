using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth.Content.Items
{
    internal class MetronomePassiveItemBehavior : Passive
    {
        // Taken from: https://github.com/royal0959/R2RailgunnerPassive/blob/main/RailgunnerPassive/CustomItems.cs
        public static void CreateItem()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ItemDef Item = new ItemDef
            {
                name = "MetronomePassiveItem",
                _itemTierDef = null,

                deprecatedTier = ItemTier.NoTier,
                pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion(),
                pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion(),
                nameToken = SynthSurvivor.SYNTH_PREFIX + "IT_IS_NOT_GONNA_BE_SEEN_ANYWAY_METRONOME_NAME",
                pickupToken = SynthSurvivor.SYNTH_PREFIX + "IT_IS_NOT_GONNA_BE_SEEN_ANYWAY_METRONOME_PICKUP",
                descriptionToken = SynthSurvivor.SYNTH_PREFIX + "IT_IS_NOT_GONNA_BE_SEEN_ANYWAY_METRONOME_DESC",
                loreToken = SynthSurvivor.SYNTH_PREFIX + "IT_IS_NOT_GONNA_BE_SEEN_ANYWAY_METRONOME_LORE",
                tags =
                [
                    ItemTag.WorldUnique,
                    ItemTag.CannotCopy,
                    ItemTag.CannotDuplicate,
                    ItemTag.BrotherBlacklist,
                    ItemTag.CannotSteal,
                ],

                canRemove = false,
                hidden = true
            };
#pragma warning restore CS0618 // Type or member is obsolete

            var itemIndex = new CustomItem(Item, Passive.displayRules);
            ItemAPI.Add(itemIndex);
            Passive.Metronome = Item;
        }

        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return Passive.Metronome;
        }

        void OnEnable()
        {
        }

        void FixedUpdate()
        {
        }
    }
}
