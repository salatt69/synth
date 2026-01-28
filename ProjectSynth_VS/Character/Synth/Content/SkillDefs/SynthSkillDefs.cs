using EntityStates;
using ProjectSynth.Character.Synth.States.Primary;
using ProjectSynth.Modules;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;

namespace ProjectSynth.Character.Synth.Content.SkillDefs
{
    // Taken from https://github.com/TheTimeSweeper/EnforcerMod/blob/master/EnforcerMod_VS/Content/SkillDefs/EnforcerSkillDefs.cs
    class SynthSkillDefs
    {
        private static string Prefix => SynthSurvivor.SYNTH_PREFIX;

        public static SkillDef Primary_ThirtyNineMusic()
        {
            SkillDef tnm = Skills.CreateSkillDef2(new SkillDefInfo2
            {
                skillName = "39 Music!",
                skillNameToken = Prefix + "PRIMARY_THIRTY_NINE_MUSIC_NAME",
                skillDescriptionToken = Prefix + "PRIMARY_THIRTY_NINE_MUSIC_DESCRIPTION",
                keywordTokens = [ Prefix + "KEYWORD_FOLLOW_THE_RHYTHM" ],
                icon = Bundle.tex_ThirtyNineMusic,

                activationStateMachineName = "Weapon",
                activationState = new SerializableEntityStateType(typeof(ThirtyNineMusic)),
                interruptPriority = InterruptPriority.Any,

                baseRechargeInterval = 0,
                baseMaxStock = 0,
                rechargeStock = 0,
                requiredStock = 0,
                stockToConsume = 0,

                attackSpeedBuffsRestockSpeed = false,
                attackSpeedBuffsRestockSpeed_Multiplier = 1,

                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,

                resetCooldownTimerOnUse = false,
                beginSkillCooldownOnSkillEnd = true,
                isCooldownBlockedUntilManuallyReset = false,

                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
                canceledFromSprinting = true,
                isCombatSkill = true,

                mustKeyPress = false,
                triggeredByPressRelease = false,

                autoHandleLuminousShot = true,
                suppressSkillActivation = false,

                hideStockCount = false,
                hideCooldown = false
            });

            return tnm;
        }

        public static SkillDef Secondary_SonicBoom()
        {
            SkillDef sonicBoom = Skills.CreateSkillDef2(new SkillDefInfo2
            {
                skillName = "Sonic Boom",
                skillNameToken = Prefix + "SECONDARY_SONIC_BOOM_NAME",
                skillDescriptionToken = Prefix + "SECONDARY_SONIC_BOOM_DESCRIPTION",
                keywordTokens = [ Prefix + "KEYWORD_FOLLOW_THE_RHYTHM" ],
                icon = Bundle.tex_SonicBoom,

                activationStateMachineName = "Weapon",
                activationState = new SerializableEntityStateType(typeof(States.Secondary.SonicBoom)),
                interruptPriority = InterruptPriority.PrioritySkill,

                baseRechargeInterval = 4f,
                baseMaxStock = 1,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                attackSpeedBuffsRestockSpeed = false,
                attackSpeedBuffsRestockSpeed_Multiplier = 1,

                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,

                resetCooldownTimerOnUse = false,
                beginSkillCooldownOnSkillEnd = true,
                isCooldownBlockedUntilManuallyReset = false,

                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
                canceledFromSprinting = false,
                isCombatSkill = false,

                mustKeyPress = true,
                triggeredByPressRelease = false,

                autoHandleLuminousShot = true,
                suppressSkillActivation = false,

                hideStockCount = false,
                hideCooldown = false
            });

            return sonicBoom;

            // make note of SkillDef.InstantiateNextState()
        }
    }
}
