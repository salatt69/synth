using Newtonsoft.Json.Utilities;
using ProjectSynth.Character.Synth.Content.SkillDefs;
using ProjectSynth.Character.Synth.States.Hologram;
using ProjectSynth.Character.Synth.UI.Crosshair;
using ProjectSynth.Core;
using ProjectSynth.Hologram;
using ProjectSynth.Modules;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthAssets
    {
        private static AssetBundle _ab;

        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;
        public static GameObject bombExplosionEffect;
        public static GameObject vfx_stunningPerformance; // TODO:
        public static GameObject vfx_culturallyShocked; // TODO:
        public static GameObject vfx_divaExplosion; // TODO:

        // projectiles
        public static GameObject proj_ThirtyNineMusic;
        public static GameObject proj_Diva;

        // UI
        public static GameObject synthCrosshair;
        public static GameObject defaultSprintingCrosshair;
        public static GameObject divaIndicator; // TODO:

        // Pod
        public static GameObject synthSurvivorPod; // TODO:

        // textures
        public static Sprite tex_icon_SonicBoom; // TODO:
        public static Sprite tex_icon_ThirtyNineMusic; // TODO:
        public static Sprite tex_icon_Diva; // TODO:
        public static Sprite tex_icon_DivaTeleport; // TODO:
        public static Sprite tex_icon_EncoreBuff; // TODO:

        public static Texture tex_synthPortrait; // TODO:

        public static Texture tex_RampDivaSphereMain;
        public static Texture tex_RampDivaSphereVoid;
        public static Texture tex_RampDivaSphereAlt;

        public static Texture tex_RampStunningPerformanceMain;

        // materials
        public static Material mat_DivaBlink;
        public static Material mat_DivaSphere;
        public static Material mat_DivaTrailLine;
        public static Material mat_DivaTrailParticles;

        public static Material mat_cultureShockOverlayMain; // TODO:

        public static Material mat_DivaStunningPerformaceSphere;


        public static void Init(AssetBundle assetBundle)
        {
            _ab = assetBundle;

            Sounds.CreateSoundEvents();

            RegisterTextures();
            RegisterMisc();

            CreateMaterials();

            CreateEffects();
            CreateProjectiles();
            
            defaultSprintingCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SprintingCrosshair.prefab").WaitForCompletion();
            CreateSynthCrosshair();
        }
        
        private static void RegisterTextures()
        {
            tex_synthPortrait = _ab.LoadAsset<Texture>("texHenryIcon");
            tex_icon_SonicBoom = _ab.LoadAsset<Sprite>("texBazookaFireIcon");
            tex_icon_ThirtyNineMusic = _ab.LoadAsset<Sprite>("texBoxingGlovesIcon");
            tex_icon_Diva = _ab.LoadAsset<Sprite>("texSecondaryIcon");
            tex_icon_DivaTeleport = _ab.LoadAsset<Sprite>("texBazookaIconScepter");
            tex_icon_EncoreBuff = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC3/Items/SharedSuffering/texSharedSufferingDebuffIcon.png").WaitForCompletion();

            tex_RampDivaSphereMain = _ab.LoadAsset<Texture>("texRampDivaSphereMain");
            tex_RampDivaSphereVoid = _ab.LoadAsset<Texture>("texRampDivaSphereVoid");
            tex_RampDivaSphereAlt = _ab.LoadAsset<Texture>("texRampDivaSphereAlt");

            tex_RampStunningPerformanceMain = _ab.LoadAsset<Texture>("texRampStunningPerformance");
        }

        private static void CreateMaterials()
        {
            mat_DivaBlink = _ab.LoadAsset<Material>("matDivaBlink").ConvertStubbedShaderToHopoo_CloudRemap();
            mat_DivaSphere = _ab.LoadAsset<Material>("matDivaSphere").ConvertStubbedShaderToHopoo_Intersection();
            mat_DivaTrailLine = _ab.LoadAsset<Material>("matDivaTrailLine").ConvertStubbedShaderToHopoo_CloudRemap();
            mat_DivaTrailParticles = _ab.LoadAsset<Material>("matDivaTrailParicles").ConvertStubbedShaderToHopoo_CloudRemap();
            mat_DivaStunningPerformaceSphere = _ab.LoadAsset<Material>("matDivaStunningPerformanceSphere").ConvertStubbedShaderToHopoo_Intersection();

            mat_cultureShockOverlayMain = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matIsShocked.mat").WaitForCompletion();
        }  

        private static void RegisterMisc()
        {
            synthSurvivorPod = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorPod.prefab").WaitForCompletion();
            divaIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab").WaitForCompletion();
        }

        private static void CreateEffects()
        {
            bombExplosionEffect = _ab.LoadEffect("BombExplosionEffect", "HenryBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

            swordSwingEffect = _ab.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _ab.LoadEffect("ImpactHenrySlash");

            vfx_stunningPerformance = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazerSupplyDropNova.prefab").WaitForCompletion();
            vfx_stunningPerformance.transform.Find("Nova Sphere").GetComponent<ParticleSystemRenderer>().material = mat_DivaStunningPerformaceSphere;

            vfx_divaExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Mage/MageLightningBombExplosion.prefab").WaitForCompletion();

            vfx_culturallyShocked = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShockedEffect.prefab").WaitForCompletion();
        }

        private static void CreateProjectiles()
        {
            // ThirtyNineMusic
            proj_ThirtyNineMusic = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/SyringeProjectile.prefab")
                .WaitForCompletion()?
                .InstantiateClone("ThirtyNineMusic", true);

            var tnm_controller = proj_ThirtyNineMusic.GetComponent<ProjectileController>();
            tnm_controller.startSound = Sounds.ThirtyNineMusicShot;

            var tnm_single = proj_ThirtyNineMusic.GetComponent<ProjectileSingleTargetImpact>();
            tnm_single.hitSound = Sounds.thirtyNineMusicHitSoundEvent;

            var tnm_ghost = _ab.LoadAsset<GameObject>("ThirtyNineMusicGhost")?
                .InstantiateClone("ThirtyNineMusicGhost", true);

            tnm_ghost.AddComponent<NetworkIdentity>();
            tnm_ghost.AddComponent<ProjectileGhostController>();

            tnm_ghost.AddComponent<VFXAttributes>().DoNotPool = true;
            tnm_controller.ghostPrefab = tnm_ghost;

            ContentAddition.AddProjectile(proj_ThirtyNineMusic);

            // Virtual Deviation (codename: Diva)
            proj_Diva = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiMine.prefab")
                .WaitForCompletion()?
                .InstantiateClone("DivaProjectile", true);

            Asset.DestroyChild(proj_Diva, "Ring");
            Asset.DestroyChild(proj_Diva, "PrepEffect");
            Asset.DestroyChild(proj_Diva, "WeakIndicator");
            Asset.DestroyChild(proj_Diva, "StrongIndicator");

            foreach (var comp in proj_Diva.GetComponents<MonoBehaviour>())
            {
                if (comp is Deployable
                    || comp is ProjectileDeployToOwner
                    || comp is ProjectileStickOnImpact
                    || comp is EntityStateMachine
                    )
                {
                    UnityEngine.Object.DestroyImmediate(comp);
                }
            }

            var divaVisuals = _ab.LoadAsset<GameObject>("DivaVisuals")!.InstantiateClone("DivaVisuals", false);
            Transform diva_sphere = divaVisuals.transform.Find("Hologram/Sphere");
            Transform diva_blink = divaVisuals.transform.Find("Blink");

            divaVisuals.transform.SetParent(proj_Diva.transform, false);

            var diva_pulse = proj_Diva.AddComponent<ParticlePulseMusicSync>();
            diva_pulse.particleSystem = diva_blink.GetComponent<ParticleSystem>();

            var diva_esm1 = proj_Diva.AddComponent<EntityStateMachine>();
            diva_esm1.initialStateType = new EntityStates.SerializableEntityStateType(typeof(DivaArmingUnarmed));
            diva_esm1.mainStateType = new EntityStates.SerializableEntityStateType(typeof(DivaArmingUnarmed));
            diva_esm1.customName = "Arming";

            var diva_esm2 = proj_Diva.AddComponent<EntityStateMachine>();
            diva_esm2.initialStateType = new EntityStates.SerializableEntityStateType(typeof(WaitForStick));
            diva_esm2.mainStateType = new EntityStates.SerializableEntityStateType(typeof(StunningPerformance));
            diva_esm2.customName = "Main";

            var diva_networkEsm = proj_Diva.GetComponent<NetworkStateMachine>();
            diva_networkEsm.stateMachines = [diva_esm1, diva_esm2];

            proj_Diva.AddComponent<ProjectileMarker>();
            proj_Diva.AddComponent<DivaAnimator>();

            var diva_stick = proj_Diva.AddComponent<ProjectileStickOnImpactByNormal>();
            diva_stick.minGroundNormalY = 0.65f;
            diva_stick.ignoreCharacters = true;
            diva_stick.ignoreWorld = false;
            diva_stick.alignNormals = true;

            var diva_controller = proj_Diva.GetComponent<ProjectileController>();

            var diva_target = proj_Diva.GetComponent<ProjectileSphereTargetFinder>();
            diva_target.lookRange = 14f;
            diva_sphere.localScale *= diva_target.lookRange * 2f;

            var diva_lifetime = proj_Diva.AddComponent<DivaLifetime>();
            diva_lifetime.flyingLifetime = 5f;
            diva_lifetime.stuckLifetime = 10000f;

            var diva_ghost = _ab.LoadAsset<GameObject>("DivaGhost")?.InstantiateClone("DivaProjectileGhost", true);
            diva_controller.ghostPrefab = diva_ghost;
            diva_ghost.AddComponent<ProjectileGhostController>();
            diva_ghost.AddComponent<VFXAttributes>().DoNotPool = true;

            ContentAddition.AddProjectile(proj_Diva);
        }
        private static void CreateSynthCrosshair()
        {
            synthCrosshair = _ab.LoadAsset<GameObject>("SynthCrosshair");

            HudElement hudElem = synthCrosshair.AddComponent<HudElement>();
            CrosshairController controller = synthCrosshair.AddComponent<CrosshairController>();

            controller.spriteSpreadPositions =
            [
               new CrosshairController.SpritePosition
               {
                   target = synthCrosshair.transform.Find("Heart, R").GetComponent<RectTransform>(),
                   zeroPosition = new Vector2(25f, 0f),
                   onePosition = new Vector2(60f, 0f)
               },
               new CrosshairController.SpritePosition
               {
                   target = synthCrosshair.transform.Find("Heart, L").GetComponent<RectTransform>(),
                   zeroPosition = new Vector2(-25f, 0f),
                   onePosition = new Vector2(-60f, 0f)
               }
            ];
            controller.maxSpreadAngle = 3;

            SynthCrosshairController synthController = synthCrosshair.AddComponent<SynthCrosshairController>();

            Texture image = defaultSprintingCrosshair.GetComponent<RawImage>().texture;
            Color color = defaultSprintingCrosshair.GetComponent<RawImage>().color;

            RawImage rawImage = synthCrosshair.transform.Find("Center, Sprint").GetComponent<RawImage>();
            rawImage.texture = image;
            rawImage.color = color;
        }
    }

    public static class Sounds
    {
        public static readonly string ThirtyNineMusicShot = "Play_synth_ThirtyNineMusic_shot";

        // Metronome
        public static readonly string MetronomeSustain = "Play_synth_MetronomeSustain";
        public static readonly string MetronomeSustainStop = "Stop_synth_MetronomeSustain";
        public static readonly string[] MetronomeRecharge =
        {
            "Play_MetronomeRecharge_1",
            "Play_MetronomeRecharge_2",
            "Play_MetronomeRecharge_3",
            "Play_MetronomeRecharge_4"
        };
        
        public static NetworkSoundEventDef thirtyNineMusicHitSoundEvent;

        public static void CreateSoundEvents()
        {
            thirtyNineMusicHitSoundEvent = CreateSoundEvent(Sounds.ThirtyNineMusicShot);
        }

        private static NetworkSoundEventDef CreateSoundEvent(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            ContentAddition.AddNetworkSoundEventDef(networkSoundEventDef);

            return networkSoundEventDef;
        }
    }
}
