using System;
using System.Collections.Generic;
using System.IO;
using Didimo.Builder;
using Didimo.Builder.GLTF;
using Didimo.Core.Config;
using Didimo.Core.Utility;
using Didimo.GLTFUtility;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Didimo.Core.Editor
{
    [ScriptedImporter(1, "gltf", importQueueOffset: 100)]
    public class GLTFImporter : ScriptedImporter
    {
        public ImportSettings importSettings;

        public static bool CanImportDidimos()
        {
#if !USING_UNITY_URP
            return false;
#else
            return true;
#endif
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!CanImportDidimos())
            {
                return;
            }

            ShaderResources shaderResources = ResourcesLoader.ShaderResources();

            if (shaderResources == null)
            {
                Debug.LogError("Shader resources was null.");
                return;
            }

            //MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);
            MaterialBuilder materialBuilder = new UniversalRenderingPipelineMaterialBuilder();

            if (importSettings == null) importSettings = new ImportSettings();
            importSettings.animationSettings.useLegacyClips = true;
            importSettings.shaderForName = shaderName =>
            {
                materialBuilder.FindIdealShader(shaderName, out Shader shader);
                return shader;
            };

            Importer.ImportResult importResult =
                Importer.LoadFromFile(Path.GetFullPath(ctx.assetPath), importSettings, Format.GLTF);
            if (importResult.isDidimo)
            {
                GLTFBuildData.BuildFromScriptedImporter(importResult);

                // Save asset with reset pose Animation Clip
                List<AnimationClip> gltfAnimationClips = new List<AnimationClip>(importResult.animationClips);
                if (importResult.resetAnimationClip != null) gltfAnimationClips.Add(importResult.resetAnimationClip);
                GLTFAssetUtility.SaveToAsset(importResult.rootObject, gltfAnimationClips.ToArray(), ctx,
                    importSettings);
            }
            else
            {
                // Save asset
                GLTFAssetUtility.SaveToAsset(importResult.rootObject, importResult.animationClips, ctx, importSettings);
            }
        }

        public override bool SupportsRemappedAssetType(Type type)
        {
            return type == typeof(Material);
        }
    }
}