using ProjectSynth.Survivors.Synth.Achievements;
using R2API;
using RoR2;
using UnityEngine;

namespace ProjectSynth.Survivors.Synth
{
    public static class SynthUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            masterySkinUnlockableDef.cachedName = SynthMasteryAchievement.unlockableIdentifier;
            masterySkinUnlockableDef.nameToken = Modules.Tokens.GetAchievementNameToken(SynthMasteryAchievement.identifier);;
            masterySkinUnlockableDef.achievementIcon = SynthSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement");
            ContentAddition.AddUnlockableDef(masterySkinUnlockableDef);
        }
    }
}
