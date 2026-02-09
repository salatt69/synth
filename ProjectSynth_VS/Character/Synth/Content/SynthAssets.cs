using Newtonsoft.Json.Utilities;
using ProjectSynth.Character.Synth.States.Hologram;
using ProjectSynth.Character.Synth.UI.Crosshair;
using ProjectSynth.Core;
using ProjectSynth.Hologram;
using ProjectSynth.Modules;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Rewired.Demos.GamepadTemplateUI.GamepadTemplateUI;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthAssets
    {
        private static AssetBundle _ab;

        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;
        public static GameObject bombExplosionEffect;

        // projectiles
        public static GameObject proj_ThirtyNineMusic;
        public static GameObject proj_Diva;

        // crosshair
        public static GameObject synthCrosshair;
        public static GameObject defaultSprintingCrosshair;

        // textures
        public static Sprite tex_icon_SonicBoom;
        public static Sprite tex_icon_ThirtyNineMusic;
        public static Sprite tex_icon_Diva;
        public static Sprite tex_icon_DivaTeleport;

        
        public static void Init(AssetBundle assetBundle)
        {
            _ab = assetBundle;

            Sounds.CreateSoundEvents();

            RegisterTextures();
            RegisterMisc();
            
            CreateEffects();
            CreateProjectiles();
            
            defaultSprintingCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SprintingCrosshair.prefab").WaitForCompletion();
            CreateSynthCrosshair();
        }
        
        private static void RegisterTextures()
        {
            tex_icon_SonicBoom = _ab.LoadAsset<Sprite>("texBazookaFireIcon");
            tex_icon_ThirtyNineMusic = _ab.LoadAsset<Sprite>("texBoxingGlovesIcon");
            tex_icon_Diva = _ab.LoadAsset<Sprite>("texSecondaryIcon");
            tex_icon_DivaTeleport = _ab.LoadAsset<Sprite>("texBazookaIconScepter");
        }

        private static void RegisterMisc()
        {
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
        }

        private static void CreateProjectiles()
        {
            // tnm
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

            // vd
            proj_Diva = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiMine.prefab")
                .WaitForCompletion()?
                .InstantiateClone("DivaProjectile", true);

            Asset.DestroyChild(proj_Diva, "Ring");
            Asset.DestroyChild(proj_Diva, "PrepEffect");
            Asset.DestroyChild(proj_Diva, "WeakIndicator");
            Asset.DestroyChild(proj_Diva, "StrongIndicator");

            var divaVisuals = _ab.LoadAsset<GameObject>("DivaVisuals")!.InstantiateClone("DivaVisuals", false);
            divaVisuals.transform.SetParent(proj_Diva.transform, false);

            foreach (var comp in proj_Diva.GetComponents<MonoBehaviour>())
            {
                if (comp is Deployable
                    || comp is ProjectileDeployToOwner
                    || comp is ProjectileStickOnImpact
                    )
                {
                    comp.enabled = false;
                }
            }

            // obliterate existing esm from the engi mine
            var oldEsms = proj_Diva.GetComponents<EntityStateMachine>();
            for (int i = 0; i < oldEsms.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(oldEsms[i]);
            }

            var diva_esm = proj_Diva.AddComponent<EntityStateMachine>();
            diva_esm.initialStateType = new EntityStates.SerializableEntityStateType(typeof(WaitForStick));
            diva_esm.mainStateType = new EntityStates.SerializableEntityStateType(typeof(Lure));
            diva_esm.customName = "Main";

            var diva_networkEsm = proj_Diva.GetComponent<NetworkStateMachine>();
            diva_networkEsm.stateMachines = [diva_esm];

            var diva_marker = proj_Diva.AddComponent<DivaMarker>();

            var diva_stick = proj_Diva.AddComponent<ProjectileStickOnImpactByNormal>();
            diva_stick.minGroundNormalY = 0.65f;
            diva_stick.ignoreCharacters = true;
            diva_stick.ignoreWorld = false;
            diva_stick.alignNormals = true;
            diva_stick.stickParticleSystem = proj_Diva.GetComponentsInChildren<ParticleSystem>(true);

            var diva_controller = proj_Diva.GetComponent<ProjectileController>();

            var diva_ghost = _ab.LoadAsset<GameObject>("DivaGhost")?.InstantiateClone("DivaProjectileGhost", true);
            diva_controller.ghostPrefab = diva_ghost;
            diva_ghost.AddComponent<ProjectileGhostController>();
            diva_ghost.AddComponent<VFXAttributes>().DoNotPool = true;
            diva_ghost.AddComponent<DivaAnimator>();

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
