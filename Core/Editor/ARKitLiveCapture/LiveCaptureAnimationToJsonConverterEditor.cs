#if UNITY_EDITOR && USING_LIVE_CAPTURE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.LiveCapture;
using Didimo.Mocap;
using UnityEditor;
using UnityEngine;  


namespace Didimo.Editor
{
    public static class LiveCaptureAnimationToJsonConverter
    {
        public static DidimoAnimationJson ConvertToMocapAnimationObject(AnimationClip animationClip)
        {
            Dictionary<string, List<float>> poseData = new Dictionary<string, List<float>>();

            bool constainsSkeletonData = false;
            List<float>[] headPosition = new List<float>[3];
            List<float>[] headRotation = new List<float>[3];
            List<float>[] leftEyeRotation = new List<float>[3];
            List<float>[] rightEyeRotation = new List<float>[3];
            
            float frameDeltaTime = 1 / animationClip.frameRate;
            
            foreach (EditorCurveBinding editorCurveBinding in AnimationUtility.GetCurveBindings(animationClip))
            {
                // Poses
                if (editorCurveBinding.propertyName.StartsWith("m_BlendShapes."))
                {
                    GetDataFromAnimationCurve(animationClip, editorCurveBinding, frameDeltaTime, out string poseName, out List<float> animationValues);
                    poseName = DidimoLiveCaptureMapper.RemapShapeName(poseName);
                    poseData.Add(poseName, animationValues);
                }
                // Rest of join transforms (positions and rotations)
                else if (editorCurveBinding.propertyName.StartsWith("m_HeadPosition."))
                {
                    constainsSkeletonData = true;
                    GetDataFromAnimationCurve(animationClip, editorCurveBinding, frameDeltaTime, out int index, out List<float> animationValues);
                    headPosition[index] = animationValues;
                }
                
                else if (editorCurveBinding.propertyName.StartsWith("m_HeadOrientation."))
                {
                    constainsSkeletonData = true;
                    GetDataFromAnimationCurve(animationClip, editorCurveBinding, frameDeltaTime, out int index, out List<float> animationValues);
                    headRotation[index] = animationValues;
                }
                
                
                else if (editorCurveBinding.propertyName.StartsWith("m_LeftEyeOrientation."))
                {
                    constainsSkeletonData = true;
                    GetDataFromAnimationCurve(animationClip, editorCurveBinding, frameDeltaTime, out int index, out List<float> animationValues);
                    leftEyeRotation[index] = animationValues;
                }
                
                
                else if (editorCurveBinding.propertyName.StartsWith("m_RightEyeOrientation."))
                {
                    constainsSkeletonData = true;
                    GetDataFromAnimationCurve(animationClip, editorCurveBinding, frameDeltaTime, out int index, out List<float> animationValues);
                    rightEyeRotation[index] = animationValues;
                }
            }

            Dictionary<string, List<float[]>> skeletonValues = constainsSkeletonData ? BuildSkeletonData(headPosition, headRotation, leftEyeRotation, rightEyeRotation) : null;

            int frameCount = poseData.Values.Min(e => e.Count);
            List<float> timestamps = new List<float>();
            for (float i = 0; i < frameCount; i++)
            {
                timestamps.Add(i * frameDeltaTime);
            }
            
            return new DidimoAnimationJson(poseData, skeletonValues, timestamps, (int) animationClip.frameRate, timestamps.Last(), frameCount);
        }
        
        
        public static string ConvertToJSONString(AnimationClip animationClip)
        {
            DidimoAnimationJson didimoAnimationJson = ConvertToMocapAnimationObject(animationClip);
            return didimoAnimationJson.ToJSONString();
        }
        
        public static void ConvertAndSaveToFolder(AnimationClip animationClip, string saveFolder, string filename="")
        {
            if (string.IsNullOrEmpty(filename)) filename = $"{animationClip.name}.json";
            string savePath = Path.Combine(saveFolder, filename).Replace("/", "\\");
            ConvertAndSaveToFile(animationClip, savePath);
        }

        public static void ConvertAndSaveToFile(AnimationClip animationClip, string savePath)
        {
            string jsonString = ConvertToJSONString(animationClip);
            File.WriteAllText(savePath, jsonString);
            AssetDatabase.Refresh();
        }

        private static List<float> GetAnimationCurveFloatValues(AnimationCurve animationCurve, float frameDeltaTime)
        {
            if (animationCurve.length == 0) return new List<float>();
            
            List<float> poseValues = new List<float>(animationCurve.length);
            poseValues.Add(animationCurve.keys[0].value);
            for (int ix = 1; ix < animationCurve.length; ix++)
            {
                Keyframe currentKeyframe = animationCurve.keys[ix];
                Keyframe previousKeyframe = animationCurve.keys[ix - 1];

                int intervalFrames = Mathf.RoundToInt((currentKeyframe.time - previousKeyframe.time) / frameDeltaTime);
                if (intervalFrames > 1)
                {
                    // There are missing keyframes, so curve is same value during these
                    for (int intervalFrameIx = 1; intervalFrameIx < intervalFrames; intervalFrameIx++)
                    {
                        poseValues.Add(previousKeyframe.value);
                    }
                }

                poseValues.Add(currentKeyframe.value);
            }
            
            return poseValues;
        }
        
