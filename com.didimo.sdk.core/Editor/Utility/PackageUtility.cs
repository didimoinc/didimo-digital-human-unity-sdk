using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Didimo.Core.Editor
{
    public static class PackageUtility
    {
        public static async Task ImportSampleAndLoadScene(string packageName, string sampleName, string scenePath, [CanBeNull] Action<bool> onCompletion = null)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Importing sample...", "Please wait while we import the sample", 0);
                ListRequest request = Client.List();

                while (!request.IsCompleted) await Task.Delay(100);
                if (request.Status == StatusCode.Failure)
                {
                    onCompletion?.Invoke(false);
                    return;
                }

                PackageInfo package = request.Result.First(p => p.name == packageName);
                EditorUtility.DisplayProgressBar("Importing sample...", "Please wait while we import the sample", .5f);

                Sample sample = Sample.FindByPackage(package.name, package.version).FirstOrDefault(s => string.IsNullOrEmpty(sampleName) || s.displayName == sampleName);
                string importPath = sample.importPath.Replace("\\", "/").Remove(0, Application.dataPath.Length + 1);
                sample.Import();
                string path = $"Assets/{importPath}/{scenePath}";

                SceneAsset obj = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset)) as SceneAsset;
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);

                EditorUtility.DisplayProgressBar("Importing sample...", "Please wait while we import the sample", 1);
                
                AssetDatabase.Refresh();

                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                onCompletion?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onCompletion?.Invoke(false);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}