using EntityStates;
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

        public static SkillDef Secondary_SonicBoom()
        {
            SkillDef sd = ScriptableObject.CreateInstance<SkillDef>();
            sd.skillName = "Sonic Boom";
            sd.skillNameToken = Prefix + "SECONDARY_SONIC_BOOM_NAME";
            sd.skillDescriptionToken = Prefix + "SECONDARY_SONIC_BOOM_DESCRIPTION";
            sd.keywordTokens = new string[] { Prefix + "KEYWORD_FOLLOW_THE_RHYTHM" };
            sd.icon = SynthAssets.secondary;

            sd.activationStateMachineName = "Weapon";
            sd.activationState = new SerializableEntityStateType(typeof(States.Secondary.SonicBoom));
            sd.interruptPriority = InterruptPriority.PrioritySkill;

            sd.baseRechargeInterval = 4f;
            sd.baseMaxStock = 1;
            sd.rechargeStock = 1;
            sd.requiredStock = 1;
            sd.stockToConsume = 1;

            sd.attackSpeedBuffsRestockSpeed = false;
            sd.attackSpeedBuffsRestockSpeed_Multiplier = 1;

            sd.fullRestockOnAssign = true;
            sd.dontAllowPastMaxStocks = false;

            sd.resetCooldownTimerOnUse = false;
            sd.beginSkillCooldownOnSkillEnd = true;
            sd.isCooldownBlockedUntilManuallyReset = false;

            sd.cancelSprintingOnActivation = false;
            sd.forceSprintDuringState = true;
            sd.canceledFromSprinting = false;
            sd.isCombatSkill = false;

            sd.mustKeyPress = true;
            sd.triggeredByPressRelease = false;

            sd.autoHandleLuminousShot = true;
            sd.suppressSkillActivation = false;

            sd.hideStockCount = false;
            sd.hideCooldown = false;
            return sd;

            // make note of SkillDef.InstantiateNextState()
        }
    }
}