        private static void GetDataFromAnimationCurve(AnimationClip animationClip, EditorCurveBinding editorCurveBinding, float frameDeltaTime, out string name, out List<float> values)
        {
            name = editorCurveBinding.propertyName.Split('.')[1];
            
            AnimationCurve animationCurve = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
            values = GetAnimationCurveFloatValues(animationCurve, frameDeltaTime);
        }
        
        private static void GetDataFromAnimationCurve(AnimationClip animationClip, EditorCurveBinding editorCurveBinding, float frameDeltaTime, out int index, out List<float> values)
        {
            string coordinate = editorCurveBinding.propertyName.Split('.')[1];
            index = GetIndexIntegerFromCoordinate(coordinate);
            
            AnimationCurve animationCurve = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
            values = GetAnimationCurveFloatValues(animationCurve, frameDeltaTime);
        }


        
        private static int GetIndexIntegerFromCoordinate(string coordinate)
        {
            switch (coordinate)
            {
                case "x": return 0;
                case "y": return 1;
                case "z": return 2;
                case "w": return 4;
                default:  throw new Exception($"Could not parse coordinate {coordinate} into index int");
            }
        }

        private static Dictionary<string, List<float[]>> BuildSkeletonData(List<float>[] headPosition, List<float>[] headRotation, List<float>[] leftEyeRotation, List<float>[] rightEyeRotation)
        {
            Dictionary<string, List<float[]>> skeletonData = new Dictionary<string, List<float[]>>();
            if (headPosition.All(e => e != null)) skeletonData.Add("headPosition", BuildVector3FromListArray(headPosition));
            if (headRotation.All(e => e != null)) skeletonData.Add("headRotation", BuildVector3FromListArray(headRotation));
            if (leftEyeRotation.All(e => e != null)) skeletonData.Add("leftEyeRotation", BuildVector3FromListArray(leftEyeRotation));
            if (rightEyeRotation.All(e => e != null)) skeletonData.Add("rightEyeRotation", BuildVector3FromListArray(rightEyeRotation));
            return skeletonData;
        }

        private static List<float[]> BuildVector3FromListArray(List<float>[] data)
        {
            List<float[]> result = new List<float[]>();
            for (int index = 0; index < data[0].Count; index++)
            {
                result.Add(new float[] { data[0][index], data[1][index], data[2][index]});
            }
            return result;
        }
    }


    public class LiveCaptureAnimationToJsonConverterEditor : EditorWindow
    {

        private const   string EDITOR_PREFS_SAVE_LOCATION = "Didimo.LiveCaptureToJson.LastSaveLocation";
        protected const string DEFAULT_MOCAP_NAME         = "mocap.json";

        [SerializeField]
        private AnimationClip mocapAnimationClip;

        [SerializeField]
        private string saveLocation;




        [MenuItem("Didimo/Tools/Live Capture Animation Converter")]
        public static void ShowWindow()
        {
            LiveCaptureAnimationToJsonConverterEditor window = GetWindow<LiveCaptureAnimationToJsonConverterEditor>();
            window.titleContent = new GUIContent("Live Capture Animation Converter");
            window.Show();
        }

        private void OnEnable()
        {
            saveLocation = EditorPrefs.GetString(EDITOR_PREFS_SAVE_LOCATION, $"{Application.dataPath}/{DEFAULT_MOCAP_NAME}");
        }

        private void OnGUI()
        {
            
            mocapAnimationClip = EditorGUILayout.ObjectField("Mocap Animation Clip", mocapAnimationClip, typeof(AnimationClip), false) as AnimationClip;
            
            EditorGUILayout.BeginHorizontal();
            saveLocation = EditorGUILayout.TextField("Save Location", saveLocation);
            if (GUILayout.Button("Change", GUILayout.ExpandWidth(false)))
            {
                string animationName = mocapAnimationClip == null ? DEFAULT_MOCAP_NAME : $"{mocapAnimationClip.name}.json";
                string directory = Path.GetDirectoryName(saveLocation);

                string newSaveLocation = EditorUtility.SaveFilePanel("Mocap JSON location", directory, animationName, "json");
                if (!string.IsNullOrEmpty(newSaveLocation))
                {
                    saveLocation = newSaveLocation;
                    EditorPrefs.SetString(EDITOR_PREFS_SAVE_LOCATION, saveLocation);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("SAVE MOCAP", GUILayout.ExpandWidth(true))) Convert(saveLocation);
        }
        
        private void Convert(string filepath)
        {
            if (mocapAnimationClip == null)
            {
                Debug.LogWarning("Unable to save, animation clip is null");
                return;
            }

            if (string.IsNullOrEmpty(filepath))
            {
                Debug.LogWarning("Unable to save, filepath is empty or null");
                return;
            }
            
            LiveCaptureAnimationToJsonConverter.ConvertAndSaveToFile(mocapAnimationClip, filepath);
        }
    }
}
#endif