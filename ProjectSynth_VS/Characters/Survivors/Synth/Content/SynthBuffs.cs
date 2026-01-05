using ProjectSynth.Modules;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Survivors.Synth
{
    public static class SynthBuffs
    {
        // armor buff gained during roll
        public static BuffDef armorBuff;
        public static BuffDef encoreDebuff;

        public static void Init(AssetBundle assetBundle)
        {
            armorBuff = ScriptableObject.CreateInstance<BuffDef>();
            armorBuff.name = "HenryArmorBuff";
            armorBuff.buffColor = Color.white;
            armorBuff.canStack = false;
            armorBuff.isDebuff = false;
            armorBuff.eliteDef = null;
            armorBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Croco/texBuffRegenBoostIcon.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(armorBuff);

            encoreDebuff = ScriptableObject.CreateInstance<BuffDef>();
            encoreDebuff.name = "Encore";
            encoreDebuff.buffColor = Color.magenta;
            encoreDebuff.canStack = true;
            encoreDebuff.isDebuff = true;
            encoreDebuff.eliteDef = null;
            encoreDebuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Grandparent/texBuffOverheat.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(encoreDebuff);
        }
    }
}
