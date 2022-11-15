using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Core.Config;
using Didimo.Core.Deformables;
using Didimo.Core.Utility;
// using Didimo.GLTFUtility;
using Didimo.Speech;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AssetImporters;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using BodyPart = Didimo.Core.Utility.DidimoParts.BodyPart;
using Object = UnityEngine.Object;

namespace Didimo.Builder
{
    public class DidimoImporterJsonConfigUtils
    {
        protected static string ASSET_ROOT_PATH         = "Assets/";
        protected static string PACKAGES_ROOT_PATH      = "Packages/";
        protected static string PACKAGE_CACHE_ROOT_PATH = "Library/PackageCache/";

        public const string JSON_NAME = "avatar_info.json";

        public static string GetConfigFilePathForFolder(string folder)
        {
            return Path.Combine(folder, JSON_NAME);
        }
        public static DidimoImporterJsonConfig GetConfigAtFolder(string folder)
        {
            DidimoImporterJsonConfig jsonConfig = null;
            if (CheckIfJsonExists(folder))
            {
                jsonConfig = JsonConvert.DeserializeObject<DidimoImporterJsonConfig>(
                    File.ReadAllText(GetConfigFilePathForFolder(folder)));
            }

            return jsonConfig;
        }

        public static bool CheckIfJsonExists(string folder)
        {
            return File.Exists(GetConfigFilePathForFolder(folder));
        }


#if UNITY_EDITOR
        public static void SetupDidimoForEditor(GameObject didimoGameObject,
            DidimoImporterJsonConfig importerJsonConfig,
            AssetImportContext context,
            string assetPath, AnimationClip[] animationClips, string didimoKey,
            AnimationClip resetAnimationClip, Action<Object> onObjectCreated = null)
        {
            RenderPipelineMaterials renderPipelineMaterials = ResourcesLoader.GetRenderPipelineMaterials();
            string renderPipelineMaterialsPath = ResourcesLoader.GetRenderPipelineMaterialsPath();

            context.DependsOnArtifact(renderPipelineMaterialsPath);
            
            if (renderPipelineMaterials == null)
            {
                Debug.LogWarning($"{assetPath}: RenderPipelineMaterials was null.");
                return;
            }
            
            context.AddObjectToAsset(renderPipelineMaterials.name, AssetDatabase.LoadAssetAtPath<Object>(renderPipelineMaterialsPath));

            // The following code causes issues with import, not sure why. According to Unity documentation, it should work
            // string configPath = GetConfigFilePathForFolder(Path.GetDirectoryName(assetPath)).Replace("\\", "/");
            // context.DependsOnArtifact(configPath);
            // Object avatarInfo = AssetDatabase.LoadAssetAtPath<Object>(configPath);
            // if (avatarInfo != null)
            // {
            //     context.AddObjectToAsset("avatar_info", avatarInfo);
            // }

            IEnumerator en = SetupDidimoImpl(false, didimoGameObject, importerJsonConfig, assetPath, animationClips,
                resetAnimationClip, didimoKey, onObjectCreated);
                while (en.MoveNext())
            {
            }
        }
#endif

        public static IEnumerator SetupDidimoForRuntime(GameObject didimoGameObject,
            DidimoImporterJsonConfig importerJsonConfig,
            string assetPath, AnimationClip[] animationClips, AnimationClip resetAnimationClip, string didimoKey,
            Action<Object> onObjectCreated = null)
        {
            if (!Application.isPlaying)
            {
                throw new Exception("SetupDidimoForRuntime can only be called during play mode.");
            }

            yield return SetupDidimoImpl(true, didimoGameObject, importerJsonConfig, assetPath, animationClips,
                resetAnimationClip, didimoKey, onObjectCreated);
        }

