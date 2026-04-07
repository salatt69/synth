using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthBuffs
    {
        public static BuffDef Encore { get; private set; }

        public static void Init()
        {
            Encore = ScriptableObject.CreateInstance<BuffDef>();
            Encore.name = "Encore";
            Encore.buffColor = Color.magenta;
            Encore.canStack = true;
            Encore.isDebuff = true;
            Encore.eliteDef = null;
            Encore.iconSprite = SynthAssets.tex_icon_EncoreBuff;
            ContentAddition.AddBuffDef(Encore);
        }
    }
}
