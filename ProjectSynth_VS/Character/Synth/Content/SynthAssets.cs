using ProjectSynth.Character.Synth.UI.Crosshair;
using ProjectSynth.Core;
using ProjectSynth.Hologram;
using ProjectSynth.Modules;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
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

        // projectiles
        public static GameObject proj_ThirtyNineMusic;
        public static GameObject proj_ExpoNade;

        // crosshair
        public static GameObject synthCrosshair;
        public static GameObject defaultSprintingCrosshair;

        // textures
        public static Sprite tex_SonicBoom;
        public static Sprite tex_ThirtyNineMusic;
        public static Sprite tex_ExpoNade;
        public static Sprite tex_ExpoShift;

        // misc
        public static GameObject go_hologram;
        
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

            R2API.PrefabAPI.RegisterNetworkPrefab(proj_ExpoNade);
            R2API.PrefabAPI.RegisterNetworkPrefab(go_hologram);
        }
        
        private static void RegisterTextures()
        {
            tex_SonicBoom = _ab.LoadAsset<Sprite>("texBazookaFireIcon");
            tex_ThirtyNineMusic = _ab.LoadAsset<Sprite>("texBoxingGlovesIcon");
            tex_ExpoNade = _ab.LoadAsset<Sprite>("texSecondaryIcon");
            tex_ExpoShift = _ab.LoadAsset<Sprite>("texBazookaIconScepter");
        }

        private static void RegisterMisc()
        {
            go_hologram = _ab.LoadAsset<GameObject>("Hologram");
            go_hologram.AddComponent<NetworkIdentity>();
            go_hologram.AddComponent<DestroyOnTimer>().duration = 20f;
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

            var tnm_ghost = _ab.LoadAsset<GameObject>("ThirtyNineMusicModel")?
                .InstantiateClone("ThirtyNineMusicModel", true);

            tnm_ghost.AddComponent<NetworkIdentity>();
            tnm_ghost.AddComponent<ProjectileGhostController>();

            tnm_ghost.AddComponent<VFXAttributes>().DoNotPool = true;
            tnm_controller.ghostPrefab = tnm_ghost;

            ContentAddition.AddProjectile(proj_ThirtyNineMusic);

            // nade
            proj_ExpoNade = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab")
                .WaitForCompletion()?
                .InstantiateClone("ExpoNade", true);

            var expo_controller = proj_ExpoNade.GetComponent<ProjectileController>();
            // expo_controller.startSound = 

            var expo_impact = proj_ExpoNade.GetComponent<ProjectileImpactExplosion>();
            expo_impact.timerAfterImpact = false;
            expo_impact.lifetime = 10f;
            expo_impact.destroyOnWorld = false;
            expo_impact.destroyOnEnemy = false;
            expo_impact.detonateOnEnemy = false;

            var expo_reporter = proj_ExpoNade.AddComponent<HologramProjectileReporter>();

            var expo_hologram = proj_ExpoNade.AddComponent<HologramSpawnBehavior>();
            expo_hologram.objectToSpawn = SynthAssets.go_hologram;

            var expo_ghost = _ab.LoadAsset<GameObject>("ExpoNadeModel")?
                .InstantiateClone("ExpoNadeModel", true);

            expo_ghost.AddComponent<NetworkIdentity>();
            expo_ghost.AddComponent<ProjectileGhostController>();

            expo_ghost.AddComponent<VFXAttributes>().DoNotPool = true;
            expo_controller.ghostPrefab = expo_ghost;

            ContentAddition.AddProjectile(proj_ExpoNade);
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
