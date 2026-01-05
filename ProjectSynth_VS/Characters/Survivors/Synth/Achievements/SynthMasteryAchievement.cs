using RoR2;
using ProjectSynth.Modules.Achievements;

namespace ProjectSynth.Survivors.Synth.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class SynthMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = SynthSurvivor.SYNTH_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = SynthSurvivor.SYNTH_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => SynthSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}