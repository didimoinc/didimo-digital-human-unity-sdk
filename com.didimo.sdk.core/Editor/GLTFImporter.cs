using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Builder;
using Didimo.Builder.GLTF;
using Didimo.Core.Config;
using Didimo.Core.Utility;
using Didimo.GLTFUtility;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Object = System.Object;

namespace Didimo.Core.Editor
{
    public class GLTFPreProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // If we just imported a shared texture, re-import didimos that need the textures
            if (importedAssets.FirstOrDefault(asset => Path.GetFullPath(asset).StartsWith(Path.GetFullPath(GLTFImage.GltfImageSharedPath))) != null)
            {
                    
                List<string> allDidimoPaths = Directory.GetFiles("Assets", "*.gltf", SearchOption.AllDirectories).ToList();
                foreach (var packageRoot in new[] {"Packages", "Library/PackageCache"})
                {
                    foreach (var package in Directory.EnumerateDirectories(packageRoot))
                    {
                        if (package.Replace("\\", "/").StartsWith(packageRoot + "/com.didimo"))
                        {
                            allDidimoPaths.AddRange(Directory.GetFiles(package, "*.gltf", SearchOption.AllDirectories));
                        }
                    }
                }

                foreach (string didimoPath in allDidimoPaths)
                {
                    GLTFImporter importer = AssetImporter.GetAtPath(didimoPath) as GLTFImporter;
                    if (importer != null && importer.importSettings.needsReimportForTextures)
                    {
                        importer.importSettings.needsReimportForTextures = false;
                        importer.SaveAndReimport();
                    }
                }
            }
        }
    }

#if USE_DIDIMO_CUSTOM_FILE_EXTENSION
    [ScriptedImporter(GLTF_IMPORTER_VERSION, new string[]{"gltfd", "glbd"}, importQueueOffset: 100)]
#else
    [ScriptedImporter(GLTF_IMPORTER_VERSION,  new string[]{"gltf", "glb"}, importQueueOffset: 100)]
#endif

    public class GLTFImporter : ScriptedImporter
    {
        public const int GLTF_IMPORTER_VERSION = 12;

        public ImportSettings importSettings;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            ShaderResources shaderResources = ResourcesLoader.ShaderResources();

            if (shaderResources == null)
            {
                Debug.LogError("Shader resources was null.");
                return;
            }

            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);
            //MaterialBuilder materialBuilder = new UniversalRenderingPipelineMaterialBuilder();

            if (importSettings == null) importSettings = new ImportSettings();
            importSettings.animationSettings.useLegacyClips = true;
            importSettings.shaderForName = shaderName =>
            {
                materialBuilder.FindIdealShader(shaderName, out Shader shader);
                return shader;
            };

            importSettings.postMaterialCreate = material => materialBuilder.PostMaterialCreate(material);

            Importer.ImportResult importResult = Importer.LoadFromFile(Path.GetFullPath(ctx.assetPath), importSettings, Format.GLTF);
            if (importResult.isDidimo)
            {
                importSettings.isDidimo = true;
                GLTFBuildData.BuildFromScriptedImporter(importResult, importSettings, ctx.assetPath);
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

        public override bool SupportsRemappedAssetType(Type type) { return type == typeof(Material); }
    }
}