        private static Dictionary<string, Texture2D> LoadTextures(bool runtimeImport, string basePath, DidimoImporterJsonConfig importerJsonConfig)
        {
            /// TODO: Shared textures, texture cache, textures form web URL
            List<string> texturePaths = new();
            Dictionary<string, Texture2D> result = new Dictionary<string, Texture2D>();
            
            foreach (var mesh in importerJsonConfig.meshList)
            {
                foreach (KeyValuePair<string, string> texture in mesh.textures)
                {
                    texturePaths.Add(texture.Value);
                }
            }

            // remove duplicates
            texturePaths = texturePaths.Distinct().ToList();
            if (runtimeImport)
            {
                foreach (var texturePath in texturePaths)
                {
                    string pathFormatted = Path.GetFullPath(Path.Combine(basePath, texturePath)).Replace("\\", "/");
    
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, false);
                    byte[] textureData = File.ReadAllBytes(new Uri(pathFormatted).AbsolutePath);
                    tex.LoadImage(textureData);
                    result.Add(texturePath, tex);
                }

                return result;
            }

#if UNITY_EDITOR
            foreach (var texturePath in texturePaths)
            {
                string pathFormatted = Path.GetFullPath(Path.Combine(basePath, texturePath)).Replace("\\", "/");
                List<string> rootPaths = new List<string> {ASSET_ROOT_PATH, PACKAGES_ROOT_PATH, PACKAGE_CACHE_ROOT_PATH};

                foreach (string rootPath in rootPaths)
                {
                    string rootPathAbsl = Path.GetFullPath(rootPath).Replace("\\", "/");
                    if (pathFormatted.StartsWith(rootPathAbsl))
                    {
                        // In case path contains "../"
                        // pathFormatted = Path.GetFullPath(pathFormatted);
                        pathFormatted = pathFormatted.Substring(rootPathAbsl.Length);
                        // Package Caches have name@hash, so we need to strip that out
                        if (rootPath == PACKAGE_CACHE_ROOT_PATH)
                        {
                            string cachedPackageFolderName = pathFormatted.Split('/')[0];
                            string packageFolderName = cachedPackageFolderName.Substring(0, cachedPackageFolderName.LastIndexOf('@'));
                            pathFormatted = $"{PACKAGES_ROOT_PATH}/{packageFolderName}/" + pathFormatted.Substring(cachedPackageFolderName.Length);
                        }
                        else
                        {
                            pathFormatted = rootPath + pathFormatted;
                        }

                        // Load textures from asset database if we can
                        Texture2D assetTexture = AssetDatabase.LoadAssetAtPath(pathFormatted, typeof(Texture2D)) as Texture2D;

                        if (assetTexture != null)
                        {
                            // onObjectCreated?.Invoke(assetTexture);
                            result.Add(texturePath, assetTexture);
                            // TextureImporter textureImporter = AssetImporter.GetAtPath(pathFormatted) as TextureImporter;
                            // textureImporter!.sRGBTexture = !linear;
                            // textureImporter!.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                            // // We only save them at the end, to prevent errors of loading this texture again after calling import
                            // // textureImporter.SaveAndReimport();
                            // AssetDatabase.WriteImportSettingsIfDirty(pathFormatted);

                            // onFinish(assetTexture);
                            // if (onProgress != null) onProgress(1f);
                        }
                        else
                        {
                            Debug.LogError($"Unable to get texture {pathFormatted}");
                        }
                    }
                }
            }
#endif
            return result;
        }

        private static void SetTextureImporterProperties(bool runtime, ref Texture2D texture, bool linear, bool normalMap)
        {
            if(runtime){
                // TODO: How to import normal maps in runtime?
                if (linear)
                {
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
                    tex.LoadRawTextureData(texture.GetRawTextureData());
                }

                return;
            }
            
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            textureImporter!.sRGBTexture = !linear;
            textureImporter!.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            // We only save them at the end, to prevent errors of loading this texture again after calling import
            // textureImporter.SaveAndReimport();
            AssetDatabase.WriteImportSettingsIfDirty(assetPath);
#endif
        }
        
        private static IEnumerator SetupDidimoImpl(bool runtime, GameObject didimoGameObject,
            DidimoImporterJsonConfig importerJsonConfig,
            string assetPath, AnimationClip[] animationClips,
            AnimationClip resetAnimationClip, string didimoKey, Action<Object> onObjectCreated)
        {

            Dictionary<string, Texture2D> textureMap = new ();
            // if (importTextures)
            // {
            // TODO: Make this async
                textureMap = LoadTextures(runtime, Path.GetDirectoryName(assetPath), importerJsonConfig);
            // }

            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);

            DidimoComponents didimoComponents = ComponentUtility.GetOrAdd<DidimoComponents>(didimoGameObject);
            didimoGameObject.AddComponent<DidimoParts>();
            didimoComponents.DidimoKey = didimoKey;

            BuildDidimoParts(didimoComponents, importerJsonConfig);

