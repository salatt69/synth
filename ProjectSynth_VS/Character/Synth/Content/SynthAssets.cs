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

        // materials
        public static Material mat_DivaBlink;
        public static Material mat_DivaSphere;

        
        public static void Init(AssetBundle assetBundle)
        {
            _ab = assetBundle;

            Sounds.CreateSoundEvents();

            RegisterTextures();
            RegisterMisc();

            CreateParticleSystemMaterials();
            CreateIntersectionMaterials();

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

        private static void CreateParticleSystemMaterials()
        {
            mat_DivaBlink = Materials.CloudRemap.CreateHopooCloudRemapMaterial(new Materials.CloudRemap.CloudRemapInfo
            {
                _TintColor = Color.white,
                _MainTex = new Materials.TiledTextureInfo
                {
                    texture = _ab.LoadAsset<Texture2D>("texDivaBlinkMask"),
                    tiling = Vector2.one,
                    offset = Vector2.zero
                },
                _RemapTex = new Materials.TiledTextureInfo
                {
                    texture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion(),
                    tiling = Vector2.one,
                    offset = Vector2.zero
                },
                _InvFade = 2f,
                _Boost = 1f,
                _AlphaBoost = 0.78f,
                _CloudsOn = 1f,
                _Cloud1Tex = new Materials.TiledTextureInfo
                {
                    texture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/TiledTextures/texCloudDifferenceBW2.png").WaitForCompletion(),
                    tiling = Vector2.one,
                    offset = Vector2.zero
                },
                _CutoffScroll = new Vector4(15f, 15f, 13f, 13f),
            },
            "matDivaBlink"
            );
        }

        private static void RegisterMisc()
        {

        }

        private static void CreateIntersectionMaterials()
        {
            mat_DivaSphere = Materials.Intersection.CreateHopooIntersectionMaterial(new Materials.Intersection.IntersectionInfo
            {
                _TintColor = Color.grey,
                _Cloud1Tex =
                {
                    texture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Engi/texEngiShield.png").WaitForCompletion(),
                    tiling = Vector2.one,
                    offset = Vector2.zero
                },
                _Cloud2Tex =
                {
                    texture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/TiledTextures/texMagmaCloud.png").WaitForCompletion(),
                    tiling = Vector2.one,
                    offset = Vector2.zero
                },
                _RemapTex =
                {
                    //texture = _ab.LoadAsset<Texture2D>(""),
                    texture = Addressables.LoadAssetAsync<Texture2D>("RoR2/DLC3/GroundEnemies/texRampGroundEnemies.png").WaitForCompletion(),
                    tiling = Vector2.one,
                    offset = Vector2.zero
                },
                _CutoffScroll = new Vector4(1f, 3f, 8f, 8f),
                _InvFade = 0.65f,
                _SoftPower = 0.1f,
                _Boost = 0.11f,
                _AlphaBoost = 11f,
                _IntersectionStrength = 1.4f,
            },
            "matDivaSphere"
            );
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

            // clone WeakIndicator before obliterating it, since we need it for the blink effect
            var wiClone = UnityEngine.Object.Instantiate(proj_Diva.transform.Find("WeakIndicator").gameObject);
            var sphereClone = UnityEngine.Object.Instantiate(proj_Diva.transform.Find("StrongIndicator/Sphere").gameObject);

            Asset.DestroyChild(proj_Diva, "Ring");
            Asset.DestroyChild(proj_Diva, "PrepEffect");
            Asset.DestroyChild(proj_Diva, "WeakIndicator");
            Asset.DestroyChild(proj_Diva, "StrongIndicator");

            var divaVisuals = _ab.LoadAsset<GameObject>("DivaVisuals")!.InstantiateClone("DivaVisuals", false);
            divaVisuals.transform.SetParent(proj_Diva.transform, false);

            // parent the cloned WeakIndicator as Blink
            wiClone.transform.SetParent(divaVisuals.transform, false);
            wiClone.name = "Blink";
            wiClone.transform.localPosition = Vector3.zero;
            wiClone.transform.localRotation = Quaternion.identity;
            wiClone.transform.localScale = Vector3.one;
            wiClone.SetActive(true);

            sphereClone.transform.SetParent(divaVisuals.transform.Find("Hologram"), false);
            sphereClone.transform.localPosition = Vector3.zero;
            sphereClone.transform.localRotation = Quaternion.identity;
            sphereClone.transform.localScale = Vector3.one * 25f;
            sphereClone.SetActive(true);
            sphereClone.GetComponent<ObjectScaleCurve>().timeMax = 0.4f;
            sphereClone.GetComponent<MeshRenderer>().material = mat_DivaSphere;

            var diva_psr = wiClone.GetComponent<ParticleSystemRenderer>();
            diva_psr.material = mat_DivaBlink;

            var diva_ps = wiClone.GetComponent<ParticleSystem>();
            var diva_psMain = diva_ps.main;
            var diva_psEmission = diva_ps.emission;
            var diva_sot = diva_ps.sizeOverLifetime;

            // main
            diva_psMain.duration = 1f;
            diva_psMain.loop = false;
            diva_psMain.prewarm = false;
            diva_psMain.startLifetime = 0.4f;
            diva_psMain.startSize = 2f;
            diva_psMain.startColor = new Color(0.52f, 0.79f, 0.88f);
            diva_psMain.maxParticles = 16;

            // burst
            diva_psEmission.rateOverTime = 0f;
            diva_psEmission.SetBursts(
            [
                new ParticleSystem.Burst
                {
                    time = 0f,
                    count = 1,
                    probability = 1f
                }
            ]);

            // create a custom curve for the size over lifetime to make it look more interesting
            var k0 = new Keyframe(0f, 0f);
            var k1 = new Keyframe(0.2f, 0.5f);
            var k2 = new Keyframe(1f, 1f);

            k0.outTangent = 5f;
            k1.inTangent = 2f;
            k1.outTangent = 0.3f;
            k2.inTangent = 0f;

            k0.m_WeightedMode = (int)WeightedMode.Both;
            k1.m_WeightedMode = (int)WeightedMode.Both;
            k2.m_WeightedMode = (int)WeightedMode.Both;

            var curve = new AnimationCurve(k0, k1, k2);

            diva_sot.size = new ParticleSystem.MinMaxCurve(1f, curve);

            diva_ps.Clear();

            var diva_pulse = proj_Diva.AddComponent<DivaPulse>();
            diva_pulse.particleSystem = diva_ps;

            // --- rest of your pipeline unchanged ---
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

            var diva_esm1 = proj_Diva.AddComponent<EntityStateMachine>();
            diva_esm1.initialStateType = new EntityStates.SerializableEntityStateType(typeof(DivaArmingUnarmed));
            diva_esm1.mainStateType = new EntityStates.SerializableEntityStateType(typeof(DivaArmingUnarmed));
            diva_esm1.customName = "Arming";

            var diva_esm2 = proj_Diva.AddComponent<EntityStateMachine>();
            diva_esm2.initialStateType = new EntityStates.SerializableEntityStateType(typeof(WaitForStick));
            diva_esm2.mainStateType = new EntityStates.SerializableEntityStateType(typeof(Lure));
            diva_esm2.customName = "Main";

            var diva_networkEsm = proj_Diva.GetComponent<NetworkStateMachine>();
            diva_networkEsm.stateMachines = [diva_esm1, diva_esm2];

            proj_Diva.AddComponent<DivaMarker>();
            proj_Diva.AddComponent<DivaAnimator>();

            var diva_stick = proj_Diva.AddComponent<ProjectileStickOnImpactByNormal>();
            diva_stick.minGroundNormalY = 0.65f;
            diva_stick.ignoreCharacters = true;
            diva_stick.ignoreWorld = false;
            diva_stick.alignNormals = true;

            var diva_controller = proj_Diva.GetComponent<ProjectileController>();

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
