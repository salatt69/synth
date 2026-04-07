using ProjectSynth.Mod;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSynth.Modules
{
    internal static class Skins
    {
        internal static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] defaultRendererInfos, GameObject root, UnlockableDef unlockableDef = null)
        {
            var skinDefParams = new RoR2.SkinDefParams
            {
                rendererInfos = (CharacterModel.RendererInfo[])defaultRendererInfos.Clone(),
                gameObjectActivations = Array.Empty<RoR2.SkinDefParams.GameObjectActivation>(),
                meshReplacements = Array.Empty<RoR2.SkinDefParams.MeshReplacement>(),
                projectileGhostReplacements = Array.Empty<RoR2.SkinDefParams.ProjectileGhostReplacement>(),
                minionSkinReplacements = Array.Empty<RoR2.SkinDefParams.MinionSkinReplacement>()
            };
            var skinDef = ScriptableObject.CreateInstance<RoR2.SkinDef>();
            skinDef.skinDefParams = skinDefParams;
            skinDef.name = skinName;
            skinDef.icon = skinIcon;
            skinDef.unlockableDef = unlockableDef;
            skinDef.rootObject = root;
            skinDef.baseSkins = Array.Empty<SkinDef>();
            skinDef.nameToken = skinName;
            return skinDef;
        }

        private static CharacterModel.RendererInfo[] GetRendererMaterials(CharacterModel.RendererInfo[] defaultRenderers, params Material[] materials)
        {
            CharacterModel.RendererInfo[] newRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(newRendererInfos, 0);

            for (int i = 0; i < newRendererInfos.Length; i++)
            {
                try
                {
                    newRendererInfos[i].defaultMaterial = materials[i];
                }
                catch
                {
                    Log.Error("error adding skin rendererinfo material. make sure you're not passing in too many");
                }
            }

            return newRendererInfos;
        }
        /// <summary>
        /// pass in strings for mesh assets in your bundle. pass the same amount and order based on your rendererinfos, filling with null as needed
        /// <code>
        /// myskindef.meshReplacements = Modules.Skins.getMeshReplacements(defaultRenderers,
        ///    "meshHenrySword",
        ///    null,
        ///    "meshHenry");
        /// </code>
        /// </summary>
        /// <param name="assetBundle">your skindef's rendererinfos to access the renderers</param>
        /// <param name="defaultRendererInfos">your skindef's rendererinfos to access the renderers</param>
        /// <param name="meshes">name of the mesh assets in your project</param>
        /// <returns></returns>
        internal static RoR2.SkinDefParams.MeshReplacement[] GetMeshReplacements(AssetBundle assetBundle, CharacterModel.RendererInfo[] defaultRendererInfos, params string[] meshes)
        {

            List<RoR2.SkinDefParams.MeshReplacement> meshReplacements = new List<RoR2.SkinDefParams.MeshReplacement>();

            for (int i = 0; i < defaultRendererInfos.Length; i++)
            {
                if (string.IsNullOrEmpty(meshes[i]))
                    continue;

                meshReplacements.Add(
                new RoR2.SkinDefParams.MeshReplacement
                {
                    renderer = defaultRendererInfos[i].renderer,
                    mesh = assetBundle.LoadAsset<Mesh>(meshes[i])
                });
            }

            return meshReplacements.ToArray();
        }
    }
}