            DidimoDeformables deformables = didimoGameObject.AddComponent<DidimoDeformables>();
            deformables.CacheHairOffsets();
            deformables.deformationFile = DidimoDeformables.GetDeformationFile(Path.GetDirectoryName(assetPath) + "/");

            
            foreach (MeshProperties meshProperties in importerJsonConfig.meshList)
            {
                BodyPart bodyPart = BodyPart.Unknown;
                BodyPartMap.TryGetValue(meshProperties.bodyPart, out bodyPart);

                if (bodyPart == BodyPart.HairMesh)
                {
                    HandleHair(didimoComponents, meshProperties, onObjectCreated);
                    continue;
                }

                Renderer bodyPartRenderer = didimoComponents.Parts.BodyPartToRenderer(bodyPart);
                if (bodyPartRenderer == null) continue;

                materialBuilder.FindIdealMaterialForBodyPart(bodyPart, out Material material);
                if (material == null)
                {
                    Debug.LogWarning($"Material for body part '{bodyPart}' was null.");
                    continue;
                }
                material.name = meshProperties.meshName + "_MAT";
                onObjectCreated?.Invoke(material);
                if (meshProperties.textures != null)
                {
                    foreach (var t in meshProperties.textures)
                    {
                        // int textureIndex = texturePaths.IndexOf(texturePaths.FirstOrDefault(texturePath =>
                        //     texturePath == t.Value));

                        if (BodyPartIds.TryGetValue(bodyPart,
                                out Dictionary<string, string> ids))
                        {
                            if (ids.TryGetValue(t.Key, out string mapping))
                            {
                                bool normalMap = NormalMaps.Contains(t.Key);
                                bool sRGB = SRGBMaps.Contains(t.Key);

                                // Debug.Log(t.Value);
                                Texture2D texture = textureMap[t.Value];
                                material.SetTexture(mapping, texture);

                                SetTextureImporterProperties(runtime, ref texture, !sRGB, normalMap);
                                // IEnumerator en = textureTask.Result[textureIndex].GetTextureCached(!sRGB, normalMap,
                                //     texture2D => { material.SetTexture(mapping, texture2D); });
                                // while (en.MoveNext())
                                // {
                                // }
                            }
                        }
                    }
                }

                bodyPartRenderer.material = material;
            }

            didimoGameObject.AddComponent<DidimoMaterials>();
            didimoGameObject.AddComponent<DidimoAnimator>();

            Transform headJoint = didimoComponents.Parts.HeadJoint;
            LegacyAnimationPoseController poseController =
                didimoGameObject.AddComponent<LegacyAnimationPoseController>();
            poseController.BuildController(animationClips, resetAnimationClip, headJoint);

            didimoGameObject.AddComponent<DidimoSpeech>();

            didimoGameObject.AddComponent<DidimoEyeShadowController>();
            didimoGameObject.AddComponent<DidimoIrisController>();
            
            didimoComponents.Parts.LeftEyeMeshRenderer.updateWhenOffscreen = true;
            didimoComponents.Parts.RightEyeMeshRenderer.updateWhenOffscreen = true;
            if (didimoComponents.Parts.EyeLashesMeshRenderer != null)
            {
                didimoComponents.Parts.EyeLashesMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }

