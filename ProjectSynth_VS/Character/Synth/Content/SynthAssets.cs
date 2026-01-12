using ProjectSynth.Character.Synth.UI.Crosshair;
using ProjectSynth.Core;
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
        private static AssetBundle _assetBundle;

        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;
        public static GameObject bombExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef thirtyNineMusicHitSoundEvent;

        // projectiles
        public static GameObject thirtyNineMusicProjectile;

        // crosshairs
        public static GameObject synthCrosshair;
        public static GameObject dafaultSprintngCrosshair;


        public static void Init(AssetBundle assetBundle)
        {
            _assetBundle = assetBundle;

            Bundle.Init(assetBundle);

            CreateSoundEvents();
            CreateEffects();
            CreateProjectiles();

            dafaultSprintngCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SprintingCrosshair.prefab").WaitForCompletion();
            CreateCrosshair(_assetBundle);
        }

        private static void CreateCrosshair(AssetBundle bundle)
        {
            synthCrosshair = bundle.LoadAsset<GameObject>("SynthCrosshair");

            HudElement hudElem = synthCrosshair.AddComponent<HudElement>();
            CrosshairController controller = synthCrosshair.AddComponent<CrosshairController>();

            //controller.spriteSpreadPositions = new[]
            //{
            //    new CrosshairController.SpritePosition
            //    {
            //        target = synthCrosshair.transform.Find("Bracket, L").GetComponent<RectTransform>(),
            //        zeroPosition = new Vector2(-12f, 0f),
            //        onePosition = new Vector2(-32f, 0f)
            //    },
            //    new CrosshairController.SpritePosition
            //    {
            //        target = synthCrosshair.transform.Find("Bracket, R").GetComponent<RectTransform>(),
            //        zeroPosition = new Vector2(12f, 0f),
            //        onePosition = new Vector2(32f, 0f)
            //    }
            //};

            SynthCrosshairController synthController = synthCrosshair.AddComponent<SynthCrosshairController>();

            Texture image = dafaultSprintngCrosshair.GetComponent<RawImage>().texture;
            Color color = dafaultSprintngCrosshair.GetComponent<RawImage>().color;

            RawImage rawImage = synthCrosshair.transform.Find("Center, Sprint").GetComponent<RawImage>();
            rawImage.texture = image;
            rawImage.color = color;
        }

        #region sound events
        private static void CreateSoundEvents()
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

        #endregion sound events

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();

            swordSwingEffect = _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");
        }

        private static void CreateBombExplosionEffect()
        {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "HenryBombExplosion");

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

        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            thirtyNineMusicProjectile = CreateProjectile(
                "RoR2/Base/Treebot/SyringeProjectile.prefab",
                "ThirtyNineMusic",
                thirtyNineMusicHitSoundEvent,
                Sounds.ThirtyNineMusicShot
                );
        }

        private static GameObject CreateProjectile(string addressablePath, string projectileName, NetworkSoundEventDef hitSound, string startSound)
        {
            GameObject projectile = Addressables.LoadAssetAsync<GameObject>(addressablePath).WaitForCompletion()?.InstantiateClone(projectileName, true);
            if (!projectile)
            {
                Log.Warning($"Failed to load addresable GameObject! Check the spelling. [{addressablePath}]");
                return null;
            }

            var controller = projectile.GetComponent<ProjectileController>();
            controller.startSound = startSound;

            var single = projectile.GetComponent<ProjectileSingleTargetImpact>();
            single.hitSound = hitSound;

            var ghost = _assetBundle.LoadAsset<GameObject>($"{projectileName}Model")?.InstantiateClone($"{projectileName}Model", true);
            if (!ghost)
            {
                Log.Warning($"Failed to load ghost projectile! Build your AssetBundle ot check the spelling. [{projectileName}Model]");
                return null;
            }

            ghost.AddComponent<NetworkIdentity>();
            ghost.AddComponent<ProjectileGhostController>();

            ghost.AddComponent<VFXAttributes>().DoNotPool = true;
            controller.ghostPrefab = ghost;

            ContentAddition.AddProjectile(projectile);

            return projectile;
        }
        #endregion projectiles
    }

    public static class Bundle
    {
        private static AssetBundle _ab;

        public static void Init(AssetBundle assetBundle) { _ab = assetBundle; }

        public static readonly Sprite tex_SonicBoom = _ab.LoadAsset<Sprite>("texSecondaryIcon");
        public static readonly Sprite tex_ThirtyNineMusic = _ab.LoadAsset<Sprite>("texBoxingGlovesIcon");
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
    }
}
