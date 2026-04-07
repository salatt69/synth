using Newtonsoft.Json.Utilities;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
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
using ProjectSynth.States.Synth.Metro;
using ProjectSynth.States.Synth.Diva;
using ProjectSynth.Components;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthAssets
    {
        private static AssetBundle _ab;

        public static CharacterCameraParams ccpMikuBeam;
        public static GameObject mdlSynth;

        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;
        public static GameObject bombExplosionEffect;
        public static GameObject vfx_stunningPerformance; // TODO:
        public static GameObject vfx_cultureShock;
        public static GameObject vfx_divaExplosion; // TODO:
        public static GameObject vfx_encoreExplosion; // TODO:
        public static GameObject vfx_mikuBeamEffect; // TODO:
        public static GameObject vfx_tnmMuzzleFlash; // TODO:
        public static GameObject vfx_tnmTracer; // TODO:

        // projectiles
        public static GameObject proj_ThirtyNineMusic;
        public static GameObject proj_Diva;

        // UI
        public static GameObject synthCrosshair; // TODO:
        public static GameObject synthMetroOverlay; // TODO:
        public static GameObject synthRushOverlay; // TODO:
        public static GameObject defaultSprintingCrosshair;
        public static GameObject divaIndicator; // TODO:
        public static GameObject divaIndicatorLooking; // TODO:

        // Pod
        public static GameObject synthSurvivorPod; // TODO:

        // textures
        public static Sprite tex_icon_SonicBoom; // TODO:
        public static Sprite tex_icon_ThirtyNineMusic; // TODO:
        public static Sprite tex_icon_Diva; // TODO:
        public static Sprite tex_icon_DivaTeleport; // TODO:
        public static Sprite tex_icon_EncoreBuff; // TODO:
        public static Sprite tex_icon_Metro; // TODO:

        public static Texture tex_synthPortrait; // TODO:

        public static Texture tex_encoreBars;

        public static Texture tex_RampDivaSphereMain;
        public static Texture tex_RampDivaSphereVoid;
        public static Texture tex_RampDivaSphereAlt;

        public static Texture tex_RampStunningPerformanceMain;

        public static Texture tex_RampEncoreGlitter; // TODO:

        // materials
        public static Material mat_SynthBody; // TODO:
        public static Material mat_SynthScreen; // TODO:

        public static Material mat_DivaBlink;
        public static Material mat_DivaSphere;
        public static Material mat_DivaTrailLine;
        public static Material mat_DivaTrailParticles;

        public static Material mat_SoundWave; // TODO:

        public static Material mat_cultureShockOverlayMain; // TODO:
        public static Material mat_mikuStun;
        public static Material mat_eighthNote;

        public static Material mat_DivaStunningPerformaceSphere;

        public static Material mat_encoreGlitter;

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
            CreateCrosshairAndOverlay();
        }
        
        private static void RegisterTextures()
        {
            tex_synthPortrait = _ab.LoadAsset<Texture>("texHenryIcon");
            tex_icon_SonicBoom = _ab.LoadAsset<Sprite>("texBazookaFireIcon");
            tex_icon_ThirtyNineMusic = _ab.LoadAsset<Sprite>("texBoxingGlovesIcon");
            tex_icon_Diva = _ab.LoadAsset<Sprite>("texSecondaryIcon");
            tex_icon_DivaTeleport = _ab.LoadAsset<Sprite>("texBazookaIconScepter");
            tex_icon_EncoreBuff = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC3/Items/SharedSuffering/texSharedSufferingDebuffIcon.png").WaitForCompletion();
            tex_icon_Metro = _ab.LoadAsset<Sprite>("texPassiveIcon");

            tex_encoreBars = _ab.LoadAsset<Texture>("texEncoreBars");

            tex_RampDivaSphereMain = _ab.LoadAsset<Texture>("texRampDivaSphereMain");
            tex_RampDivaSphereVoid = _ab.LoadAsset<Texture>("texRampDivaSphereVoid");
            tex_RampDivaSphereAlt = _ab.LoadAsset<Texture>("texRampDivaSphereAlt");

            tex_RampStunningPerformanceMain = _ab.LoadAsset<Texture>("texRampStunningPerformance");
        }

        private static void CreateMaterials()
        {
            mat_SynthBody = _ab.LoadAsset<Material>("matSynthBody").ConvertStubbedShaderToHopoo_Standart();
            mat_SynthScreen = _ab.LoadAsset<Material>("matSynthScreen").ConvertStubbedShaderToHopoo_Standart();
            mdlSynth = _ab.LoadAsset<GameObject>("mdlSynth");

            mat_DivaBlink = _ab.LoadAsset<Material>("matDivaBlink").ConvertStubbedShaderToHopoo_CloudRemap();
            mat_DivaSphere = _ab.LoadAsset<Material>("matDivaSphere").ConvertStubbedShaderToHopoo_Intersection();
            mat_DivaTrailLine = _ab.LoadAsset<Material>("matDivaTrailLine").ConvertStubbedShaderToHopoo_CloudRemap();
            mat_DivaTrailParticles = _ab.LoadAsset<Material>("matDivaTrailParicles").ConvertStubbedShaderToHopoo_CloudRemap();
            mat_DivaStunningPerformaceSphere = _ab.LoadAsset<Material>("matDivaStunningPerformanceSphere").ConvertStubbedShaderToHopoo_Intersection();

            mat_SoundWave = _ab.LoadAsset<Material>("matSoundWave").ConvertStubbedShaderToHopoo_CloudRemap();

            mat_cultureShockOverlayMain = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matIsShocked.mat").WaitForCompletion();

            mat_mikuStun = _ab.LoadAsset<Material>("matDivaMikuStun").ConvertStubbedShaderToHopoo_OpaqueCloudRemap();
            mat_eighthNote = _ab.LoadAsset<Material>("matDivaEighthNote").ConvertStubbedShaderToHopoo_OpaqueCloudRemap();

            mat_encoreGlitter = _ab.LoadAsset<Material>("matEncoreGlitter").ConvertStubbedShaderToHopoo_CloudRemap();
        }

        private static void RegisterMisc()
        {
            synthSurvivorPod = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorPod.prefab").WaitForCompletion();
            divaIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiMissileTrackingIndicator.prefab").WaitForCompletion();
            divaIndicatorLooking = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTargetIndicator.prefab").WaitForCompletion();

            ccpMikuBeam = ScriptableObject.CreateInstance<CharacterCameraParams>();
            ccpMikuBeam.name = "ccpMikuBeam";
            ccpMikuBeam.data.minPitch = -70;
            ccpMikuBeam.data.maxPitch = 50;
            ccpMikuBeam.data.wallCushion = 0.1f;
            ccpMikuBeam.data.pivotVerticalOffset = 2f;
            ccpMikuBeam.data.idealLocalCameraPos = new Vector3(2, -2.5f, -2f);
            ccpMikuBeam.data.fov = 75f;
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
            vfx_mikuBeamEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamCorrupt.prefab").WaitForCompletion();

            // CS
            vfx_cultureShock = _ab.LoadAsset<GameObject>("CultureShock");
            var cshock_psDration = vfx_cultureShock.AddComponent<ScaleParticleSystemDuration>();
            cshock_psDration.initialDuration = 2f;
            cshock_psDration.particleSystems = vfx_cultureShock.GetComponentsInChildren<ParticleSystem>();

            var cshock_vfxAttributes = vfx_cultureShock.AddComponent<VFXAttributes>();
            cshock_vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            cshock_vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;

            vfx_cultureShock.AddComponent<HoverOverHead>();
            vfx_cultureShock.AddComponent<Rigidbody>().isKinematic = true;

            vfx_cultureShock.transform.Find("Bonk").gameObject.AddComponent<HoverOverHead>();

            // EE
            vfx_encoreExplosion = _ab.LoadAsset<GameObject>("EncoreExplosion");
            var encoreExplosion_effectComponent = vfx_encoreExplosion.AddComponent<EffectComponent>();
            encoreExplosion_effectComponent.soundName = "";
            encoreExplosion_effectComponent.applyScale = true;

            var encoreExplosion_vfxAttributes = vfx_encoreExplosion.AddComponent<VFXAttributes>();
            encoreExplosion_vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            encoreExplosion_vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;

            var encoreExplosion_particleEnd = vfx_encoreExplosion.AddComponent<DestroyOnParticleEnd>();
            encoreExplosion_particleEnd.trackedParticleSystem = vfx_encoreExplosion.GetComponentInChildren<ParticleSystem>();
            ContentAddition.AddEffect(vfx_encoreExplosion);

            vfx_tnmMuzzleFlash = _ab.LoadAsset<GameObject>("SynthTNMMuzzleFlash");
            var tnmMuzzleFlash_vfxAttributes = vfx_tnmMuzzleFlash.AddComponent<VFXAttributes>();
            tnmMuzzleFlash_vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
            tnmMuzzleFlash_vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;

            var tnmMuzzleFlash_effectComponent = vfx_tnmMuzzleFlash.AddComponent<EffectComponent>();
            tnmMuzzleFlash_effectComponent.soundName = "";
            tnmMuzzleFlash_effectComponent.positionAtReferencedTransform = true;
            tnmMuzzleFlash_effectComponent.parentToReferencedTransform = true;

            var tnmMuzzleFlash_destroyOnTimer = vfx_tnmMuzzleFlash.AddComponent<DestroyOnTimer>();
            tnmMuzzleFlash_destroyOnTimer.duration = 1.0f;
            ContentAddition.AddEffect(vfx_tnmMuzzleFlash);

            vfx_tnmTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamTracer.prefab").WaitForCompletion();

        }

        private static void CreateProjectiles()
        {
            // ThirtyNineMusic
            //proj_ThirtyNineMusic = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/SyringeProjectile.prefab")
            //    .WaitForCompletion()?
            //    .InstantiateClone("ThirtyNineMusic", true);

            //var tnm_controller = proj_ThirtyNineMusic.GetComponent<ProjectileController>();
            //tnm_controller.startSound = Sounds.ThirtyNineMusicShot;

            //var tnm_single = proj_ThirtyNineMusic.GetComponent<ProjectileSingleTargetImpact>();
            //tnm_single.hitSound = Sounds.thirtyNineMusicHitSoundEvent;

            //var tnm_ghost = _ab.LoadAsset<GameObject>("ThirtyNineMusicGhost")?
            //    .InstantiateClone("ThirtyNineMusicGhost", true);

            //tnm_ghost.AddComponent<NetworkIdentity>();
            //tnm_ghost.AddComponent<ProjectileGhostController>();

            //tnm_ghost.AddComponent<VFXAttributes>().DoNotPool = true;
            //tnm_controller.ghostPrefab = tnm_ghost;

            //ContentAddition.AddProjectile(proj_ThirtyNineMusic);

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
                // TODO: deployable doesn;t want ot be removed, look into it
                if (comp is ProjectileDeployToOwner
                    || comp is ProjectileStickOnImpact
                    || comp is EntityStateMachine
                    || comp is Deployable
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

        private static void CreateCrosshairAndOverlay()
        {
            synthCrosshair = _ab.LoadAsset<GameObject>("SynthCrosshair");

            synthMetroOverlay = _ab.LoadAsset<GameObject>("MetroOverlay");
            synthMetroOverlay.AddComponent<SynthOverlayController>();

            synthRushOverlay = _ab.LoadAsset<GameObject>("RushOverlay");
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

        // CultureShock
        public static readonly string[] CultureShockStart =
        {
            "Play_CultureShockStart_1",
            "Play_CultureShockStart_2",
            "Play_CultureShockStart_3",
            "Play_CultureShockStart_4",
            "Play_CultureShockStart_5",
            "Play_CultureShockStart_6",
            "Play_CultureShockStart_7"
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
