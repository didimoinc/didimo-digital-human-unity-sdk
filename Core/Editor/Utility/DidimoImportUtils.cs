using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Didimo
{
    class DidimoImportUtils : AssetPostprocessor
    {
        public static  bool         ShouldReimport = false;
        private static bool         reimportedDidimos;
        private static List<string> didimosToReimport;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (ShouldReimport && !reimportedDidimos)
            {
                List<string> didimoPaths = importedAssets.Where(asset => asset.StartsWith("Assets/Didimo", StringComparison.InvariantCultureIgnoreCase) &&
                                                                         asset.EndsWith(".gltf", StringComparison.InvariantCultureIgnoreCase))
                                                         .ToList();

                didimosToReimport = new List<string>();
                EditorApplication.update += DelayedReimportDidimos;
                foreach (string didimoPath in didimoPaths)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(didimoPath);
                    if (go == null)
                    {
                        didimosToReimport.Add(didimoPath);
                    }
                }
                reimportedDidimos = true;
            }
        }

        private static void DelayedReimportDidimos()
        {
            // If we failed to import didimos due to null resources, reimport them
            // This happens when we first import the SDK, because we don't have DidimoResources available when we're trying to import the gltf didimos
            // We need DidimoResources to be able to create materials
            if (GLTFImporter.CanImportDidimos())
            {
                foreach (string didimoPath in didimosToReimport)
                {
                    AssetDatabase.ImportAsset(didimoPath);
                }
            }

            didimosToReimport = null;

            EditorApplication.update -= DelayedReimportDidimos;
        }
    }
}