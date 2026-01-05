using System;
using ProjectSynth.Modules;
using ProjectSynth.Survivors.Synth.Achievements;

namespace ProjectSynth.Survivors.Synth
{
    public static class SynthTokens
    {
        public static void Init()
        {
            AddHenryTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Henry.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddHenryTokens()
        {
            string prefix = SynthSurvivor.SYNTH_PREFIX;

            string desc = "Henry is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Pistol is a powerful anti air, with its low cooldown and high damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease." + Environment.NewLine + Environment.NewLine;

            string subtitle = "Pop pipop pipop pop pip poo\r\nPop pipop pipop pop pip poo\r\nPop pipop pipop pop pip poo\r\nPop pipop pipop pop pip poo\r\n"
                + "Pop pipop pipop pop pip poo\r\nPop pipop pipop pop pip poo\r\nPop pipop pipop pop pip poo\r\nPop pipop pipop pop pip poo";

            string outro = "..and so he left, searching for a new identity.";
            string outroFailure = "..and so he vanished, forever a blank slate.";

            string lore = "later to be added...";

            Language.Add(prefix + "NAME", "Synth");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", subtitle);
            Language.Add(prefix + "LORE", lore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "FIRST_PASSIVE_NAME", "M1K-U");
            Language.Add(prefix + "FIRST_PASSIVE_DESCRIPTION", "eat shit");

            Language.Add(prefix + "SECOND_PASSIVE_NAME", "M1K-U v2.0");
            Language.Add(prefix + "SECOND_PASSIVE_DESCRIPTION", "fuck you looking at");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SLASH_NAME", "Sword");
            Language.Add(prefix + "PRIMARY_SLASH_DESCRIPTION", $"Swing forward for " + Tokens.DamageValueText(SynthStaticValues.swordDamageCoefficient));

            Language.Add(prefix + "PRIMARY_THIRTY_NINE_MUSIC_NAME", "39 Music!");
            Language.Add(prefix + "PRIMARY_THIRTY_NINE_MUSIC_DESCRIPTION", $"?");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_GUN_NAME", "Handgun");
            Language.Add(prefix + "SECONDARY_GUN_DESCRIPTION", Tokens.agilePrefix + $"Fire a handgun for <style=cIsDamage>{100f * SynthStaticValues.gunDamageCoefficient}% damage</style>.");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_ROLL_NAME", "Roll");
            Language.Add(prefix + "UTILITY_ROLL_DESCRIPTION", "Roll a short distance, gaining <style=cIsUtility>300 armor</style>. <style=cIsUtility>You cannot be hit during the roll.</style>");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_BOMB_NAME", "Bomb");
            Language.Add(prefix + "SPECIAL_BOMB_DESCRIPTION", $"Throw a bomb for <style=cIsDamage>{100f * SynthStaticValues.bombDamageCoefficient}% damage</style>.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(SynthMasteryAchievement.identifier), "Henry: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(SynthMasteryAchievement.identifier), "As Henry, beat the game or obliterate on Monsoon.");
            #endregion

            #region Keywords
            string boostableDesc = "Allows the skill to be shaped in a special way when used in sync with the rhythm of M1K-U metrnome.";

            Language.Add(prefix + "KEYWORD_FOLLOW_THE_RHYTHM", Tokens.KeywordText("Follow the Rhythm", boostableDesc));
            #endregion
        }
    }
}
