using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Core.Config;
using Didimo.Core.Deformables;
using Didimo.Core.Utility;
using Didimo.GLTFUtility;
using Didimo.Speech;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using BodyPart = Didimo.Core.Utility.DidimoParts.BodyPart;
using Object = UnityEngine.Object;

namespace Didimo.Builder
{
    public class DidimoImporterJsonConfigUtils
    {
        const string JSON_NAME = "avatar_info.json";

        public static DidimoImporterJsonConfig GetConfigAtFolder(string folder)
        {
            DidimoImporterJsonConfig jsonConfig = null;
            if (CheckIfJsonExists(folder))
            {
                jsonConfig = JsonConvert.DeserializeObject<DidimoImporterJsonConfig>(
                    File.ReadAllText(Path.Combine(folder, JSON_NAME)));
            }

            return jsonConfig;
        }

        public static bool CheckIfJsonExists(string folder)
        {
            return File.Exists(Path.Combine(folder, JSON_NAME));
        }


#if UNITY_EDITOR
        public static void SetupDidimoForEditor(GameObject didimoGameObject,
            DidimoImporterJsonConfig importerJsonConfig,
            string assetPath, AnimationClip[] animationClips,
            AnimationClip resetAnimationClip, Action<Object> onObjectCreated)
        {
            IEnumerator en = SetupDidimoImpl(didimoGameObject, importerJsonConfig, assetPath, animationClips,
                resetAnimationClip, true, onObjectCreated);
            while (en.MoveNext())
            {
            }
        }
#endif

        public static IEnumerator SetupDidimoForRuntime(GameObject didimoGameObject,
            DidimoImporterJsonConfig importerJsonConfig,
            string assetPath, AnimationClip[] animationClips, AnimationClip resetAnimationClip,
            Action<Object> onObjectCreated)
        {
            if (!Application.isPlaying)
            {
                throw new Exception("SetupDidimoForRuntime can only be called during play mode.");
            }

            yield return SetupDidimoImpl(didimoGameObject, importerJsonConfig, assetPath, animationClips,
                resetAnimationClip, false, onObjectCreated);
        }

        private static IEnumerator SetupDidimoImpl(GameObject didimoGameObject,
            DidimoImporterJsonConfig importerJsonConfig,
            string assetPath, AnimationClip[] animationClips,
            AnimationClip resetAnimationClip, bool isAssetImporter, Action<Object> onObjectCreated)
        {
            List<string> texturePaths = new();
            foreach (var mesh in importerJsonConfig.meshList)
            {
                foreach (KeyValuePair<string, string> texture in mesh.textures)
                {
                    texturePaths.Add(texture.Value);
                }
            }

            // remove duplicates
            texturePaths = texturePaths.Distinct().ToList();

            List<GLTFImage> images = new();
            List<GLTFTexture> textures = new();

            for (int i = 0; i < texturePaths.Count; i++)
            {
                GLTFImage image = new GLTFImage();
                image.uri = texturePaths[i];
                image.name = Path.GetFileNameWithoutExtension(texturePaths[i]);
                images.Add(image);

                GLTFTexture texture = new();
                texture.name = image.name;
                texture.source = i;
                textures.Add(texture);
            }

            List<Importer.ImportTask> importTasks = new List<Importer.ImportTask>();

            // Import textures 
            GLTFImage.ImportTask imageTask =
                new GLTFImage.ImportTask(images, Path.GetFullPath(Path.GetDirectoryName(assetPath)!), null);
            GLTFTexture.ImportTask textureTask = new GLTFTexture.ImportTask(textures, imageTask);

            imageTask.RunSynchronously();
            textureTask.RunSynchronously();

            ShaderResources shaderResources = ResourcesLoader.ShaderResources();

            if (shaderResources == null)
            {
                Debug.LogError("Shader resources was null.");
                yield break;
            }

            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);

            DidimoComponents didimoComponents = ComponentUtility.GetOrAdd<DidimoComponents>(didimoGameObject);
            didimoGameObject.AddComponent<DidimoParts>();

            BuildDidimoParts(didimoComponents, importerJsonConfig);

            DidimoDeformables deformables = didimoGameObject.AddComponent<DidimoDeformables>();
            deformables.CacheHairOffsets();
            deformables.deformationFile = DidimoDeformables.GetDeformationFile(Path.GetDirectoryName(assetPath) + "/");

            foreach (MeshProperties m in importerJsonConfig.meshList)
            {
                BodyPart bodyPart = BodyPart.Unknown;
                BodyPartMap.TryGetValue(m.bodyPart, out bodyPart);

                if (bodyPart == BodyPart.HairMesh)
                {
                    HandleHair(didimoComponents, m, onObjectCreated);
                    continue;
                }

                Renderer bodyPartRenderer = didimoComponents.Parts.BodyPartToRenderer(bodyPart);

                materialBuilder.FindIdealShaderForBodyPart(bodyPart, out Shader shader);
                Material material = new Material(shader);
                material.name = m.meshName + "_MAT";
                onObjectCreated?.Invoke(material);
                if (m.textures != null)
                {
                    foreach (var t in m.textures)
                    {
                        int textureIndex = texturePaths.IndexOf(texturePaths.FirstOrDefault(texturePath =>
                            texturePath == t.Value));

                        if (BodyPartIds.TryGetValue(bodyPart,
                                out Dictionary<string, string> ids))
                        {
                            if (ids.TryGetValue(t.Key, out string mapping))
                            {
                                bool normalMap = NormalMaps.Contains(t.Key);
                                bool sRGB = SRGBMaps.Contains(t.Key);

                                IEnumerator en = textureTask.Result[textureIndex].GetTextureCached(!sRGB, normalMap,
                                    texture2D => { material.SetTexture(mapping, texture2D); });
                                while (en.MoveNext())
                                {
                                }
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
        }

        private static void HandleHair(DidimoComponents didimoComponents, MeshProperties hairProperties,
            Action<Object> onObjectCreated)
        {
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
            {"albedo_opacity", "_MainTex"}
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

        public static readonly string[] SRGBMaps = {"albedo"};
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