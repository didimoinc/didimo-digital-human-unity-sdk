using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Didimo.Core.Editor
{
    public class InstallUnityPackage
    {
        private AddRequest request;

        public void AddPackage(string packageUri)
        {
            request = Client.Add(packageUri);
            EditorApplication.update += Progress;
        }

        public void Progress()
        {
            if (request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                    UnityEngine.Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure)
                    UnityEngine.Debug.Log(request.Error.message);

                EditorApplication.update -= Progress;

                if (request.Status == StatusCode.Success)
                    ReopenProject();

            }
        }

        public static void ReopenProject()
        {
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }
    }
}