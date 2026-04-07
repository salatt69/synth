using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Character.Synth.States.Special;
using ProjectSynth.Character.Synth.States.Utility;
using ProjectSynth.Components;
using ProjectSynth.Mod;
using ProjectSynth.Modules;
using ProjectSynth.Modules.BaseContent.Characters;
using ProjectSynth.States.Synth;
using ProjectSynth.States.Synth.Metro;
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
        public override string modelPrefabName => "mdlSynth";
        public override string displayPrefabName => "SynthDisplay";

        public const string SYNTH_PREFIX = SynthPlugin.DEVELOPER_PREFIX + "_SYNTH_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => SYNTH_PREFIX;

        public override BodyInfo bodyInfo => new()
        {
            bodyName = bodyName,
            bodyNameToken = SYNTH_PREFIX + "NAME",
            subtitleNameToken = SYNTH_PREFIX + "SUBTITLE",

            characterPortrait = SynthAssets.tex_synthPortrait,
            bodyColor = new Color(0.26f, 0.95f, 1f),
            sortPosition = 100,

            crosshair = SynthAssets.synthCrosshair,
            podPrefab = SynthAssets.synthSurvivorPod,

            maxHealth = 110f,
            healthRegen = 1.5f,
            armor = 0f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos =>
        [
            new CustomRendererInfo
            {
                childName = "SwordModel",
                material = assetBundle.LoadAsset<Material>("matHenry").ConvertStubbedShaderToHopoo_Standart(),
            },
            new CustomRendererInfo
            {
                childName = "GunModel",
            },
            new CustomRendererInfo
            {
                childName = "Model",
            }
        ];

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
            assetBundle = Asset.LoadAssetBundle(assetBundleName);
            SynthAssets.Init(assetBundle);

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

            SynthBuffs.Init();
            SynthDamageTypes.Register();

            SynthPassive.Initialize();

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<DivaTracker>();
            bodyPrefab.AddComponent<SynthSurvivorController>();
            bodyPrefab.AddComponent<SynthMetroRuntime>();
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
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.SpawnTeleporterState), typeof(SynthMain));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "DivaDeploy");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Metro", typeof(MetroWaitForInputState), typeof(MetroWaitForInputState));
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
            // TODO: make it into Skills module
            // TODO: or rather just rethink how passives are set up entirely. (no item needed)

            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "Passive");

            PassiveItemSkillDef metro = SynthSkillDefs.Passive_Metro();
            PassiveItemSkillDef another = SynthSkillDefs.Passive_Another();

            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, metro, another);
        }

        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            SkillDef tnm = SynthSkillDefs.Primary_ThirtyNineMusic();
            Skills.AddPrimarySkills(bodyPrefab, tnm);
        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "HenryRoll",
                skillNameToken = SYNTH_PREFIX + "SECONDARY_ROLL_NAME",
                skillDescriptionToken = SYNTH_PREFIX + "SECONDARY_ROLL_DESCRIPTION",
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

            SkillDef diva = SynthSkillDefs.Secondary_DeployDiva();
            SynthSkillDefs.Secondary_TeleportToDiva();

            Skills.AddSecondarySkills(bodyPrefab, diva, utilitySkillDef1);
        }

        private void AddUtilitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            SkillDef sonicBoom = SynthSkillDefs.Utility_SonicBoom();
            Skills.AddUtilitySkills(bodyPrefab, sonicBoom);
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
                activationStateMachineName = "DivaDeploy",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 10f,

                isCombatSkill = true,
                mustKeyPress = false,
            });

            SkillDef mikuBeam = SynthSkillDefs.Special_MikuBeam();

            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1, mikuBeam);
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