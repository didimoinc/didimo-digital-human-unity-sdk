using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Builder;
using Didimo.Core.Utility;
using Didimo.Editor.Inspector;
using GLTFast.Editor;
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
            // // If we just imported a shared texture, re-import didimos that need the textures
            // if (importedAssets.FirstOrDefault(asset =>
            //         Path.GetFullPath(asset).StartsWith(Path.GetFullPath(GLTFImage.GltfImageSharedPath))) != null)
            // {
            //     List<string> allDidimoPaths =
            //         Directory.GetFiles("Assets", "*.gltf", SearchOption.AllDirectories).ToList();
            //     foreach (var packageRoot in new[] {"Packages", "Library/PackageCache"})
            //     {
            //         foreach (var package in Directory.EnumerateDirectories(packageRoot))
            //         {
            //             if (package.Replace("\\", "/").StartsWith(packageRoot + "/com.didimo"))
            //             {
            //                 allDidimoPaths.AddRange(Directory.GetFiles(package, "*.gltf", SearchOption.AllDirectories));
            //             }
            //         }
            //     }
            //
            //     foreach (string didimoPath in allDidimoPaths)
            //     {
            //         GLTFImporter importer = AssetImporter.GetAtPath(didimoPath) as GLTFImporter;
            //         if (importer != null && importer.importSettings.needsReimportForTextures)
            //         {
            //             importer.importSettings.needsReimportForTextures = false;
            //             importer.SaveAndReimport();
            //         }
            //     }
            // }
        }
    }

#if USE_DIDIMO_CUSTOM_FILE_EXTENSION
    [ScriptedImporter(GLTF_IMPORTER_VERSION, new string[]{"gltfd", "glbd"}, importQueueOffset: 100)]
#else
    [ScriptedImporter(GLTF_IMPORTER_VERSION, new string[] {"gltf", "glb"}, importQueueOffset: 100)]
#endif

    public class GLTFImporter : ScriptedImporter
    {
        public const int GLTF_IMPORTER_VERSION = 24;

        public ImportSettings importSettings;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);

            if (importSettings == null) importSettings = new ImportSettings();
            // importSettings.animationSettings.useLegacyClips = true;
            importSettings.materialForName = shaderName =>
            {
                materialBuilder.FindIdealMaterial(shaderName, out Material material);
                return material;
            };

            DidimoImporterJsonConfig didimoImporterJsonConfig =
                DidimoImporterJsonConfigUtils.GetConfigAtFolder(Path.GetDirectoryName(ctx.assetPath)!);
            importSettings.postMaterialCreate = material => materialBuilder.PostMaterialCreate(material);


            GltfImporter importer = new GltfImporter();
            importer.importSettings = new();

            // Didimo glTFs only contain facial poses as animation, which must be imported as legacy
            if (didimoImporterJsonConfig != null)
            {
                importer.importSettings.animationMethod = GLTFast.ImportSettings.AnimationMethod.Legacy;
                importer.editorImportSettings = new EditorImportSettings
                {
                    generateSecondaryUVSet = importSettings.generateLightmapUVs
                };
            }

            GltfImporter.ImportResult importResult = importer.OnImportAsset(ctx, didimoImporterJsonConfig == null);

            if ( /* importResult.isDidimo ||*/ didimoImporterJsonConfig != null)
            {
                importSettings.isDidimo = true;
                
                DidimoImporterJsonConfigUtils.SetupDidimoForEditor(ctx.mainObject as GameObject,
                    didimoImporterJsonConfig, ctx, assetPath, importResult.animationClips, "",
                    importResult.resetClip,
                    createdObject =>
                    {
                        ctx.AddObjectToAsset(createdObject.name, createdObject);
                    });

                SetAvatar(importSettings, ctx.mainObject as GameObject);
                if (importSettings.avatar != null)
                {
                    ctx.AddObjectToAsset(importSettings.avatar.name, importSettings.avatar);
                }
            }
        }

        private static void SetAvatar(ImportSettings importSettings, GameObject root)
        {
            if ((importSettings.animationType == ImportSettings.AnimationType.Generic ||
                 importSettings.animationType == ImportSettings.AnimationType.Humanoid) &&
                importSettings.avatarDefinition != ImportSettings.AvatarDefinition.NoAvatar)
            {
                Animator animator = ComponentUtility.GetOrAdd<Animator>(root);

                if (importSettings.avatarDefinition == ImportSettings.AvatarDefinition.CreateFromThisModel)
                {
                    if (importSettings.animationType == ImportSettings.AnimationType.Generic)
                    {
                        importSettings.avatar = AvatarBuilder.BuildGenericAvatar(root, "");
                    }
                    else if (importSettings.animationType == ImportSettings.AnimationType.Humanoid)
                    {
                        BuildHumanoidAvatar(root, importSettings);
                    }
                }

                animator.avatar = importSettings.avatar;
                animator.avatar.name = "avatar";
            }
        }

        private static void BuildHumanoidAvatar(GameObject root, ImportSettings importSettings)
        {
            HumanDescription humanDescription = new HumanDescription();
            Transform[] transforms = root.GetComponentsInChildren<Transform>();
            List<SkeletonBone> skeletonBones = new List<SkeletonBone>(transforms.Length);

            Avatar defaultAvatar = ResourcesLoader.DidimoDefaultAvatar();

            for (int i = 0; i < transforms.Length; i++)
            {
                SkeletonBone skeletonBone = new SkeletonBone();
                skeletonBone.name = transforms[i].name;
                try
                {
                    skeletonBone.rotation = defaultAvatar.humanDescription.skeleton
                        .First(s => s.name == skeletonBone.name).rotation;
                }
                catch (Exception)
                {
                    // Debug.LogWarning(e);
                    // Skip this bone
                    continue;
                }

                skeletonBone.position = transforms[i].localPosition;
                //skeletonBone[i].rotation = transforms[i].localRotation;
                skeletonBone.scale = transforms[i].localScale;
                skeletonBones.Add(skeletonBone);
            }

            humanDescription.skeleton = skeletonBones.ToArray();
            //humanDescription.skeleton = defaultAvatar.humanDescription.skeleton.Clone() as SkeletonBone[];
            humanDescription.human = defaultAvatar.humanDescription.human;

            importSettings.avatar = AvatarBuilder.BuildHumanAvatar(root, humanDescription);
        }

        public override bool SupportsRemappedAssetType(Type type)
        {
            return type == typeof(Material);
        }
    }
}