using System;
using System.Collections.Generic;
using System.Linq;
using Didimo.Builder;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Editor
{
    public class CheckDidimosImported : IProjectSettingIssue
    {
        private const string REASON = "Some didimos are not yet imported.\n" +
                                      "This can happen the first time the Didimo SDK is imported, if the URP package wasn't already installed.\n" +
                                      "Please click the \"Reimport didimos\" button.";

        private List<string> didimosToReimport = null;
        
        public List<string> GetDidimosThatFailedImport()
        {
            if (didimosToReimport == null)
            {
                List<string> didimoPaths = AssetDatabase.GetAllAssetPaths()
                                                        .Where(asset => asset.StartsWith("Packages/com.didimo", StringComparison.InvariantCultureIgnoreCase) &&
                                                                        asset.EndsWith(".gltf", StringComparison.InvariantCultureIgnoreCase))
                                                        .ToList();
                List<string> fbxDidimoPaths = AssetDatabase.GetAllAssetPaths()
                                                        .Where(asset => asset.StartsWith("Packages/com.didimo", StringComparison.InvariantCultureIgnoreCase) &&
                                                                        asset.EndsWith(".fbx", StringComparison.InvariantCultureIgnoreCase))
                                                        .ToList();
                fbxDidimoPaths.RemoveAll(s => !DidimoImporterJsonConfigUtils.CheckIfJsonExists(s));
                didimoPaths.AddRange(fbxDidimoPaths);
                didimosToReimport = new List<string>();
                foreach (string didimoPath in didimoPaths)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(didimoPath);
                    if (go == null)
                    {
                        didimosToReimport.Add(didimoPath);
                    }
                }
            }
            
            return didimosToReimport;
        }

        public bool CheckOk()
        {
            GetDidimosThatFailedImport();

            if (didimosToReimport.Count != 0)
            {
                EditorGUILayout.HelpBox(ResolveText(), MessageType.Error);
            }

            return didimosToReimport.Count == 0;
        }

        public void Resolve()
        {
            if (didimosToReimport == null) return;

            foreach (string path in didimosToReimport)
            {
                AssetDatabase.ImportAsset(path);
            }

            didimosToReimport.Clear();
        }

        public string ResolveText() => REASON;
    }
}