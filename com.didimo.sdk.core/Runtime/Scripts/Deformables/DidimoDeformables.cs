using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Didimo.Extensions;
using Didimo.Core.Utility;
using UnityEditor;
using static Didimo.Core.Config.ShaderResources;
using System.IO;
using Didimo.Core.Config;



namespace Didimo.Core.Deformables
{
    public class DidimoDeformables : DidimoBehaviour
    {
        // We will only need this temporarily, as hair pieces will be skinned in the future
        [SerializeField, HideInInspector]
        private Matrix4x4 hairOffset = Matrix4x4.identity;

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
                transform.TryFindRecursive("Head", out idealBone);                
            }

            if (idealBone == null)
            {
                transform.TryFindRecursive("Head", out idealBone);
            }

            if (idealBone == null)
            {
                Debug.LogWarning($"Cannot find ideal deformable bone with any of " + $"the names: '{string.Join(",", instance.IdealBoneNames)}'");
            }

            Transform instanceTransform = instance.transform;
            
            
            Mesh deformableMesh = MeshUtils.GetMesh(deformable.gameObject);
            #if UNITY_EDITOR
            PerformEditorBasedOperations(instanceTransform, deformableMesh, flags, deformMode);
            #endif

            instanceTransform.SetParent(idealBone ? idealBone : DidimoComponents.transform);
            if (hairOffset == Matrix4x4.zero)
            {
                CacheHairOffsets();
            }
            instanceTransform.localPosition = hairOffset.MultiplyPoint(Vector3.zero);
            instanceTransform.localRotation = hairOffset.rotation;
            instance.name = deformable.ID;

            return true;
        }

        public bool TryCreate<TDeformable>(string deformableId, out TDeformable instance, DeformMode deformMode = DeformMode.Nothing, int flags = 0)
            where TDeformable : Deformable
        {
            if (TryFindDeformable(DeformableUtils.GetAllDeformables().ToArray(), deformableId, out TDeformable deformable) == false)
            {
                var experimentalDeformableDatabase = Resources.Load<DeformableDatabase>("ExperimentalDeformableDatabase");                
                if (TryFindDeformable(experimentalDeformableDatabase.Deformables, deformableId, out TDeformable tdeformable) == false)
                {
                    Debug.LogWarning($"No database deformable found with ID: {deformableId}");
                    instance = null;
                    return false;
                }
                else
                    deformable = tdeformable;
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
                Renderer bodyMeshRenderer = MeshUtils.GetMeshRendererFromBodyPart(DidimoComponents.gameObject, EBodyPartID.BODY);
                Renderer headMeshRenderer = MeshUtils.GetMeshRendererFromBodyPart(DidimoComponents.gameObject, EBodyPartID.HEAD);
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
                                    Matrix4x4 fudgeScale = Matrix4x4.identity;
                                  
                                    Mesh vswapmesh = DeformMesh(deformableMeshName, deformableMesh, headMesh, sourceHeadMesh, fudgeScale);
                                    MeshUtils.SetMesh(instanceTransform.gameObject, vswapmesh);
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
                bool success = false;
                string PipelineSuffix = ResourcesLoader.PipelineName[(int)ResourcesLoader.GetAppropriateID()];
                var deformableMeshPathAndName = Path.ChangeExtension(deformableMeshLocation, "").TrimEnd('.');
                string prefabPath = deformableMeshPathAndName + "_" + PipelineSuffix + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(instanceTransform.gameObject, prefabPath, out success);                
            }
#endif
        }

        public static Mesh DeformMesh(string deformableName, Mesh sourceDeformable, Mesh targetDidimo, Mesh sourceDidimo, Matrix4x4 transform)
        {

#if USING_CLIENT_SIDE_DEFORMER
            string sourceDidimoFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceDidimo));
            string targetDidimoFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(targetDidimo));
            string savePath = targetDidimoFolder + Path.DirectorySeparatorChar + deformableName + ".obj";

            if (!string.IsNullOrEmpty(savePath))
            {
                var appPath = Path.GetDirectoryName(Application.dataPath);

                List<Vector3> source = sourceDidimo.vertices.ToList();
                List<Vector3> target = targetDidimo.vertices.ToList();

                var sourceAssetPath = AssetDatabase.GetAssetPath(sourceDeformable);
                ModelImporter sourceDeformableImporter = AssetImporter.GetAtPath(sourceAssetPath) as ModelImporter;

                Debug.Log("Derforming '" + sourceAssetPath+ "' to '" + savePath + "'" );

                OBJ obj = new OBJ();
                obj.DeserializeFromFile(sourceAssetPath);
                obj.ApplyScaleToVertices(sourceDeformableImporter.globalScale);

                List<Vector3> verticesToDeform = obj.vertices;

                CGTPS cgtps = new CGTPS(source, target);

                List<Vector3> deformedVertices = cgtps.TransformVertices(verticesToDeform);      
                for (var i = 0; i < deformedVertices.Count; ++i)
                    deformedVertices[i] = transform.MultiplyPoint(deformedVertices[i]);
                obj.vertices = deformedVertices;
                obj.ApplyScaleToVertices(1f / sourceDeformableImporter.globalScale);

                obj.SerializeToFile(savePath);

                AssetDatabase.Refresh();


                foreach (var mtllib in obj.mtlLibs)
                {
                    var destPath = appPath + Path.DirectorySeparatorChar + targetDidimoFolder + Path.DirectorySeparatorChar + deformableName + ".mtl";
                    //if (!System.IO.File.Exists(destPath))
                    {
                        var defoType = (deformableName.ToLower().IndexOf("hat") != -1)? "Hats":"Hair";
                        var path = appPath + "/Packages/com.didimo.sdk.core/Runtime/Content/Deformables/" + defoType+ "/" + deformableName + "/" + deformableName + ".mtl";
                        //FileUtil.CopyFileOrDirectory(path.Replace("/", "/"), destPath.Replace("/", "/"));                        
                        if (!System.IO.File.Exists(path))
                        {
                            Debug.LogWarning("Problem copying '" + path + "' to '" + destPath + "', trying variant...");
                            path = appPath + "/Packages/com.didimo.sdk.core/Runtime/Content/Deformables/" + defoType + "/" + deformableName.Replace("0","00") + "/" + deformableName + ".mtl";
                        }
                        try
                        {
                            File.Copy(path, destPath, true);
                            Debug.LogWarning("Copied '" + path + "' to '" + destPath + "', trying variant...");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning("Problem copying '" + path + "' to '" + destPath +"' error was: " + ex.Message) ;
                        }
                        
                    }
                }

                savePath = savePath.Replace("/", "/");
                
                if (savePath.StartsWith(appPath))
                    savePath = savePath.Remove(0, appPath.Length + 1);

                ModelImporter deformedImporter = AssetImporter.GetAtPath(savePath) as ModelImporter;
                if (deformedImporter)
                {
                    deformedImporter.useFileScale = sourceDeformableImporter.useFileScale;
                    deformedImporter.globalScale = sourceDeformableImporter.globalScale;
                }
                deformedImporter.SaveAndReimport();
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(savePath);
                return mesh;
            }
#endif
            return null;
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