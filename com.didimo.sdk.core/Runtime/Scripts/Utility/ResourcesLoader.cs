using Didimo.Core.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo.Core.Utility
{
    public enum EPipelineType
    {
        EPT_URP     = 0,
        EPT_HDRP    = 1,
        EPT_SRP     = 2,
        EPT_UNKNOWN = 3,
    }

    public class ResourcesLoader
    {
        public static string[] ResourceIDs = {"ShaderResources", "ShaderResourcesHDRP", "ShaderResourcesSRP", "Unknown"};

        public static EPipelineType GetAppropriateID()
        {
            if (GraphicsSettings.renderPipelineAsset)
            {
                if (GraphicsSettings.renderPipelineAsset.name.Contains("HDRP"))
                    return EPipelineType.EPT_HDRP;
            }

            return EPipelineType.EPT_URP;
        }

        public static string GetAppropriateIDString() { return ResourceIDs[(int) GetAppropriateID()]; }

        public static ShaderResources ShaderResources(EPipelineType id = EPipelineType.EPT_UNKNOWN)
        {
            if (id == EPipelineType.EPT_UNKNOWN)
                id = GetAppropriateID();
            var idString = ResourceIDs[(int) id];
            var shaderResources = Resources.Load<ShaderResources>(idString);

            // Might be required for the first time the project is loaded
#if UNITY_EDITOR
            if (shaderResources == null)
            {
                shaderResources = AssetDatabase.LoadAssetAtPath<ShaderResources>("Packages/com.didimo.sdk.core/Runtime/Pipeline/Resources/ShaderResources.asset");
            }
#endif

            return shaderResources;
        }

        public static Avatar DidimoDefaultAvatar()
        {
            var didimoDefaultAvatar = Resources.Load<Avatar>("DidimoDefaultAvatar");
            // Might be required for the first time the project is loaded
#if UNITY_EDITOR
            if (didimoDefaultAvatar == null)
            {
                didimoDefaultAvatar = AssetDatabase.LoadAssetAtPath<Avatar>("Packages/com.didimo.sdk.core/Runtime/Content/Resources/DidimoDefaultAvatar.asset");
            }
#endif

            return didimoDefaultAvatar;
        }
    }
}