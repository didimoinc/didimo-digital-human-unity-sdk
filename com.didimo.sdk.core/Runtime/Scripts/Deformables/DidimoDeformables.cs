using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Didimo.Extensions;
using Didimo.Core.Utility;
using UnityEditor;
using static Didimo.Core.Config.ShaderResources;
using System.IO;

namespace Didimo.Core.Deformables
{
    public class DidimoDeformables : DidimoBehaviour
    {
        // We will only need this temporarily, as hair pieces will be skinned in the future
        [SerializeField, HideInInspector]
        private Matrix4x4 hairOffset;

        public void CacheHairOffsets()
        {
            if (transform.TryFindRecursive("Head", out Transform head))
            {
                hairOffset = head.worldToLocalMatrix;
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
                if (_deformables == null)
                {
                    _deformables = new Dictionary<string, Deformable>();
                    var deformableList = gameObject.GetComponentsInChildren<Deformable>();
                    foreach (var deformable in deformableList)
                    {
                        if (!_deformables.ContainsKey(deformable.ID))
                            _deformables.Add(deformable.ID, deformable);
                    }
                }

                return _deformables;
            }
        }

        public void OnAfterAssemblyReload() { Debug.Log("After Assembly Reload"); }

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

        public bool TryCreate<TDeformable>(TDeformable deformable, out TDeformable instance, DeformMode deformMode = DeformMode.Nothing, bool forceRemove = false)
            where TDeformable : Deformable
        {
            if (deformable == null)
            {
                Debug.LogWarning("Cannot instantiate deformable from a null template");
                instance = null;
                return false;
            }

            if ((deformable.SingleInstancePerDidimo || forceRemove) && Exists<TDeformable>())
            {
                DestroyAll<TDeformable>();
            }
            else
            {
                if (Exists<TDeformable>())
                    Debug.Log("Not destroying deformable even though one was found");
                else
                    Debug.Log("Not destroying deformable");
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
                Debug.LogWarning($"Cannot find ideal deformable bone with any of " + $"the names: '{string.Join(",", instance.IdealBoneNames)}'");
            }

            Transform instanceTransform = instance.transform;

            if (deformMode != DeformMode.Nothing)
            {
#if UNITY_EDITOR
                Mesh deformableMesh = MeshUtils.GetMesh(deformable.gameObject);
                Renderer bodyMeshRenderer = MeshUtils.GetMeshRendererFromBodyPart(DidimoComponents.gameObject, EBodyPartID.BODY);
                if (bodyMeshRenderer != null)
                {
                    Mesh bodyMesh = MeshUtils.GetMesh(bodyMeshRenderer.gameObject);

                    string bodyMeshLocation = AssetDatabase.GetAssetPath(bodyMesh);
                    string deformableMeshLocation = AssetDatabase.GetAssetPath(deformableMesh);

                    DirectoryInfo bodyMeshDirectory = new DirectoryInfo(Path.GetDirectoryName(bodyMeshLocation));
                    string deformableMeshName = Path.GetFileNameWithoutExtension(deformableMeshLocation);

                    string bodySpecificHairDirectory =
                        Path.Combine(FeedDirectoryToUnity(bodyMeshDirectory.Parent.FullName), "Hairs", bodyMeshDirectory.Name.Replace("gltf", "hairs"));
                    string[] deformableAssets =
                        AssetDatabase.FindAssets(deformableMeshName, new string[] {FeedDirectoryToUnity(bodyMeshDirectory.FullName), bodySpecificHairDirectory});
                    foreach (var asset in deformableAssets)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(asset);
                        if (path.EndsWith("obj"))
                        {
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
                            }
                        }
                    }
                }
#endif
            }
            
            instanceTransform.SetParent(idealBone ? idealBone : DidimoComponents.transform);
            instanceTransform.localPosition = hairOffset.MultiplyPoint(Vector3.zero);
            instanceTransform.localRotation = hairOffset.rotation;
            instance.name = deformable.ID;

            return true;
        }

        public bool TryCreate<TDeformable>(string deformableId, out TDeformable instance, DeformMode deformMode = DeformMode.Nothing, bool forceRemove = false)
            where TDeformable : Deformable
        {
            var deformableDatabase = Resources.Load<DeformableDatabase>("DeformableDatabase");

            if (TryFindDeformable(deformableDatabase.Deformables, deformableId, out TDeformable deformable) == false)
            {
                Debug.LogWarning($"No database deformable found with ID: {deformableId}");
                instance = null;
                return false;
            }

            return TryCreate(deformable, out instance, deformMode, forceRemove);
        }

        public void DestroyAll<TDeformable>() where TDeformable : Deformable
        {
            static void OnRemove(KeyValuePair<string, Deformable> kvp)
            {
                if (kvp.Value != null)
                {
#if UNITY_EDITOR
                    if (PrefabUtility.GetPrefabInstanceHandle(kvp.Value.gameObject) == null)
#endif
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(kvp.Value.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(kvp.Value.gameObject);
                        }
                    }
#if UNITY_EDITOR
                    else
                    {
                        kvp.Value.gameObject.SetActive(false);
                        Debug.LogWarning($"Attempting to destroy deformable on prefab ('{kvp.Value.gameObject}'), this is not allowed so it's only been temporarily hidden.");
                    }
#endif
                }
            }

            deformables.RemoveWhere(d => d.Value is TDeformable, OnRemove);
        }

        private bool Exists<TDeformable>() where TDeformable : Deformable { return TryFindAllDeformables<TDeformable>().Any(); }

        private IEnumerable<TDeformable> TryFindAllDeformables<TDeformable>() where TDeformable : Deformable
        {
            return deformables.Where(d => d.Value is TDeformable).Select(d => d.Value).Cast<TDeformable>();
        }

        struct DeformableMeshPair
        {
            Deformable deformable;
            Mesh       mesh;
        }

        public static bool TryFindDeformable<TDeformable>(Deformable[] deformableArray, string id, out TDeformable deformable) where TDeformable : Deformable
        {
            deformable = deformableArray.Where(d => d is TDeformable).Cast<TDeformable>().FirstOrDefault(h => h.ID == id);

            return deformable != null;
        }
    }
}