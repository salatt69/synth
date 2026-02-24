using HG;
using ProjectSynth.Core;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Modules
{
    // Thanks to https://github.com/risk-of-thunder/RoR2StubbedShaders, materials can be configured directly in Unity.
    // At runtime, we only need to swap the stubbed shader with Hopoo's original shader.
    internal static class Materials
    {
        private static readonly List<Material> cachedMaterials = [];

        internal static readonly Shader HGStandard = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGStandard.shader").WaitForCompletion();
        internal static readonly Shader HGCloudRemap = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGCloudRemap.shader").WaitForCompletion();
        internal static readonly Shader HGIntersectionCloudRemap = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGIntersectionCloudRemap.shader").WaitForCompletion();

        public static Material ConvertStubbedShaderToHopoo_Standart(this Material mat) => ConvertToHopoo(mat, HGStandard);
        public static Material ConvertStubbedShaderToHopoo_CloudRemap(this Material mat) => ConvertToHopoo(mat, HGCloudRemap);
        public static Material ConvertStubbedShaderToHopoo_Intersection(this Material mat) => ConvertToHopoo(mat, HGIntersectionCloudRemap);
        private static Material ConvertToHopoo(Material mat, Shader newShader)
        {
            if (cachedMaterials.Contains(mat)) return mat;
            mat.shader = newShader;
            cachedMaterials.Add(mat);
            return mat;
        }
    }
}