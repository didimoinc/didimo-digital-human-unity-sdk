using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Builder;
using Didimo.GLTFUtility;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Didimo
{
    [ScriptedImporter(1, "gltf")]
    public class GLTFImporter : ScriptedImporter
    {
        public ImportSettings importSettings;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);

            if (importSettings == null) importSettings = new ImportSettings();
            importSettings.animationSettings.useLegacyClips = true;
            importSettings.shaderForName = shaderName =>
            {
                materialBuilder.FindIdealShader(shaderName, out Shader shader);
                return shader;
            };
            
            Importer.ImportResult importResult = Importer.LoadFromFile(Path.GetFullPath(ctx.assetPath), importSettings, Format.GLTF);
            if (importResult.isDidimo)
            {
                GameObject root = importResult.rootObject;
                root.AddComponent<DidimoComponents>();
                root.AddComponent<DidimoAnimator>();
                DidimoEyeShadowController eyeShadowController = root.AddComponent<DidimoEyeShadowController>();
                eyeShadowController.Build(importResult.eyeShadowController);
                DidimoIrisController didimoIrisController = root.AddComponent<DidimoIrisController>();
                didimoIrisController.Build(importResult.irisController);

                root.AddComponent<DidimoMaterials>();
            
                LegacyAnimationPoseController poseController = root.AddComponent<LegacyAnimationPoseController>();
                poseController.BuildController(importResult.animationClips, importResult.resetAnimationClip, importResult.headJoint);
                
                // Save asset with reset pose Animation Clip
                List<AnimationClip> gltfAnimationClips = new List<AnimationClip>(importResult.animationClips);
                if (importResult.resetAnimationClip != null) gltfAnimationClips.Add(importResult.resetAnimationClip);
                GLTFAssetUtility.SaveToAsset(importResult.rootObject, gltfAnimationClips.ToArray(), ctx, importSettings);
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