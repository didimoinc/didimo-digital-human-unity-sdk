using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Didimo.Builder;

namespace Didimo.Core.Editor
{
    public class FBXDidimoImporter : AssetPostprocessor
    {
        public const string FBX_EXTENSION = ".fbx";

        public static Dictionary<string, DidimoImporterJsonConfig> ImporterConfigCache = new ();

        AnimationClip[] animationClips = null;
        AnimationClip resetAnimationClip = null;

        public override int GetPostprocessOrder()
        {
            return 10; // must be after unity's post processor so it doesn't overwrite our own stuff
        }

        void OnPreprocessModel()
        {
            // Only process .FBX files. This gets called for every ModelImporter
            if (!assetPath.ToLowerInvariant().EndsWith(FBX_EXTENSION)) return;

            DidimoImporterJsonConfig importerJsonConfig = DidimoImporterJsonConfigUtils.GetConfigAtFolder(Path.GetDirectoryName(assetPath));
            if (importerJsonConfig == null) return;

            ModelImporter importer = assetImporter as ModelImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            // Temporary settings while we don't have multiple materials on hair pieces, on the DGP
            importer.optimizeMeshPolygons = false;
            importer.optimizeMeshVertices = false;
            importer.isReadable = true;
            importer.weldVertices = false;
            importer.SaveAndReimport();

            SplitAnimationClips(importerJsonConfig);
        }

        void OnPostprocessModel(GameObject g)
        {
            // Only process .FBX files. This gets called for every ModelImporter
            if (!assetPath.ToLowerInvariant().EndsWith(FBX_EXTENSION)) return;

            DidimoImporterJsonConfig importerJsonConfig = DidimoImporterJsonConfigUtils.GetConfigAtFolder(Path.GetDirectoryName(assetPath));
            if (importerJsonConfig == null) return;
            
            animationClips = AnimationUtility.GetAnimationClips(g);
            // CleanupAnimations(animationClips, 0f, 0f, 0f);

            resetAnimationClip = GenerateResetAnimationClip(animationClips);
            context.AddObjectToAsset(resetAnimationClip.name, resetAnimationClip);
            
            DidimoImporterJsonConfigUtils.SetupDidimoForEditor(g, importerJsonConfig, assetPath, animationClips, resetAnimationClip,
                material =>
                {
                    context.AddObjectToAsset(material.name, material);
                });
            ImporterConfigCache.Remove(Path.GetDirectoryName(assetPath));
        }

        
        void SplitAnimationClips(DidimoImporterJsonConfig importerJsonConfig)
        {
            IReadOnlyDictionary<string, int[]> targetData = importerJsonConfig.GetAllPoseClips();
            if (targetData.Count < 1) return;

            List<ModelImporterClipAnimation> animationClips = new List<ModelImporterClipAnimation>(targetData.Count);
            foreach (KeyValuePair<string, int[]> clipData in targetData)
            {
                ModelImporterClipAnimation animationClip = new ModelImporterClipAnimation
                {
                    name = clipData.Key,
                    firstFrame = clipData.Value[0],
                    lastFrame = clipData.Value[1],
                    loopTime = false
                };
                animationClips.Add(animationClip);
            }

            (assetImporter as ModelImporter).clipAnimations = animationClips.ToArray();
            (assetImporter as ModelImporter).SaveAndReimport();
        }

        AnimationClip GenerateResetAnimationClip(AnimationClip[] animationClips)
        {
            if (animationClips.Length == 0) return null;

            AnimationClip resetPoseAnimationClip = new AnimationClip();
            resetPoseAnimationClip.name = "RESET_POSE";
            resetPoseAnimationClip.frameRate = animationClips[0].frameRate;
            resetPoseAnimationClip.legacy = animationClips[0].legacy;

            HashSet<string> visitedProperties = new HashSet<string>();

            foreach (AnimationClip animationClip in animationClips)
            {
                foreach (EditorCurveBinding editorCurveBinding in AnimationUtility.GetCurveBindings(animationClip))
                {
                    // string propertyName = editorCurveBinding.propertyName.Contains('.') ? editorCurveBinding.propertyName.Split('.')[0] : editorCurveBinding.propertyName;
                    string key = $"{editorCurveBinding.path}/{editorCurveBinding.propertyName}";

                    if (!visitedProperties.Contains(key))
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
                        resetPoseAnimationClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type,
                            editorCurveBinding.propertyName, new AnimationCurve(curve[0]));
                        visitedProperties.Add(key);
                    }
                }
            }

