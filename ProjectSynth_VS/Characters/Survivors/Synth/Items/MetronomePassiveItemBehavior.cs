using EntityStates;
using ProjectSynth.Survivors.Synth;
using ProjectSynth.Survivors.Synth.SkillStates;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Survivors.Synth
{
    internal class MetronomePassiveItemBehavior : PassiveItems
    {
        public static void CreateItem()
        {
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

            var itemIndex = new CustomItem(Item, PassiveItems.displayRules);
            ItemAPI.Add(itemIndex);
            PassiveItems.Metronome = Item;
        }

        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return PassiveItems.Metronome;
        }

        void OnEnable()
        {
        }

        void FixedUpdate()
        {
        }
    }
}
