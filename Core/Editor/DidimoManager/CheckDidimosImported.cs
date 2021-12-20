using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool CheckOk()
        {
            if (didimosToReimport == null)
            {
                List<string> didimoPaths = AssetDatabase.GetAllAssetPaths()
                                                        .Where(asset => asset.StartsWith("Assets/Didimo", StringComparison.InvariantCultureIgnoreCase) &&
                                                                        asset.EndsWith(".gltf", StringComparison.InvariantCultureIgnoreCase))
                                                        .ToList();

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