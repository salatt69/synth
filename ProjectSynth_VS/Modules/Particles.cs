using ProjectSynth.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Modules
{
    internal static class Particles
    {
        #region Types

        internal struct TiledTextureInfo
        {
            public Texture texture;
            public Vector2 tiling;
            public Vector2 offset;

            public static readonly TiledTextureInfo Default = new()
            {
                texture = null,
                tiling = Vector2.one,
                offset = Vector2.zero
            };

            public bool IsDefault =>
                texture == null &&
                tiling == Vector2.one &&
                offset == Vector2.zero;
        }

        internal enum CullingMode
        {
            None = 0,
            Front = 1,
            Back = 2
        }

        internal enum ZTestMode
        {
            Disabled = 0,
            Never = 1,
            Less = 2,
            Equal = 3,
            LessEqual = 4,
            Greater = 5,
            NotEqual = 6,
            GreaterEqual = 7,
            Always = 8,
        }

        internal class CloudRemapInfo
        {
            public Color _TintColor = Color.white;
            public float _DisableRemapOn = 0f;
            public TiledTextureInfo _MainTex = TiledTextureInfo.Default;
            public TiledTextureInfo _RemapTex = TiledTextureInfo.Default;

            public float _InvFade = 0f;
            public float _Boost = 1f;
            public float _AlphaBoost = 1f;
            public float _AlphaBias = 0f;

            public float _UseUV1On = 0f;
            public float _FadeCloseOn = 0f;
            public float _FadeCloseDistance = 0.5f;
            public CullingMode _Cull = CullingMode.None;
            public ZTestMode _ZTest = ZTestMode.LessEqual;
            public float _DepthOffset = 0f;

            public float _CloudsOn = 0f;
            public float _CloudOffsetOn = 0f;
            public float _DistortionStrength = 0f;
            public TiledTextureInfo _Cloud1Tex = TiledTextureInfo.Default;
            public TiledTextureInfo _Cloud2Tex = TiledTextureInfo.Default;
            public Vector4 _CutoffScroll = Vector4.zero;

            public float _VertexColorOn = 1f;
            public float _VertexAlphaOn = 0f;
            public float _CalcTextureAlphaOn = 0f;
            public float _VertexOffsetOn = 0f;

            public float _FresnelOn = 0f;
            public float _SkyboxOnly = 0f;
            public float _FresnelPower = 0f;
            public float _OffsetAmount = 0f;
        }

        #endregion

        internal static readonly Shader HGCloudRemap = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGCloudRemap.shader").WaitForCompletion();

        public static Material CreateHopooCloudRemapMaterial(CloudRemapInfo info, string name = null)
        {
            // this makes not much sense, if any at all, but better safe than sorry
            info ??= new CloudRemapInfo();

            if (!HGCloudRemap)
            {
                Log.Warning("HGCloudRemap shader missing. cannot create material");
                return null;
            }

            var mat = new Material(HGCloudRemap)
            {
                name = name ?? "HGCloudRemap_Material"
            };

            ApplyCloudRemapInfo(mat, info);
            ApplyCloudRemapKeywords(mat, info);

            return mat;
        }

        private static void ApplyTiledTexture(Material mat, string propName, in TiledTextureInfo t)
        {
            if (!mat || !mat.HasProperty(propName)) return;

            mat.SetTexture(propName, t.texture);
            mat.SetTextureScale(propName, t.tiling);
            mat.SetTextureOffset(propName, t.offset);
        }

        private static void ApplyCloudRemapInfo(Material mat, CloudRemapInfo info)
        {
            if (!mat || info == null) return;

            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", info._TintColor);
            if (mat.HasProperty("_DisableRemapOn")) mat.SetFloat("_DisableRemapOn", info._DisableRemapOn);
            ApplyTiledTexture(mat, "_MainTex", info._MainTex);
            ApplyTiledTexture(mat, "_RemapTex", info._RemapTex);

            if (mat.HasProperty("_InvFade")) mat.SetFloat("_InvFade", info._InvFade);
            if (mat.HasProperty("_Boost")) mat.SetFloat("_Boost", info._Boost);
            if (mat.HasProperty("_AlphaBoost")) mat.SetFloat("_AlphaBoost", info._AlphaBoost);
            if (mat.HasProperty("_AlphaBias")) mat.SetFloat("_AlphaBias", info._AlphaBias);

            if (mat.HasProperty("_UseUV1On")) mat.SetFloat("_UseUV1On", info._UseUV1On);
            if (mat.HasProperty("_FadeCloseOn")) mat.SetFloat("_FadeCloseOn", info._FadeCloseOn);
            if (mat.HasProperty("_FadeCloseDistance")) mat.SetFloat("_FadeCloseDistance", info._FadeCloseDistance);
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", (float)info._Cull);
            if (mat.HasProperty("_ZTest")) mat.SetFloat("_ZTest", (float)info._ZTest);
            if (mat.HasProperty("_DepthOffset")) mat.SetFloat("_DepthOffset", info._DepthOffset);

            if (mat.HasProperty("_CloudsOn")) mat.SetFloat("_CloudsOn", info._CloudsOn);
            if (mat.HasProperty("_CloudOffsetOn")) mat.SetFloat("_CloudOffsetOn", info._CloudOffsetOn);
            if (mat.HasProperty("_DistortionStrength")) mat.SetFloat("_DistortionStrength", info._DistortionStrength);
            ApplyTiledTexture(mat, "_Cloud1Tex", info._Cloud1Tex);
            ApplyTiledTexture(mat, "_Cloud2Tex", info._Cloud2Tex);
            if (mat.HasProperty("_CutoffScroll")) mat.SetVector("_CutoffScroll", info._CutoffScroll);

            if (mat.HasProperty("_VertexColorOn")) mat.SetFloat("_VertexColorOn", info._VertexColorOn);
            if (mat.HasProperty("_VertexAlphaOn")) mat.SetFloat("_VertexAlphaOn", info._VertexAlphaOn);
            if (mat.HasProperty("_CalcTextureAlphaOn")) mat.SetFloat("_CalcTextureAlphaOn", info._CalcTextureAlphaOn);
            if (mat.HasProperty("_VertexOffsetOn")) mat.SetFloat("_VertexOffsetOn", info._VertexOffsetOn);

            if (mat.HasProperty("_FresnelOn")) mat.SetFloat("_FresnelOn", info._FresnelOn);
            if (mat.HasProperty("_SkyboxOnly")) mat.SetFloat("_SkyboxOnly", info._SkyboxOnly);
            if (mat.HasProperty("_FresnelPower")) mat.SetFloat("_FresnelPower", info._FresnelPower);
            if (mat.HasProperty("_OffsetAmount")) mat.SetFloat("_OffsetAmount", info._OffsetAmount);
        }

        private static void ApplyCloudRemapKeywords(Material mat, CloudRemapInfo info)
        {
            if (!mat || info == null) return;

            static bool On(float v) => v > 0.5f;
            static void Set(Material m, string kw, bool enabled)
            {
                if (enabled) m.EnableKeyword(kw);
                else m.DisableKeyword(kw);
            }

            Set(mat, "DISABLEREMAP", On(info._DisableRemapOn));
            Set(mat, "FADECLOSE", On(info._FadeCloseOn));
            Set(mat, "FRESNEL", On(info._FresnelOn));
            Set(mat, "SKYBOX_ONLY", On(info._SkyboxOnly));

            Set(mat, "USE_UV1", On(info._UseUV1On));
            Set(mat, "USE_CLOUDS", On(info._CloudsOn));
            Set(mat, "CLOUDOFFSET", On(info._CloudOffsetOn));

            Set(mat, "VERTEXCOLOR", On(info._VertexColorOn));
            Set(mat, "VERTEXALPHA", On(info._VertexAlphaOn));
            Set(mat, "VERTEXOFFSET", On(info._VertexOffsetOn));

            Set(mat, "SOFTPARTICLES_ON", info._InvFade > 0.0001f);
        }
    }
}
