using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Didimo.Extensions;
using Didimo.Core.Utility;
using UnityEditor;
using System.IO;
using Didimo.Core.Deformer;

namespace Didimo.Core.Deformables
{
    public class DidimoDeformables : DidimoBehaviour
    {
        public TextAsset deformationFile;

        [NonSerialized]
        public string DeformationFilePath;

        // We will only need this temporarily, as hair pieces will be skinned in the future
        [SerializeField, HideInInspector]
        private Matrix4x4 hairOffset = Matrix4x4.identity;

        public static string FindDeformationFile(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory)) return null;
            // Find a file in the root folder that is a .dmx or .npz
            return Directory.EnumerateFiles(rootDirectory, "*.*",
                    new EnumerationOptions {IgnoreInaccessible = true, MatchCasing = MatchCasing.CaseInsensitive})
                .FirstOrDefault(s =>
                    s.EndsWith(".dmx", StringComparison.InvariantCultureIgnoreCase) ||
                    s.EndsWith(".npz", StringComparison.InvariantCultureIgnoreCase));
        }

        public static TextAsset GetDeformationFile(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory)) return null;

            string deformationFilePath = FindDeformationFile(rootDirectory);
            if (string.IsNullOrEmpty(deformationFilePath)) return null;

#if UNITY_EDITOR
            string projectPath = Path.GetDirectoryName(Application.dataPath)!.Replace('\\', '/');
            TextAsset deformationFile =
                AssetDatabase.LoadAssetAtPath<TextAsset>(deformationFilePath.Replace('\\', '/').Replace(projectPath + "/", ""));
            if (deformationFile != null) return deformationFile;
