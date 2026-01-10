using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthBuffs
    {
        // armor buff gained during roll
        public static BuffDef ArmorBuff;
        public static BuffDef EncoreDebuff;

        public static void Init(AssetBundle assetBundle)
        {
            ArmorBuff = ScriptableObject.CreateInstance<BuffDef>();
            ArmorBuff.name = "HenryArmorBuff";
            ArmorBuff.buffColor = Color.white;
            ArmorBuff.canStack = false;
            ArmorBuff.isDebuff = false;
            ArmorBuff.eliteDef = null;
            ArmorBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Croco/texBuffRegenBoostIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(ArmorBuff);

            EncoreDebuff = ScriptableObject.CreateInstance<BuffDef>();
            EncoreDebuff.name = "Encore";
            EncoreDebuff.buffColor = Color.magenta;
            EncoreDebuff.canStack = false;
            EncoreDebuff.isDebuff = true;
            EncoreDebuff.eliteDef = null;
            EncoreDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Grandparent/texBuffOverheat.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(EncoreDebuff);
        }
    }

    public static class SynthDamageTypes
    {
        public static DamageAPI.ModdedDamageType EncoreDamage;

        public static void Register()
        {
            EncoreDamage = DamageAPI.ReserveDamageType();
        }
    }
}
