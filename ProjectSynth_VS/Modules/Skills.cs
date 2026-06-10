using EntityStates;
using ProjectSynth.Mod;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace ProjectSynth.Modules
{
    internal static class Skills
    {
        #region genericskills
        public static void CreateSkillFamilies(GameObject targetPrefab) => CreateSkillFamilies(targetPrefab, SkillSlot.Primary, SkillSlot.Secondary, SkillSlot.Utility, SkillSlot.Special);
        /// <summary>
        /// Create in order the GenericSkills for the skillslots desired, and create skillfamilies for them.
        /// </summary>
        /// <param name="targetPrefab">Body prefab to add GenericSkills</param>
        /// <param name="slots">Order of slots to add to the body prefab.</param>
        public static void CreateSkillFamilies(GameObject targetPrefab, params SkillSlot[] slots)
        {
            SkillLocator skillLocator = targetPrefab.GetComponent<SkillLocator>();

            for (int i = 0; i < slots.Length; i++)
            {
                switch (slots[i])
                {
                    case SkillSlot.Primary:
                        skillLocator.primary = CreateGenericSkillWithSkillFamily(targetPrefab, "Primary");
                        break;
                    case SkillSlot.Secondary:
                        skillLocator.secondary = CreateGenericSkillWithSkillFamily(targetPrefab, "Secondary");
                        break;
                    case SkillSlot.Utility:
                        skillLocator.utility = CreateGenericSkillWithSkillFamily(targetPrefab, "Utility");
                        break;
                    case SkillSlot.Special:
                        skillLocator.special = CreateGenericSkillWithSkillFamily(targetPrefab, "Special");
                        break;
                    case SkillSlot.None:
                        break;
                }
            }
        }

        public static void ClearGenericSkills(GameObject targetPrefab)
        {
            foreach (GenericSkill obj in targetPrefab.GetComponentsInChildren<GenericSkill>())
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        public static GenericSkill CreateGenericSkillWithSkillFamily(GameObject targetPrefab, SkillSlot skillSlot, bool hidden = false)
        {
            SkillLocator skillLocator = targetPrefab.GetComponent<SkillLocator>();
            switch (skillSlot)
            {
                case SkillSlot.Primary:
                    return skillLocator.primary = CreateGenericSkillWithSkillFamily(targetPrefab, "Primary", hidden);
                case SkillSlot.Secondary:
                    return skillLocator.secondary = CreateGenericSkillWithSkillFamily(targetPrefab, "Secondary", hidden);
                case SkillSlot.Utility:
                    return skillLocator.utility = CreateGenericSkillWithSkillFamily(targetPrefab, "Utility", hidden);
                case SkillSlot.Special:
                    return skillLocator.special = CreateGenericSkillWithSkillFamily(targetPrefab, "Special", hidden);
                case SkillSlot.None:
                    Log.Error("Failed to create GenericSkill with skillslot None. If making a GenericSkill outside of the main 4, specify a familyName, and optionally a genericSkillName");
                    return null;
            }
            return null;
        }
        public static GenericSkill CreateGenericSkillWithSkillFamily(GameObject targetPrefab, string familyName, bool hidden = false) => CreateGenericSkillWithSkillFamily(targetPrefab, familyName, familyName, hidden);
        public static GenericSkill CreateGenericSkillWithSkillFamily(GameObject targetPrefab, string genericSkillName, string familyName, bool hidden = false)
        {
            GenericSkill skill = targetPrefab.AddComponent<GenericSkill>();
            skill.skillName = genericSkillName;
            skill.hideInCharacterSelect = hidden;
            skill.hideInLoadoutSelect = hidden;

            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = targetPrefab.name + familyName + "Family";
            newFamily.variants = new SkillFamily.Variant[0];

            skill._skillFamily = newFamily;

            ContentAddition.AddSkillFamily(newFamily);

            return skill;
        }
        #endregion

        #region skillfamilies

        //everything calls this
        public static void AddSkillToFamily(SkillFamily skillFamily, SkillDef skillDef, UnlockableDef unlockableDef = null)
        {
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);

            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = unlockableDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        public static void AddSkillsToFamily(SkillFamily skillFamily, params SkillDef[] skillDefs)
        {
            foreach (SkillDef skillDef in skillDefs)
            {
                AddSkillToFamily(skillFamily, skillDef);
            }
        }

        public static void AddPrimarySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().primary.skillFamily, skillDefs);
        }
        public static void AddSecondarySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().secondary.skillFamily, skillDefs);
        }
        public static void AddUtilitySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().utility.skillFamily, skillDefs);
        }
        public static void AddSpecialSkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            AddSkillsToFamily(targetPrefab.GetComponent<SkillLocator>().special.skillFamily, skillDefs);
        }

        /// <summary>
        /// pass in an amount of unlockables equal to or less than skill variants, null for skills that aren't locked
        /// <code>
        /// AddUnlockablesToFamily(skillLocator.primary, null, skill2UnlockableDef, null, skill4UnlockableDef);
        /// </code>
        /// </summary>
        public static void AddUnlockablesToFamily(SkillFamily skillFamily, params UnlockableDef[] unlockableDefs)
        {
            for (int i = 0; i < unlockableDefs.Length; i++)
            {
                SkillFamily.Variant variant = skillFamily.variants[i];
                variant.unlockableDef = unlockableDefs[i];
                skillFamily.variants[i] = variant;
            }
        }
        #endregion

        #region skilldefs
        public static PassiveItemSkillDef CreateSkillDef(PassiveItemSkillDefInfo skillDefInfo)
        {
            return CreateSkillDef<PassiveItemSkillDef>(skillDefInfo);
        }

        public static SkillDef CreateSkillDef(SkillDefInfo skillDefInfo)
        {
            return CreateSkillDef<SkillDef>(skillDefInfo);
        }

        public static SteppedSkillDef CreateSkillDef(SteppedSkillDefInfo skillDefInfo)
        {
            return CreateSkillDef<SteppedSkillDef>(skillDefInfo);
        }

        public static T CreateSkillDef<T>(SkillDefInfo skillDefInfo) where T : SkillDef
        {
            T skillDef = ScriptableObject.CreateInstance<T>();
            skillDef.skillName = skillDefInfo.skillName;
            (skillDef as ScriptableObject).name = skillDefInfo.skillName;
            skillDef.skillNameToken = skillDefInfo.skillNameToken;
            skillDef.skillDescriptionToken = skillDefInfo.skillDescriptionToken;
            skillDef.keywordTokens = skillDefInfo.keywordTokens;
            skillDef.icon = skillDefInfo.icon;

            skillDef.activationStateMachineName = skillDefInfo.activationStateMachineName;
            skillDef.activationState = skillDefInfo.activationState;
            skillDef.interruptPriority = skillDefInfo.interruptPriority;

            skillDef.baseRechargeInterval = skillDefInfo.baseRechargeInterval;
            skillDef.baseMaxStock = skillDefInfo.baseMaxStock;
            skillDef.rechargeStock = skillDefInfo.rechargeStock;
            skillDef.requiredStock = skillDefInfo.requiredStock;
            skillDef.stockToConsume = skillDefInfo.stockToConsume;

            skillDef.attackSpeedBuffsRestockSpeed = skillDefInfo.attackSpeedBuffsRestockSpeed;
            skillDef.attackSpeedBuffsRestockSpeed_Multiplier = skillDefInfo.attackSpeedBuffsRestockSpeed_Multiplier;

            skillDef.fullRestockOnAssign = skillDefInfo.fullRestockOnAssign;
            skillDef.dontAllowPastMaxStocks = skillDefInfo.dontAllowPastMaxStocks;

            skillDef.resetCooldownTimerOnUse = skillDefInfo.resetCooldownTimerOnUse;
            skillDef.beginSkillCooldownOnSkillEnd = skillDefInfo.beginSkillCooldownOnSkillEnd;
            skillDef.isCooldownBlockedUntilManuallyReset = skillDefInfo.isCooldownBlockedUntilManuallyReset;

            skillDef.cancelSprintingOnActivation = skillDefInfo.cancelSprintingOnActivation;
            skillDef.forceSprintDuringState = skillDefInfo.forceSprintDuringState;
            skillDef.canceledFromSprinting = skillDefInfo.canceledFromSprinting;
            skillDef.isCombatSkill = skillDefInfo.isCombatSkill;

            skillDef.mustKeyPress = skillDefInfo.mustKeyPress;
            skillDef.triggeredByPressRelease = skillDefInfo.triggeredByPressRelease;

            skillDef.autoHandleLuminousShot = skillDefInfo.autoHandleLuminousShot;
            skillDef.suppressSkillActivation = skillDefInfo.suppressSkillActivation;

            skillDef.hideStockCount = skillDefInfo.hideStockCount;
            skillDef.hideCooldown = skillDefInfo.hideCooldown;

            if (skillDefInfo is PassiveItemSkillDefInfo passiveItemSkillDefInfo)
            {
                var passiveItemSkillDef = skillDef as PassiveItemSkillDef;
                passiveItemSkillDef.passiveItem = passiveItemSkillDefInfo.passiveItem;
            }
            else if (skillDefInfo is SteppedSkillDefInfo steppedSkillDefInfo)
            {
                var steppedSkillDef = skillDef as SteppedSkillDef;
                steppedSkillDef.stepCount = steppedSkillDefInfo.stepCount;
                steppedSkillDef.stepGraceDuration = steppedSkillDefInfo.stepGraceDuration;
                steppedSkillDef.stepResetTimer = steppedSkillDefInfo.stepResetTimer;
            }

            return skillDef;
        }
        #endregion skilldefs
    }

    internal class SkillDefInfo
    {
        public string skillName;
        public string skillNameToken;
        public string skillDescriptionToken;
        public string[] keywordTokens;
        public Sprite icon;

        public string activationStateMachineName;
        public SerializableEntityStateType activationState;
        public InterruptPriority interruptPriority;

        public float baseRechargeInterval = 1f;
        public int baseMaxStock = 1;
        public int rechargeStock = 1;
        public int requiredStock = 1;
        public int stockToConsume = 1;

        public bool attackSpeedBuffsRestockSpeed = false;
        public float attackSpeedBuffsRestockSpeed_Multiplier = 1.0f;

        public bool resetCooldownTimerOnUse = false;
        public bool fullRestockOnAssign = true;
        public bool dontAllowPastMaxStocks = false;
        public bool beginSkillCooldownOnSkillEnd = true;
        public bool isCooldownBlockedUntilManuallyReset = false;

        public bool cancelSprintingOnActivation = true;
        public bool forceSprintDuringState = false;
        public bool canceledFromSprinting = true;

        public bool isCombatSkill = false;
        public bool mustKeyPress = true;
        public bool triggeredByPressRelease = false;
        public bool autoHandleLuminousShot = true;
        public bool suppressSkillActivation = false;
        public bool hideStockCount = false;
        public bool hideCooldown = false;

        public SkillDefInfo() { }
    }

    internal class SteppedSkillDefInfo : SkillDefInfo
    {
        public int stepCount;
        public float stepGraceDuration;
        public float stepResetTimer;

        public SteppedSkillDefInfo() { }
    }

    internal class PassiveItemSkillDefInfo : SkillDefInfo
    {
        public ItemDef passiveItem;

        public PassiveItemSkillDefInfo() { }
    }
}