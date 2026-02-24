using EntityStates;
using ProjectSynth.Character.Synth.States.Primary;
using ProjectSynth.Modules;
using R2API;
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
                icon = SynthAssets.tex_icon_ThirtyNineMusic,

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
            ContentAddition.AddSkillDef(tnm);
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
                icon = SynthAssets.tex_icon_SonicBoom,

                activationStateMachineName = "Body",
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
            ContentAddition.AddSkillDef(sonicBoom);
            return sonicBoom;
        }

        public static SkillDef Secondary_Diva()
        {
            SkillDef diva = Skills.CreateSkillDef2(new SkillDefInfo2
            {
                skillName = "Virtual Deviation",
                skillNameToken = Prefix + "SECONDARY_VIRTUAL_DEVIATION_NAME",
                skillDescriptionToken = Prefix + "SECONDARY_VIRTUAL_DEVIATION_DESCRIPTION",
                keywordTokens = [ Prefix + "KEYWORD_FOLLOW_THE_RHYTHM" ],
                icon = SynthAssets.tex_icon_Diva,

                activationStateMachineName = "Weapon2",
                activationState = new SerializableEntityStateType(typeof(States.Secondary.DeployDiva)),
                interruptPriority = InterruptPriority.Skill,

                baseRechargeInterval = 12f,
                baseMaxStock = 1,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                attackSpeedBuffsRestockSpeed = false,
                attackSpeedBuffsRestockSpeed_Multiplier = 1,

                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,

                resetCooldownTimerOnUse = false,
                beginSkillCooldownOnSkillEnd = false,
                isCooldownBlockedUntilManuallyReset = false,

                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
                canceledFromSprinting = false,
                isCombatSkill = true,

                mustKeyPress = true,
                triggeredByPressRelease = false,

                autoHandleLuminousShot = true,
                suppressSkillActivation = false,

                hideStockCount = false,
                hideCooldown = false
            });
            ContentAddition.AddSkillDef(diva);
            return diva;
        } 

        public static SkillDef Override_DivaTeleport()
        {
            SkillDef divaTeleport = Skills.CreateSkillDef2(new SkillDefInfo2
            {
                skillName = "Virtual Deviation Teleport",
                skillNameToken = Prefix + "OVERRIDE_VIRTUAL_DEVIATION_TP_NAME",
                skillDescriptionToken = Prefix + "OVERRIDE_VIRTUAL_DEVIATION_TP_DESCRIPTION",
                // keywordTokens = [ Prefix + "KEYWORD_FOLLOW_THE_RHYTHM" ],
                icon = SynthAssets.tex_icon_DivaTeleport,

                activationStateMachineName = "Weapon2",
                activationState = new SerializableEntityStateType(typeof(States.Override.DivaTeleport)),
                interruptPriority = InterruptPriority.Death,

                baseRechargeInterval = 1f,
                baseMaxStock = 1,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                attackSpeedBuffsRestockSpeed = false,
                attackSpeedBuffsRestockSpeed_Multiplier = 1,

                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,

                resetCooldownTimerOnUse = false,
                beginSkillCooldownOnSkillEnd = false,
                isCooldownBlockedUntilManuallyReset = false,

                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
                canceledFromSprinting = false,
                isCombatSkill = true,

                mustKeyPress = true,
                triggeredByPressRelease = false,

                autoHandleLuminousShot = true,
                suppressSkillActivation = false,

                hideStockCount = false,
                hideCooldown = false
            });
            ContentAddition.AddSkillDef(divaTeleport);
            return divaTeleport;
        }

        public static SkillDef Diva_Primary()
        {
            SkillDef diva = Skills.CreateSkillDef2(new SkillDefInfo2
            {
                skillName = "diva",
                skillNameToken = Prefix + "DIVA_VIRTUAL_DEVIATION_TP_NAME",
                skillDescriptionToken = Prefix + "DIVA_VIRTUAL_DEVIATION_TP_DESCRIPTION",
                // keywordTokens = [ Prefix + "KEYWORD_FOLLOW_THE_RHYTHM" ],
                icon = SynthAssets.tex_icon_DivaTeleport,

                activationStateMachineName = "Weapon",
                activationState = new SerializableEntityStateType(typeof(States.Override.DivaTeleport)),
                interruptPriority = InterruptPriority.Death,

                baseRechargeInterval = 1f,
                baseMaxStock = 1,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                attackSpeedBuffsRestockSpeed = false,
                attackSpeedBuffsRestockSpeed_Multiplier = 1,

                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,

                resetCooldownTimerOnUse = false,
                beginSkillCooldownOnSkillEnd = false,
                isCooldownBlockedUntilManuallyReset = false,

                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
                canceledFromSprinting = false,
                isCombatSkill = true,

                mustKeyPress = true,
                triggeredByPressRelease = false,

                autoHandleLuminousShot = true,
                suppressSkillActivation = false,

                hideStockCount = false,
                hideCooldown = false
            });
            ContentAddition.AddSkillDef(diva);
            return diva;
        }
    }
}
