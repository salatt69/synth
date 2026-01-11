using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Character.Synth.Content.Items;
using ProjectSynth.Character.Synth.Content.SkillDefs;
using ProjectSynth.Character.Synth.States;
using ProjectSynth.Character.Synth.States.Primary;
using ProjectSynth.Character.Synth.States.Secondary;
using ProjectSynth.Character.Synth.States.Special;
using ProjectSynth.Character.Synth.States.Utility;
using ProjectSynth.Core;
using ProjectSynth.Metronome;
using ProjectSynth.Modules;
using ProjectSynth.Modules.BaseContent.Characters;
using R2API;
using RoR2;
using RoR2.Skills;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth
{
    public class SynthSurvivor : SurvivorBase<SynthSurvivor>
    {
        public override string assetBundleName => SynthPlugin.MODNAME.ToLower() + "_bundle";
        public override string bodyName => "SynthBody";
        public override string masterName => "SynthMonsterMaster";
        public override string modelPrefabName => "mdlHenry";
        public override string displayPrefabName => "HenryDisplay";

        public const string SYNTH_PREFIX = SynthPlugin.DEVELOPER_PREFIX + "_SYNTH_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => SYNTH_PREFIX;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = SYNTH_PREFIX + "NAME",
            subtitleNameToken = SYNTH_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texHenryIcon"),
            bodyColor = new Color(67.0f, 243.0f, 255.0f),
            sortPosition = 100,

            crosshair = assetBundle.LoadAsset<GameObject>("SynthCrosshair"),
            podPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorPod.prefab").WaitForCompletion(),

            maxHealth = 110f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
            new CustomRendererInfo
            {
                childName = "SwordModel",
                material = assetBundle.LoadMaterial("matHenry"),
            },
            new CustomRendererInfo
            {
                childName = "GunModel",
            },
            new CustomRendererInfo
            {
                childName = "Model",
            }
        };

        public override UnlockableDef characterUnlockableDef => SynthUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new SynthItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Initialize()
        {
            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            SynthUnlockables.Init();

            base.InitializeCharacter();

            SynthConfig.Init();
            SynthStates.Init();
            SynthTokens.Init();

            // it is here and not in SynthPlugin.Awake(), bc this mod has no items to pickup
            // it only has items for passive skills
            Passive.Initialize();

            SynthAssets.Init(assetBundle);
            SynthBuffs.Init(assetBundle);
            SynthDamageTypes.Register();

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<MetronomeComponent>();
        }

        public void AddHitboxes()
        {
            //example of how to create a HitBoxGroup. see summary for more details
            Prefabs.SetupHitBoxGroup(characterModelObject, "SwordGroup", "SwordHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(SynthMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            AddPassiveSkill();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill()
        {
            //option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");

            PassiveItemSkillDef passiveSkillDef1 = ScriptableObject.CreateInstance<PassiveItemSkillDef>();
            passiveSkillDef1.skillName = "M1K-U";
            passiveSkillDef1.skillNameToken = SYNTH_PREFIX + "METRONOME_PASSIVE_NAME";
            passiveSkillDef1.skillDescriptionToken = SYNTH_PREFIX + "METRONOME_PASSIVE_DESCRIPTION";
            passiveSkillDef1.icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon");
            passiveSkillDef1.passiveItem = Passive.Metronome;

            passiveSkillDef1.activationState = new EntityStates.SerializableEntityStateType(typeof(SynthMain));
            passiveSkillDef1.activationStateMachineName = "Body";

            PassiveItemSkillDef passiveSkillDef2 = ScriptableObject.CreateInstance<PassiveItemSkillDef>();
            passiveSkillDef2.skillName = "M1K-U v2.0";
            passiveSkillDef2.skillNameToken = SYNTH_PREFIX + "ANOTHER_PASSIVE_NAME";
            passiveSkillDef2.skillDescriptionToken = SYNTH_PREFIX + "ANOTHER_PASSIVE_DESCRIPTION";
            passiveSkillDef2.icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon");
            passiveSkillDef2.passiveItem = Passive.Another;

            passiveSkillDef2.activationState = new EntityStates.SerializableEntityStateType(typeof(SynthMain));
            passiveSkillDef2.activationStateMachineName = "Body";

            ContentAddition.AddSkillDef(passiveSkillDef1);
            ContentAddition.AddSkillDef(passiveSkillDef2);

            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef2);
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference

            SkillDef primarySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "39 Music!",
                skillNameToken = SYNTH_PREFIX + "PRIMARY_THIRTY_NINE_MUSIC_NAME",
                skillDescriptionToken = SYNTH_PREFIX + "PRIMARY_THIRTY_NINE_MUSIC_DESCRIPTION",
                keywordTokens = new string[] { SYNTH_PREFIX + "KEYWORD_FOLLOW_THE_RHYTHM" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texBoxingGlovesIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ThirtyNineMusic)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Any,

                baseRechargeInterval = 0,
                baseMaxStock = 0,

                rechargeStock = 0,
                requiredStock = 0,
                stockToConsume = 0,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,

            });

            SteppedSkillDef primarySkillDef2 = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "HenrySlash",
                    SYNTH_PREFIX + "PRIMARY_SLASH_NAME",
                    SYNTH_PREFIX + "PRIMARY_SLASH_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new EntityStates.SerializableEntityStateType(typeof(SlashCombo)),
                    "Weapon",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            primarySkillDef2.stepCount = 2;
            primarySkillDef2.stepGraceDuration = 0.5f;

            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef1);
            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef2);
        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            //here is a basic skill def with all fields accounted for
            SkillDef secondarySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryGun",
                skillNameToken = SYNTH_PREFIX + "SECONDARY_GUN_NAME",
                skillDescriptionToken = SYNTH_PREFIX + "SECONDARY_GUN_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Shoot)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 1f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            SkillDef secondarySkillDef2 = SynthSkillDefs.Secondary_SonicBoom();

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef1);
            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef2);
        }

        private void AddUtilitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            //here's a skilldef of a typical movement skill.
            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryRoll",
                skillNameToken = SYNTH_PREFIX + "UTILITY_ROLL_NAME",
                skillDescriptionToken = SYNTH_PREFIX + "UTILITY_ROLL_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Roll)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 4f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1);
        }

        private void AddSpecialSkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            //a basic skill. some fields are omitted and will just have default values
            SkillDef specialSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryBomb",
                skillNameToken = SYNTH_PREFIX + "SPECIAL_BOMB_NAME",
                skillDescriptionToken = SYNTH_PREFIX + "SPECIAL_BOMB_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ThrowBomb)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 10f,

                isCombatSkill = true,
                mustKeyPress = false,
            });

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1);
        }
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySword",
            //    "meshHenryGun",
            //    "meshHenry");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin

            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(SYNTH_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deactivate it with this skin

            //skins.Add(masterySkin);

            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            // Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            SynthAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }
    }
}