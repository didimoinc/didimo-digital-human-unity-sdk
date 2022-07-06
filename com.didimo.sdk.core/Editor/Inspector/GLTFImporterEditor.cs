using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Didimo.GLTFUtility;
using GLTFImporter = Didimo.Core.Editor.GLTFImporter;
using Object = UnityEngine.Object;

namespace Didimo.Editor.Inspector
{
    [CustomEditor(typeof(GLTFImporter))]
    public class GLTFImporterEditor : ScriptedImporterEditor
    {
        private        int      selectedTabIndex;
        private        Vector2  scrollPosition;
        private static string[] tabNames;

        private static string[] GetTabNames() { return tabNames ??= Enum.GetNames(typeof(Tab)); }

        private enum Tab
        {
            // Default = 0,
            Model,
            Rig,
            Animation,
            Materials
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GLTFImporter importer = serializedObject.targetObject as GLTFImporter;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, GetTabNames());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);

            switch (selectedTabIndex)
            {
                // case (int) Tab.Default:
                //     DrawDefaultInspector();
                //     break;

                case (int) Tab.Model:

                    DrawModelTabUI(importer);
                    break;

                case (int) Tab.Rig:
                    DrawRigTabUI(importer);
                    break;

                case (int) Tab.Animation:

                    DrawAnimationTabUI(importer);
                    break;

                case (int) Tab.Materials:
                    DrawMaterialsTabUI(importer);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(importer);
            ApplyRevertGUI();
        }

        private void DrawModelTabUI(GLTFImporter importer)
        {
            SerializedProperty importSettings = serializedObject.FindProperty("importSettings");
            string[] exposedProperties = {"normals", "tangents", "generateLightmapUVs"};

            foreach (string exposedProperty in exposedProperties)
            {
                SerializedProperty property = importSettings.FindPropertyRelative(exposedProperty);
                EditorGUILayout.PropertyField(property);
            }
        }

        private void DrawRigTabUI(GLTFImporter importer)
        {
            SerializedProperty importSettings = serializedObject.FindProperty("importSettings");
            SerializedProperty animationType = importSettings.FindPropertyRelative("animationType");
            EditorGUILayout.PropertyField(animationType);

            switch (importer.importSettings.animationType)
            {
                case ImportSettings.AnimationType.Generic:
                case ImportSettings.AnimationType.Humanoid:

                    SerializedProperty avatarDefinition = importSettings.FindPropertyRelative("avatarDefinition");
                    EditorGUILayout.PropertyField(avatarDefinition);

                    if (importer.importSettings.avatarDefinition == ImportSettings.AvatarDefinition.CopyFromAnotherAvatar)
                    {
                        SerializedProperty avatar = importSettings.FindPropertyRelative("avatar");
                        EditorGUILayout.PropertyField(avatar);

                        if (AssetDatabase.GetAssetPath(importer.importSettings.avatar) == importer.assetPath)
                        {
                            importer.importSettings.avatar = null;
                        }
                    }

                    break;
            }
        }

        private void DrawAnimationTabUI(GLTFImporter importer)
        {
            SerializedProperty importSettings = serializedObject.FindProperty("importSettings");
            string[] exposedProperties = {"animationSettings", "interpolationMode"};

            foreach (string exposedProperty in exposedProperties)
            {
                SerializedProperty property = importSettings.FindPropertyRelative(exposedProperty);
                EditorGUILayout.PropertyField(property);
            }
        }

        private void DrawMaterialsTabUI(GLTFImporter importer)
        {
            importer.importSettings.materials = GUILayout.Toggle(importer.importSettings.materials, "Import materials");

            SerializedProperty importSettings = serializedObject.FindProperty("importSettings");
            if (!importer.importSettings.isDidimo)
            {
                SerializedProperty shaderSettings = importSettings.FindPropertyRelative("shaderOverrides");
                EditorGUILayout.PropertyField(shaderSettings);
            }

            if (GUILayout.Button("Use glTF Materials"))
            {
                foreach (var map in importer.GetExternalObjectMap())
                {
                    importer.RemoveRemap(new AssetImporter.SourceAssetIdentifier(map.Value));
                }
            }

            if (GUILayout.Button("Extract Materials"))
            {
                ExtractMaterials(importer);
            }
        }

        private static void ExtractMaterials(GLTFImporter importer)
        {
            string exportPath = EditorUtility.SaveFolderPanel("Select Materials Folder", Path.GetDirectoryName(importer!.assetPath), "");
            if (string.IsNullOrEmpty(exportPath))
            {
                return;
            }

            IEnumerable<Object> assets = AssetDatabase.LoadAllAssetsAtPath(importer!.assetPath).Where(x => x.GetType() == typeof(Material));
            Directory.CreateDirectory(exportPath);
            string currentDir = Directory.GetCurrentDirectory();
            exportPath = exportPath.Remove(0, currentDir.Length + 1);
            foreach (Object asset in assets)
            {
                string newMatPath = Path.Combine(exportPath, asset.name) + ".mat";
                newMatPath = AssetDatabase.GenerateUniqueAssetPath(newMatPath);
                string error = AssetDatabase.ExtractAsset(asset, newMatPath);
                if (string.IsNullOrEmpty(error))
                {
                    AssetDatabase.WriteImportSettingsIfDirty(newMatPath);
                    AssetDatabase.ImportAsset(newMatPath, ImportAssetOptions.ForceUpdate);
                }
                else
                {
                    Debug.LogWarning($"Failed to extract asset {asset.name}, error: {error}");
                }
            }
        }
    }
}