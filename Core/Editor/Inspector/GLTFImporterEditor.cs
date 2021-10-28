using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Didimo.Editor.Inspector
{
    [CustomEditor(typeof(GLTFImporter))]
    public class GLTFImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Extract Materials"))
            {
                GLTFImporter importer = serializedObject.targetObject as GLTFImporter;
                ExtractMaterials(importer!.assetPath, Path.Combine(Path.GetDirectoryName(importer.assetPath)!, "Materials"), importer);
            }
            
            if (GUILayout.Button("Use glTF Materials"))
            {
                GLTFImporter importer = serializedObject.targetObject as GLTFImporter;
                foreach (var map in importer.GetExternalObjectMap())
                {
                    importer.RemoveRemap(new AssetImporter.SourceAssetIdentifier(map.Value));
                }
            }

            ApplyRevertGUI();
        }

        void ExtractMaterials(string assetPath, string destinationPath, GLTFImporter importer)
        {
            IEnumerable<Object> enumerable = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(x => x.GetType() == typeof(Material));
            Directory.CreateDirectory(destinationPath);
            foreach (Object item in enumerable)
            {
                string path = Path.Combine(destinationPath, item.name) + ".mat";
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                string error = AssetDatabase.ExtractAsset(item, path);
                if (string.IsNullOrEmpty(error))
                {
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                else
                {
                    Debug.LogWarning($"Failed to extract asset {item.name}, error: {error}");
                }
            }
        }
    }
}