using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Didimo.Core.Editor
{
    public static class PackageUtility
    {
        public static async Task<Sample?> GetSample(string packageName, string sampleName)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Fetching sample...", "Please wait while we fetch the sample", 0);
                ListRequest request = Client.List();

                while (!request.IsCompleted) await Task.Delay(100);
                if (request.Status == StatusCode.Failure)
                {
                    return null;
                }

                PackageInfo package = request.Result.First(p => p.name == packageName);
                Sample sample = Sample.FindByPackage(package.name, package.version).FirstOrDefault(s => string.IsNullOrEmpty(sampleName) || s.displayName == sampleName);
                return sample;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static bool ImportSample(Sample sample)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Importing sample...", "Please wait while we import the sample", 1);
                sample.Import();
                AssetDatabase.Refresh();
                EditorUtility.DisplayProgressBar("Importing sample...", "Please wait while we import the sample", 1);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return true;
        }

        public static string GetSceneAssetPath(Sample sample, string scenePath)
        {
            string importPath = sample.importPath.Replace("\\", "/").Remove(0, Application.dataPath.Length + 1);
            return $"Assets/{importPath}/{scenePath}";
        }
        
        public static bool LoadSceneFromSample(Sample sample, string scenePath)
        {
            try
            {
                string path = GetSceneAssetPath(sample, scenePath);

                SceneAsset obj = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset)) as SceneAsset;
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);

                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
}