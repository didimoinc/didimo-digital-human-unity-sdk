using System.IO;
using System.Linq;
using Didimo.Core.Config;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Utility
{
    public class ResourcesLoader
    {
        public static ShaderResources ShaderResources()
        {
            var shaderResources = Resources
                .Load<ShaderResources>("ShaderResources");

            // Might be required for the first time the project is loaded
#if UNITY_EDITOR
            if (shaderResources == null)
            {
                string path = Directory
                    .GetFiles(Application.dataPath, "ShaderResources.asset", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (path != null)
                {
                    path = path.Replace(Application.dataPath, "");
                    path = "Assets/" + path.Replace("\\", "/");
                    shaderResources = AssetDatabase.LoadAssetAtPath<ShaderResources>(path);
                }
            }
#endif

            return shaderResources;
        }
    }
}