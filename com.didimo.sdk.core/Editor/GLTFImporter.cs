using System;
using System.Collections;
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

namespace Didimo.Core.Editor
{
    public class GLTFPreProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths, bool didDomainReload)
        {
            // If we just imported a shared texture, re-import didimos that need the textures
            if (importedAssets.FirstOrDefault(asset =>
                    Path.GetFullPath(asset).StartsWith(Path.GetFullPath(GLTFImage.GltfImageSharedPath))) != null)
            {
                List<string> allDidimoPaths =
                    Directory.GetFiles("Assets", "*.gltf", SearchOption.AllDirectories).ToList();
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
    [ScriptedImporter(GLTF_IMPORTER_VERSION, new string[] {"gltf", "glb"}, importQueueOffset: 100)]
#endif

    public class GLTFImporter : ScriptedImporter
    {
        public const int GLTF_IMPORTER_VERSION = 17;

        public ImportSettings importSettings;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            ShaderResources shaderResources = ResourcesLoader.ShaderResources();

            GameObject rootNode = null;

            try
            {
                if (shaderResources == null)
                {
                    Debug.LogError("Shader resources was null.");
                    return;
                }

                MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);

                if (importSettings == null) importSettings = new ImportSettings();
                importSettings.animationSettings.useLegacyClips = true;
                importSettings.shaderForName = shaderName =>
                {
                    materialBuilder.FindIdealShader(shaderName, out Shader shader);
                    return shader;
                };

                DidimoImporterJsonConfig didimoImporterJsonConfig =
                    DidimoImporterJsonConfigUtils.GetConfigAtFolder(Path.GetDirectoryName(ctx.assetPath)!);
                importSettings.postMaterialCreate = material => materialBuilder.PostMaterialCreate(material);

                Importer.ImportResult importResult = Importer.LoadFromFile(Path.GetFullPath(ctx.assetPath),
                    importSettings,
                    gltfObject =>
                    {
                        if (didimoImporterJsonConfig != null)
                        {
                            // We will be settings the materials and importing the textures after importing the gltf
                            gltfObject.images = null;
                            gltfObject.textures = null;
                            gltfObject.materials = null;
                            gltfObject.extensions ??= new GLTFObject.Extensions();
                            gltfObject.extensions.Didimo = new DidimoExtension();
                        }
                    }, Format.GLTF);
                rootNode = importResult.rootObject;

                if (importResult.isDidimo || didimoImporterJsonConfig != null)
                {
                    importResult.isDidimo = true;
                    importSettings.isDidimo = true;

                    if (didimoImporterJsonConfig != null)
                    {
                        DidimoImporterJsonConfigUtils.SetupDidimoForEditor(importResult.rootObject,
                            didimoImporterJsonConfig, assetPath, importResult.animationClips,
                            importResult.resetAnimationClip, null);
                        GLTFBuildData.SetAvatar(importSettings, importResult.rootObject);
                    }
                    else
                    {
                        GLTFBuildData.BuildFromScriptedImporter(importResult, importSettings, ctx.assetPath);
                    }

                    // Save asset with reset pose Animation Clip
                    List<AnimationClip> gltfAnimationClips = importResult.animationClips.ToList();
                    if (importResult.resetAnimationClip != null)
                        gltfAnimationClips.Add(importResult.resetAnimationClip);
                    GLTFAssetUtility.SaveToAsset(importResult.rootObject, gltfAnimationClips.ToArray(), ctx,
                        importSettings);
                }
                else
                {
                    // Save asset
                    GLTFAssetUtility.SaveToAsset(importResult.rootObject, importResult.animationClips, ctx,
                        importSettings);
                }

            }
            catch
            {
                if (rootNode != null)
                {
                    DestroyImmediate(rootNode);
                }

                throw;
            }
        }

        public override bool SupportsRemappedAssetType(Type type)
        {
            return type == typeof(Material);
        }
    }
}