#endif
            if (File.Exists(deformationFilePath)) return new ByteAsset(File.ReadAllBytes(deformationFilePath));
            return null;
        }
        
        public void CacheHairOffsets()
        {
            if (transform.TryFindRecursive("Head", out Transform head))
            {
                hairOffset = head.worldToLocalMatrix * transform.localToWorldMatrix.inverse;
            }
            else
            {
                hairOffset = Matrix4x4.identity;
            }
        }

        private Dictionary<string, Deformable> _deformables = null;

        public enum DeformMode
        {
            Nothing                               = 0,
            ReplaceWithPreFittedMesh              = 1,
            ReplaceVertexChannelFromPreFittedMesh = 2,
            DeformMesh                            = 3
        }

        public Dictionary<string, Deformable> deformables
        {
            get
            {
                if (_deformables == null) UpdateDeformablesList();
                return _deformables;
            }
        }

        protected Deformer.Deformer deformer;

        public Deformer.Deformer Deformer
        {
            get
            {
                if (deformer == null) BuildDefomer();
                return deformer;
            }
        }

        /// <summary>
        /// Creates/Updates the associated deformer from the deformationFile or DeformationFilePath variables
        /// </summary>
        public void BuildDefomer()
        {
            
            if (deformationFile != null) deformer = DeformerFactory.BuildDeformer(deformationFile);
            else if (!string.IsNullOrEmpty(DeformationFilePath)) deformer = DeformerFactory.BuildDeformer(DeformationFilePath);
            else deformer = DeformerFactory.BuildDefaultDeformer();
        }

        /// <summary>
        /// Updates all the list of deformables that exist under this didimo root object
        /// </summary>
        public void UpdateDeformablesList()
        {
            _deformables = new Dictionary<string, Deformable>();
            Deformable[] deformableList = gameObject.GetComponentsInChildren<Deformable>();
            foreach (Deformable deformable in deformableList)
            {
                if (!_deformables.ContainsKey(deformable.ID))
                    _deformables.Add(deformable.ID, deformable);
            }
        }

        public bool TryFind<TDeformable>(string deformableId, out TDeformable instance) where TDeformable : Deformable
        {
            if (!deformables.TryGetValue(deformableId, out Deformable deformable))
            {
                instance = null;
                return false;
            }

            instance = deformable as TDeformable;
            return instance != null;
        }

        string FeedDirectoryToUnity(string fullPath)
        {
            var fullPathLow = fullPath.ToLower();
            int idx = fullPathLow.IndexOf("packages");
            if (idx != -1)
                return fullPath.Substring(idx);
            idx = fullPathLow.IndexOf("assets");
            if (idx != -1)
                return fullPath.Substring(idx);
            return fullPath;
        }
        
        public bool TryFind<TDeformable>(out TDeformable instance) where TDeformable : Deformable
        {
            if (!deformables.Any(d => d.Value is TDeformable))
            {
                instance = null;
                return false;
            }
            instance = deformables.FirstOrDefault(d => d.Value is TDeformable).Value as TDeformable;
            return instance != null;
        }

        public static string ExtractMeshID(string meshName)
        {
            var underScoreIdx = meshName.LastIndexOf('_');
            if (underScoreIdx != -1)
            {
                var nameBefore = meshName.Substring(0, underScoreIdx);
                var mindex = nameBefore.LastIndexOf('M');
                if (mindex != -1)
                    return nameBefore.Substring(mindex + 1);
            }

            return meshName;
        }

        public enum TryCreateFlags
        {
            ForceRemove = 0x1,
            ForceCreateMaterials = 0x2,
            CreatePrefabs= 0x4
        }
        
        public bool TryCreate<TDeformable>(TDeformable deformable, out TDeformable instance, DeformMode deformMode = DeformMode.Nothing, int flags = 0)
            where TDeformable : Deformable
        {
            if (deformable == null)
            {
                Debug.LogWarning("Cannot instantiate deformable from a null template");
                instance = null;
                return false;
            }

            if ((deformable.SingleInstancePerDidimo || ((flags & (int)TryCreateFlags.ForceRemove) != 0) && Exists<TDeformable>()))
            {
                DestroyAll<TDeformable>();
            }

            deformable.DidimoComponents = DidimoComponents;
            instance = Instantiate(deformable);
            deformables.Add(deformable.ID, instance);

            Transform idealBone = null;
            foreach (string idealBoneName in instance.IdealBoneNames)
            {
                if (transform.TryFindRecursive(idealBoneName, out idealBone))
                {
                    break;
                }
            }
            if (idealBone == null)
            {
                transform.TryFindRecursive("Head", out idealBone);                
            }

            if (idealBone == null)
            {
                transform.TryFindRecursive("Head", out idealBone);
            }

            if (idealBone == null)
            {
                Debug.LogWarning($"Cannot find ideal deformable bone with any of the names: '{string.Join(",", instance.IdealBoneNames)}'");
            }

            Transform instanceTransform = instance.transform;
            
            
            Mesh deformableMesh = MeshUtils.GetMesh(deformable.gameObject);
            if (deformMode == DeformMode.DeformMesh)
            {
                Mesh deformedMesh = DeformMesh(deformableMesh, deformable.ID);
                MeshUtils.SetMesh(instance.gameObject, deformedMesh);
            }

            #if UNITY_EDITOR
            // PerformEditorBasedOperations(instanceTransform, deformableMesh, flags, deformMode);
            #endif

            instanceTransform.SetParent(idealBone ? idealBone : DidimoComponents.transform);
            if (hairOffset == Matrix4x4.zero)
            {
                CacheHairOffsets();
            }
            instanceTransform.localPosition = hairOffset.GetPosition();
            instanceTransform.localRotation = hairOffset.rotation;
            instance.name = deformable.ID;

            return true;
        }

        public bool TryCreate<TDeformable>(string deformableId, out TDeformable instance, DeformMode deformMode = DeformMode.Nothing, int flags = 0)
            where TDeformable : Deformable
        {
            if (!TryFindDeformable(DeformableUtils.GetAllDeformables().ToArray(), deformableId, out TDeformable deformable))
            {
                Debug.LogWarning($"No database deformable found with ID: {deformableId}");
                instance = null;
                return false;
            }

            return TryCreate(deformable, out instance, deformMode, flags);
        }
      
        void PerformEditorBasedOperations(Transform instanceTransform, Mesh deformableMesh, int flags, DeformMode deformMode)
        {
        #if UNITY_EDITOR
            string deformableMeshLocation = AssetDatabase.GetAssetPath(deformableMesh);
            string deformableMeshName = Path.GetFileNameWithoutExtension(deformableMeshLocation);
            if (deformMode != DeformMode.Nothing)
            {
                Renderer bodyMeshRenderer = DidimoComponents.Parts.BodyMeshRenderer;
                Renderer headMeshRenderer = DidimoComponents.Parts.HeadMeshRenderer;
                if (bodyMeshRenderer != null)
                {
                    Mesh bodyMesh = MeshUtils.GetMesh(bodyMeshRenderer.gameObject);
                    Mesh headMesh = MeshUtils.GetMesh(headMeshRenderer.gameObject);
                    string bodyMeshLocation = AssetDatabase.GetAssetPath(bodyMesh);
                    var bodyMeshID = ExtractMeshID(bodyMeshLocation);

                    //find the source head asset
                    Mesh sourceHeadMesh = null;

                    var baseMeshPath = "packages/com.didimo.sdk.internal/Assets/Content/Didimos/GLTFTemplate/avatar.gltf";
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(baseMeshPath);
                    if (go)
                    {
                        GameObject headgo = ComponentUtility.GetChildWithName(go, "mesh_m_low_baseFace_001", true);
                        if (headgo)
                            sourceHeadMesh = MeshUtils.GetMesh(headgo);
                    }
                    string sourceDidimoFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceHeadMesh));

                    DirectoryInfo bodyMeshDirectory = new DirectoryInfo(Path.GetDirectoryName(bodyMeshLocation));
                  
                    string bodySpecificHairDirectory =
                        Path.Combine(FeedDirectoryToUnity(bodyMeshDirectory.Parent.FullName), "Hairs", bodyMeshDirectory.Name.Replace("gltf", "hairs"));
                    string[] deformableAssets =
                        AssetDatabase.FindAssets(deformableMeshName,
                        new string[] { FeedDirectoryToUnity(bodyMeshDirectory.FullName),
                                       bodySpecificHairDirectory,
                                       "Assets/PreDeformables/Hair",
                                       "Packages/com.didimo.sdk.core/Runtime/Content/Deformables/Hair/",
                                       "Packages/com.didimo.sdk.experimental/Runtime/Content/Deformables/Hair",
                                       "Packages/com.didimo.sdk.experimental/Runtime/Content/Deformables/HairHD"});
                    foreach (var asset in deformableAssets)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(asset);
                        if (path.EndsWith("obj"))
                        {
                            var lowPath = path.ToLower();
                            int templateIdx = lowPath.IndexOf("template to");
                            if (templateIdx != -1)
                            {
                                var remainingString = lowPath.Substring(templateIdx);
                                if (remainingString.IndexOf(bodyMeshID) == -1)
                                    continue;
                            }
                            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                            if (mesh)
                            {
                                if (deformMode == DeformMode.ReplaceVertexChannelFromPreFittedMesh)
                                {
                                    Mesh vswapmesh = MeshUtils.TopologyFromAVerticesFromB(deformableMesh, mesh);
                                    MeshUtils.SetMesh(instanceTransform.gameObject, vswapmesh);
                                }
                                else if (deformMode == DeformMode.ReplaceWithPreFittedMesh)
                                {
                                    MeshUtils.SetMesh(instanceTransform.gameObject, mesh);                                    
                                }
                                else if (deformMode == DeformMode.DeformMesh)
                                {
                                    // Matrix4x4 fudgeScale = Matrix4x4.identity;
                                    // Mesh vswapmesh = DeformMesh(deformableMeshName, deformableMesh, headMesh, sourceHeadMesh, fudgeScale);
                                    // MeshUtils.SetMesh(instanceTransform.gameObject, vswapmesh);
                                }
                                
                                break;  
                            }
                        }
                    }
                }                
            }           
            if ((flags & (int)TryCreateFlags.ForceCreateMaterials) != 0)
                MaterialUtility.GenerateHairMaterialsForSelected(new GameObject[] { instanceTransform.gameObject }, true, true);
            if ((flags & (int)TryCreateFlags.CreatePrefabs) != 0)
            {
                string pipelineSuffix = ResourcesLoader.PipelineName[(int)ResourcesLoader.GetAppropriateID()];
                string deformableMeshPathAndName = Path.ChangeExtension(deformableMeshLocation, "").TrimEnd('.');
                string prefabPath = deformableMeshPathAndName + "_" + pipelineSuffix + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(instanceTransform.gameObject, prefabPath);
            }
