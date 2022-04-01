using System.IO;
using System.Linq;
using Didimo.Core.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo.Core.Utility
{
    public enum EPipelineType
    {
        EPT_URP = 0,
        EPT_HDRP = 1,
        EPT_SRP = 2,
        EPT_UNKNOWN = 3,
    }
    public class ResourcesLoader
    {
        public static string[] ResourceIDs = { "ShaderResources", "ShaderResourcesHDRP", "ShaderResourcesSRP", "Unknown"};

        public static EPipelineType GetAppropriateID()
        {
            if (GraphicsSettings.renderPipelineAsset)
            {
                if (GraphicsSettings.renderPipelineAsset.name.Contains("HDRP"))
                    return EPipelineType.EPT_HDRP;
            }
            return EPipelineType.EPT_URP;
        }

        public static string GetAppropriateIDString()
        {
            return ResourceIDs[(int)GetAppropriateID()];
        }

        public static ShaderResources ShaderResources(EPipelineType id = EPipelineType.EPT_UNKNOWN)
        {
            if (id == EPipelineType.EPT_UNKNOWN)
                id = GetAppropriateID();
            var idString = ResourceIDs[(int)id];
            var shaderResources = Resources.Load<ShaderResources>(idString);
                                   
            // Might be required for the first time the project is loaded
            #if UNITY_EDITOR
            if (shaderResources == null)
            {
                string path = Directory
                    .GetFiles("Packages/com.didimo.sdk.core", "ShaderResources.asset", SearchOption.AllDirectories)
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

        public static Avatar DidimoDefaultAvatar()
        {
            var didimoDefaultAvatar = Resources
                .Load<Avatar>("DidimoDefaultAvatar");
            // Might be required for the first time the project is loaded
#if UNITY_EDITOR
            if (didimoDefaultAvatar == null)
            {
                string path = Directory
                              .GetFiles("Packages/com.didimo.sdk.core", "DidimoDefaultAvatar.asset", SearchOption.AllDirectories)
                              .FirstOrDefault();
                if (path != null)
                {
                    didimoDefaultAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
                }
            }
#endif
                                   
            return didimoDefaultAvatar;
        }
    }
}