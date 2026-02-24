using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthBuffs
    {
        public static BuffDef EncoreBuff { get; private set; }

        public static void Init()
        {
            EncoreBuff = ScriptableObject.CreateInstance<BuffDef>();
            EncoreBuff.name = "Encore";
            EncoreBuff.buffColor = Color.magenta;
            EncoreBuff.canStack = false;
            EncoreBuff.isDebuff = true;
            EncoreBuff.eliteDef = null;
            EncoreBuff.iconSprite = SynthAssets.tex_icon_EncoreBuff;
            ContentAddition.AddBuffDef(EncoreBuff);
        }
    }
}