            yield break;
        }

        private static void HandleHair(DidimoComponents didimoComponents, MeshProperties hairProperties,
            Action<Object> onObjectCreated)
        {
            if (didimoComponents.Parts.HairMesh == null) return;
            
            GameObject didimoHair = didimoComponents.Parts.HairMesh.gameObject;

            List<Deformable> deformableDatabase = DeformableUtils.GetAllDeformables();
            GameObject prefabHair = deformableDatabase.FirstOrDefault(d => d.name.Equals(hairProperties.meshId))
                ?.gameObject;
            if (prefabHair != null)
            {
                SkinnedMeshRenderer targetSkinMR = didimoHair.GetComponent<SkinnedMeshRenderer>();

                List<Material> newMaterials = new List<Material>();
                foreach (Material mat in prefabHair.GetComponentsInChildren<MeshRenderer>().First().sharedMaterials)
                {
                    Material newMaterial = new Material(mat);
                    onObjectCreated?.Invoke(newMaterial);
                    newMaterials.Add(newMaterial);
                }

                targetSkinMR.sharedMaterials = newMaterials.ToArray();

                Mesh prefabMesh = prefabHair.GetComponentsInChildren<MeshFilter>().First().sharedMesh;
                Mesh didimoMesh = targetSkinMR.sharedMesh;
                Mesh deformedMesh = didimoComponents.Deformables.DeformMesh(prefabMesh, hairProperties.meshName);

                // The hair mesh that comes from the DGP can have different number of vertices from the hair mesh on the SDK
                // We need to update the bone weights accordingly
                // This assumes all bone weights are the same
                BoneWeight[] boneWeights = didimoMesh.boneWeights;
                if (boneWeights.Length > prefabMesh.vertices.Length)
                {
                    // Remove excess weights
                    boneWeights = boneWeights.SkipLast(boneWeights.Length - prefabMesh.vertices.Length).ToArray();
                }
                else if (boneWeights.Length < prefabMesh.vertices.Length)
                {
                    // Add missing weights
                    int weightsNum = boneWeights.Length;
                    int missingWeightsNum  = prefabMesh.vertices.Length - weightsNum;
                    Array.Resize(ref boneWeights, prefabMesh.vertices.Length);
                    Array.Fill(boneWeights, boneWeights.First(),weightsNum, missingWeightsNum);
                }
                
                deformedMesh.boneWeights = boneWeights;
                deformedMesh.bindposes = didimoMesh.bindposes;
                MeshUtils.CopyMesh(deformedMesh, targetSkinMR.sharedMesh);
                
                Hair hair = didimoHair.AddComponent<Hair>();
                hair.SetPreset(hairProperties.color);
            }
            else
            {
                Debug.Log("Prefab was null");
            }
        }


        const string HEAD_JOINT_NAME = "Head";

        public static readonly Dictionary<string, string> TextureMapToShaderProperty = new()
        {
            {"AO", "_AmbientOcclusionMap"},
            {"albedo", "_BaseMap"},
            {"cavity", "_CavityMap"},
            {"micronormal", "_NormalMicro"},
            {"normal", "_NormalMap"},
            {"roughness", "_RoughnessMap"},
            {"specular", "_SpecularMap"},
            {"SSS_AO", "_SssAoMask"},
            {"translucency", "_TransMap"},
            {"zbias", "_ZBiasMap"},
            {"mask", "_AlphaMask"},
            {"albedo_opacity", "_BaseMap"},
            {"metal_roughness", "_MetallicGlossMap"},
            {"clothing_mask", "_AlphaMask"}
        };

        public static string GetTextureKey(string texturePath, DidimoImporterJsonConfig importerJsonConfig)
        {
            string texture = Path.GetFileName(texturePath);

            foreach (MeshProperties m in importerJsonConfig.meshList)
            {
                foreach (var pair in m.textures)
                {
                    if (pair.Value.Equals(texture))
                    {
                        return pair.Key;
                    }
                }
            }

            return null;
        }

        public static readonly Dictionary<string, string> EyelashTextureMapToShaderProperty = new()
        {
            {"albedo_opacity", "baseColorTexture"}
        };

        public static readonly Dictionary<BodyPart, Dictionary<string, string>> BodyPartIds = new()
        {
            {BodyPart.LeftEyeMesh, TextureMapToShaderProperty},
            {BodyPart.RightEyeMesh, TextureMapToShaderProperty},
            {BodyPart.BodyMesh, TextureMapToShaderProperty},
            {BodyPart.HeadMesh, TextureMapToShaderProperty},
            {BodyPart.MouthMesh, TextureMapToShaderProperty},
            {BodyPart.EyeLashesMesh, EyelashTextureMapToShaderProperty},
            {BodyPart.ClothingMesh, TextureMapToShaderProperty}
        };

        public static readonly Dictionary<string, BodyPart> BodyPartMap = new Dictionary<string, BodyPart>
        {
            {"body", BodyPart.BodyMesh},
            {"clothing", BodyPart.ClothingMesh},
            {"hair", BodyPart.HairMesh},
            {"head", BodyPart.HeadMesh},
            {"leftEye", BodyPart.LeftEyeMesh},
            {"rightEye", BodyPart.RightEyeMesh},
            {"mouth", BodyPart.MouthMesh},
            {"eyelashes", BodyPart.EyeLashesMesh}
        };

        public static readonly string[] SRGBMaps = {"albedo", "albedo_opacity"};
        public static readonly string[] NormalMaps = {"normal", "micronormal"};

        public static void BuildDidimoParts(DidimoComponents didimoComponents,
            DidimoImporterJsonConfig importerJsonConfig)
        {
            string headJoint = HEAD_JOINT_NAME;
            string headMesh = null;
            string eyeLashesMesh = null;
            string mouthMesh = null;
            string leftEyeMesh = null;
            string rightEyeMesh = null;
            string hairMesh = null;
            string bodyMesh = null;
            string clothingMesh = null;
            foreach (MeshProperties m in importerJsonConfig.meshList)
            {
                BodyPart bodyPart = BodyPart.Unknown;
                BodyPartMap.TryGetValue(m.bodyPart, out bodyPart);
                switch (bodyPart)
                {
                    case BodyPart.HeadMesh:
                        headMesh = m.meshName;
                        break;
                    case BodyPart.EyeLashesMesh:
                        eyeLashesMesh = m.meshName;
                        break;
                    case BodyPart.MouthMesh:
                        mouthMesh = m.meshName;
                        break;
                    case BodyPart.LeftEyeMesh:
                        leftEyeMesh = m.meshName;
                        break;
                    case BodyPart.RightEyeMesh:
                        rightEyeMesh = m.meshName;
                        break;
                    case BodyPart.HairMesh:
                        hairMesh = m.meshName;
                        break;
                    case BodyPart.BodyMesh:
                        bodyMesh = m.meshName;
                        break;
                    case BodyPart.ClothingMesh:
                        clothingMesh = m.meshName;
                        break;
                    default:
                        break;
                }
            }

            didimoComponents.Parts.SetupForPartNames(headJoint, headMesh, eyeLashesMesh, mouthMesh, leftEyeMesh,
                rightEyeMesh, hairMesh, bodyMesh, clothingMesh);
        }
    }
}