#endif
        }

        public Mesh DeformMesh(Mesh sourceDeformable, string deformedMeshName=null)
        {
            Vector3[] newVertices = Deformer.DeformVertices(sourceDeformable.vertices);
            Mesh deformedMesh = MeshUtils.ApplyMeshVertexDeformation(sourceDeformable, newVertices);
            deformedMesh.name = string.IsNullOrEmpty(deformedMeshName) ? sourceDeformable.name : deformedMeshName;
            return deformedMesh;
        }


        private static void RemoveDeformableFromScene(Deformable deformable)
        {
#if UNITY_EDITOR
            if (PrefabUtility.GetPrefabAssetType(deformable.gameObject) == PrefabAssetType.NotAPrefab)
#endif
            {
                if (Application.isPlaying)
                {
                    Destroy(deformable.gameObject);
                }
                else
                {
                    DestroyImmediate(deformable.gameObject);
                }
            }
#if UNITY_EDITOR
            else
            {
                deformable.gameObject.SetActive(false);
                Debug.LogWarning($"Attempting to destroy deformable on prefab ('{deformable.gameObject}'), this is not allowed so it's only been temporarily hidden.");
            }
#endif
        }

        public void DestroyDeformable(Deformable deformable)
        {
            deformables.Remove(deformable.ID);
            RemoveDeformableFromScene(deformable);
        }

        public void DestroyDeformable(string deformableId)
        {
            deformables.Remove(deformableId, out Deformable deformable);
            RemoveDeformableFromScene(deformable);
        }


        public void DestroyAll<TDeformable>() where TDeformable : Deformable
        {
            deformables.RemoveWhere(d => d.Value is TDeformable, (deformableKeyValue) => RemoveDeformableFromScene(deformableKeyValue.Value));
        }

        private bool Exists<TDeformable>() where TDeformable : Deformable { return TryFindAllDeformables<TDeformable>().Any(); }

        private IEnumerable<TDeformable> TryFindAllDeformables<TDeformable>() where TDeformable : Deformable
        {
            return deformables.Where(d => d.Value is TDeformable).Select(d => d.Value).Cast<TDeformable>();
        }

        public static bool TryFindDeformable<TDeformable>(Deformable[] deformableArray, string id, out TDeformable deformable) where TDeformable : Deformable
        {
            deformable = deformableArray.Where(d => d is TDeformable).Cast<TDeformable>().FirstOrDefault(h => h.ID == id);
            return deformable != null;
        }
    }
}