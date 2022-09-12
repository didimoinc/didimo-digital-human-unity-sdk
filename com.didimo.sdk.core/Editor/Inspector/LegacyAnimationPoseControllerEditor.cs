using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Editor
{
    [CustomEditor(typeof(LegacyAnimationPoseController))]
    public class LegacyAnimationPoseControllerEditor : UnityEditor.Editor
    {
        // Target script
        private LegacyAnimationPoseController poseController;

        // Only trigger relevant poses
        private readonly Dictionary<string, float> posesToTrigger = new Dictionary<string, float>();

        // Editor state settings
        private static bool forceUpdatePoses = true;
        private static bool editorPosesListFoldout = true;
        private static GUIStyle deformableFoldoutStyle;

        // Layout Config Constants
        private const float SPACING = 5;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("\"Legacy\" because this component is based on Unity's Legacy Animation System. It is not deprecated.", MessageType.Info);
            // Show default script like default InspectorGUI
            
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(poseController), typeof(LegacyAnimationPoseController), false);
            GUI.enabled = true;

            if (!poseController.gameObject.scene.isLoaded)
            {
                GUI.enabled = false;
            }

            // Control Poses Sections
            EditorGUILayout.Space(SPACING);
            deformableFoldoutStyle ??= new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            editorPosesListFoldout = EditorGUILayout.Foldout(editorPosesListFoldout, "Face Shapes", true, deformableFoldoutStyle);
            if (editorPosesListFoldout)
            {
                EditorGUI.indentLevel++;
                if (poseController.animationClips == null || poseController.animationClips.Length == 0)
                {
                    EditorGUILayout.HelpBox("This didimo does not contain any poses", MessageType.Info);
                }
                else
                {
                    forceUpdatePoses = EditorGUILayout.ToggleLeft("Update Poses in \"EditMode\"", forceUpdatePoses);
                    EditorGUILayout.Space(SPACING);
                    EditorGUILayout.BeginVertical();

                    bool needsUpdate = false;
                    if (poseController.NameToPoseMapping != null)
                        foreach ((string poseName, LegacyAnimationPoseController.DidimoFaceShape poseShape) in poseController.NameToPoseMapping)
                        {
                            EditorGUI.BeginChangeCheck();
                            float poseValue = 0;
                            
                            if (poseShape.AnimationState != null)
                            {
                                poseValue = poseShape.AnimationState.normalizedTime;
                            }
                            
                            if (float.IsNaN(poseValue))
                            {
                                bool enabled = GUI.enabled;
                                GUI.enabled = false;
                                poseValue = 0;
                                EditorGUILayout.Slider(poseName, poseValue, 0f, 1f);
                                GUI.enabled = enabled;
                            }
                            else poseValue = EditorGUILayout.Slider(poseName, poseValue, 0f, 1f);

                            if (EditorGUI.EndChangeCheck())
                            {
                                needsUpdate = true;
                                posesToTrigger[poseName] = poseValue;
                            }
                            else
                            {
                                if (poseValue == 0 && posesToTrigger.ContainsKey(poseName)) posesToTrigger.Remove(poseName);
                                else if (poseValue > 0) posesToTrigger[poseName] = poseValue;
                            }
                        }

                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;

                    if (needsUpdate)
                    {
                        foreach ((string poseName, float poseValue) in posesToTrigger) poseController.SetWeightForPose(poseName, poseValue);
                        if (forceUpdatePoses && !Application.isPlaying) poseController.ForceUpdateAnimation();
                    }
                }
            }

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Reset All Poses", GUILayout.Width(0), GUILayout.ExpandWidth(true)))
            {
                poseController.ResetAllPoseWeights();
                if (forceUpdatePoses && !Application.isPlaying) poseController.ForceUpdateAnimation();
            }
            if (GUILayout.Button("Update Poses (EditMode)", GUILayout.Width(0), GUILayout.ExpandWidth(true))) poseController.ForceUpdateAnimation();
            EditorGUILayout.EndHorizontal();

            // Head Section
            EditorGUILayout.Space(SPACING);
            GUILayout.Label("Head Movement", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            poseController.headJointMovementEnabled = EditorGUILayout.ToggleLeft("Enabled", poseController.headJointMovementEnabled);
            poseController.headJointWeight = EditorGUILayout.Slider("Head Joint Weight", poseController.headJointWeight, 0f, 1f);
            EditorGUI.indentLevel--;
            
            GUI.enabled = true;
        }

        private void OnEnable()
        {
            poseController = (LegacyAnimationPoseController)target;
        }
    }
}