            return resetPoseAnimationClip;
        }

        void CleanupAnimations(AnimationClip[] animationClips, float minimumAxisTranslation, float minimumAngleRotation,
            float minimumAxisScale)
        {
            foreach (AnimationClip animationClip in animationClips)
            {
                CleanupAnimationClip(animationClip, minimumAxisTranslation, minimumAngleRotation, minimumAxisScale);
            }
        }

        void CleanupAnimationClip(AnimationClip animationClip, float minimumAxisTranslation, float minimumAngleRotation,
            float minimumAxisScale)
        {
            // To cleanup animations we must, first iterate through every property for position (x,y,z) or rotation (x,y,z,w) and find which ones are useful
            // Then iterate again and keep all XYZ(W) curves if any of them if useful. Delete otherwise.
            Dictionary<string, EditorCurveBinding[]> translationCurves = new Dictionary<string, EditorCurveBinding[]>();
            Dictionary<string, EditorCurveBinding[]> rotationCurves = new Dictionary<string, EditorCurveBinding[]>();
            Dictionary<string, EditorCurveBinding[]> eulerRotationCurves =
                new Dictionary<string, EditorCurveBinding[]>();
            Dictionary<string, EditorCurveBinding[]> scaleCurves = new Dictionary<string, EditorCurveBinding[]>();


            foreach (EditorCurveBinding editorCurveBinding in AnimationUtility.GetCurveBindings(animationClip))
            {
                if (!editorCurveBinding.propertyName.Contains('.'))
                {
                    Debug.LogWarning($"Property: {editorCurveBinding.propertyName} does not contains dot");
                    continue;
                }

                string[] properties = editorCurveBinding.propertyName.Split('.');
                string propertyName = properties[0];
                int axisIndex = GetIndexForAxis(properties[1]);


                string key = $"{editorCurveBinding.path} + {propertyName}";
                switch (propertyName)
                {
                    case "m_LocalPosition":
                        if (!translationCurves.ContainsKey(key)) translationCurves.Add(key, new EditorCurveBinding[3]);
                        translationCurves[key][axisIndex] = editorCurveBinding;
                        break;
                    case "m_LocalRotation":
                        if (!rotationCurves.ContainsKey(key)) rotationCurves.Add(key, new EditorCurveBinding[4]);
                        rotationCurves[key][axisIndex] = editorCurveBinding;
                        break;
                    case "m_LocalScale":
                        if (!scaleCurves.ContainsKey(key)) scaleCurves.Add(key, new EditorCurveBinding[3]);
                        scaleCurves[key][axisIndex] = editorCurveBinding;
                        break;
                    case "localEulerRotationRaw":
                        Debug.LogWarning("Found property localEulerRotationRaw");
                        if (!rotationCurves.ContainsKey(key)) rotationCurves.Add(key, new EditorCurveBinding[3]);
                        rotationCurves[key][axisIndex] = editorCurveBinding;
                        break;
                    default:
                        Debug.LogWarning($"Unknown Property name: {propertyName}");
                        break;
                }
            }

            // Remove unnecessary curves for translation
            foreach (KeyValuePair<string, EditorCurveBinding[]> translationCurve in translationCurves)
            {
                if (translationCurve.Value.Length != 3)
                    throw new Exception(
                        $"Translation Curve {translationCurve.Key} contains {translationCurve.Value.Length} values");

                AnimationCurve curveX = AnimationUtility.GetEditorCurve(animationClip, translationCurve.Value[0]);
                AnimationCurve curveY = AnimationUtility.GetEditorCurve(animationClip, translationCurve.Value[1]);
                AnimationCurve curveZ = AnimationUtility.GetEditorCurve(animationClip, translationCurve.Value[2]);

                if (!IsVector3AnimationCurveUseful(curveX, curveY, curveZ, minimumAxisTranslation))
                {
                    AnimationUtility.SetEditorCurve(animationClip, translationCurve.Value[0], null);
                    AnimationUtility.SetEditorCurve(animationClip, translationCurve.Value[1], null);
                    AnimationUtility.SetEditorCurve(animationClip, translationCurve.Value[2], null);
                }
            }

            // Remove unnecessary curves for rotation
            foreach (KeyValuePair<string, EditorCurveBinding[]> rotationCurve in rotationCurves)
            {
                if (rotationCurve.Value.Length != 4)
                    throw new Exception(
                        $"Rotation Curve {rotationCurve.Key} contains {rotationCurve.Value.Length} values");

                AnimationCurve curveX = AnimationUtility.GetEditorCurve(animationClip, rotationCurve.Value[0]);
                AnimationCurve curveY = AnimationUtility.GetEditorCurve(animationClip, rotationCurve.Value[1]);
                AnimationCurve curveZ = AnimationUtility.GetEditorCurve(animationClip, rotationCurve.Value[2]);
                AnimationCurve curveW = AnimationUtility.GetEditorCurve(animationClip, rotationCurve.Value[3]);

                if (!IsQuaternionAnimationCurveUseful(curveX, curveY, curveZ, curveW, minimumAngleRotation))
                {
                    AnimationUtility.SetEditorCurve(animationClip, rotationCurve.Value[0], null);
                    AnimationUtility.SetEditorCurve(animationClip, rotationCurve.Value[1], null);
                    AnimationUtility.SetEditorCurve(animationClip, rotationCurve.Value[2], null);
                    AnimationUtility.SetEditorCurve(animationClip, rotationCurve.Value[3], null);
                }
            }

            // Remove unnecessary curves for rotation
            foreach (KeyValuePair<string, EditorCurveBinding[]> eulerRotationCurve in eulerRotationCurves)
            {
                if (eulerRotationCurve.Value.Length != 3)
                    throw new Exception(
                        $"Rotation Curve {eulerRotationCurve.Key} contains {eulerRotationCurve.Value.Length} values");

                AnimationCurve curveX = AnimationUtility.GetEditorCurve(animationClip, eulerRotationCurve.Value[0]);
                AnimationCurve curveY = AnimationUtility.GetEditorCurve(animationClip, eulerRotationCurve.Value[1]);
                AnimationCurve curveZ = AnimationUtility.GetEditorCurve(animationClip, eulerRotationCurve.Value[2]);

                if (!IsEulerRotationAnimationCurveUseful(curveX, curveY, curveZ, minimumAngleRotation))
                {
                    AnimationUtility.SetEditorCurve(animationClip, eulerRotationCurve.Value[0], null);
                    AnimationUtility.SetEditorCurve(animationClip, eulerRotationCurve.Value[1], null);
                    AnimationUtility.SetEditorCurve(animationClip, eulerRotationCurve.Value[2], null);
                }
            }


            // Remove unnecessary curves for scale
            foreach (KeyValuePair<string, EditorCurveBinding[]> scaleCurve in scaleCurves)
            {
                if (scaleCurve.Value.Length != 3)
                    throw new Exception(
                        $"Translation Curve {scaleCurve.Key} contains {scaleCurve.Value.Length} values");

                AnimationCurve curveX = AnimationUtility.GetEditorCurve(animationClip, scaleCurve.Value[0]);
                AnimationCurve curveY = AnimationUtility.GetEditorCurve(animationClip, scaleCurve.Value[1]);
                AnimationCurve curveZ = AnimationUtility.GetEditorCurve(animationClip, scaleCurve.Value[2]);

                if (!IsVector3AnimationCurveUseful(curveX, curveY, curveZ, minimumAxisScale))
                {
                    AnimationUtility.SetEditorCurve(animationClip, scaleCurve.Value[0], null);
                    AnimationUtility.SetEditorCurve(animationClip, scaleCurve.Value[1], null);
                    AnimationUtility.SetEditorCurve(animationClip, scaleCurve.Value[2], null);
                }
            }

            // Set Remaining Curves to linear
            foreach (EditorCurveBinding editorCurveBinding in AnimationUtility.GetCurveBindings(animationClip))
            {
                AnimationCurve animationCurve = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
                for (int keyframeIx = 0; keyframeIx < animationCurve.length; keyframeIx++)
                {
                    AnimationUtility.SetKeyBroken(animationCurve, keyframeIx, true);
                    AnimationUtility.SetKeyLeftTangentMode(animationCurve, keyframeIx,
                        AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(animationCurve, keyframeIx,
                        AnimationUtility.TangentMode.Linear);
                }

                AnimationUtility.SetEditorCurve(animationClip, editorCurveBinding, animationCurve);
            }
        }


        int GetIndexForAxis(string axis)
        {
            switch (axis)
            {
                case "x": return 0;
                case "y": return 1;
                case "z": return 2;
                case "w": return 3;
                default: throw new ArgumentException($"Axis value is incorrect {axis}");
            }
        }


        bool IsVector3AnimationCurveUseful(AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ,
            float differenceEpsilon)
        {
            // Same implementation as the one in GLTFAnimation.cs 
            if (curveX.length < 1 || curveY.length < 1 || curveZ.length < 1) return false;

            Vector3 restPosePosition = new Vector3(curveX[0].value, curveY[0].value, curveZ[0].value);
            for (int keyIx = 1; keyIx < curveX.length; keyIx++)
            {
                Vector3 deformedPosition = new Vector3(curveX[keyIx].value, curveY[keyIx].value, curveZ[keyIx].value);
                if (IsVector3Useful(deformedPosition - restPosePosition, differenceEpsilon)) return true;
            }

            return false;
        }

        bool IsVector3Useful(Vector3 vector, float epsilon)
        {
            return Mathf.Abs(vector.x) > epsilon || Mathf.Abs(vector.y) > epsilon || Mathf.Abs(vector.z) > epsilon;
        }

        bool IsQuaternionAnimationCurveUseful(AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ,
            AnimationCurve curveW, float differenceEpsilon)
        {
            // Same implementation as the one in GLTFAnimation.cs
            if (curveX.length != curveY.length || curveX.length != curveZ.length || curveX.length != curveW.length)
            {
                throw new ArgumentException("Quaternion animation curves don't have the same length");
            }

            if (curveX.length < 1) return false;

            Quaternion restPoseRotation =
                new Quaternion(curveX[0].value, curveY[0].value, curveZ[0].value, curveW[0].value);
            for (int keyIx = 1; keyIx < curveX.length; keyIx++)
            {
                Quaternion deformedRotation = new Quaternion(curveX[keyIx].value, curveY[keyIx].value,
                    curveZ[keyIx].value, curveW[keyIx].value);
                if (Quaternion.Angle(deformedRotation, restPoseRotation) > differenceEpsilon) return true;
            }

            return false;
        }

        bool IsEulerRotationAnimationCurveUseful(AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ,
            float differenceEpsilon)
        {
            // Same implementation as the one in GLTFAnimation.cs
            if (curveX.length != curveY.length || curveX.length != curveZ.length)
            {
                throw new ArgumentException("Quaternion animation curves don't have the same length");
            }

            if (curveX.length < 1) return false;

            Quaternion restPoseRotation = Quaternion.Euler(curveX[0].value, curveY[0].value, curveZ[0].value);
            for (int keyIx = 1; keyIx < curveX.length; keyIx++)
            {
                Quaternion deformedRotation =
                    Quaternion.Euler(curveX[keyIx].value, curveY[keyIx].value, curveZ[keyIx].value);
                if (Quaternion.Angle(deformedRotation, restPoseRotation) > differenceEpsilon) return true;
            }

            return false;
        }
    }
}