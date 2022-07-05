using Didimo.Core.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo.Core.Utility
{
  

    public static class ResourcesLoader
    {
        public static string[] ResourceIDs = {"ShaderResources", "ShaderResourcesHDRP", "ShaderResourcesSRP", "Unknown"};
        public static string[] PipelineName = { "URP", "HDRP", "SRP", "Unknown" };
        public static string[] PipelineDescription = { "Universal Render Pipeline", "High Definition Render Pipeline", "Standard Render Pipeline", "Unknown" };

        
        public static EPipelineType GetAppropriateID()
        {
            if (GraphicsSettings.renderPipelineAsset)
            {
                if (GraphicsSettings.renderPipelineAsset.name.Contains("HDRP"))
                    return EPipelineType.EPT_HDRP;
            }

            return EPipelineType.EPT_URP;
        }

        public static void SetPipeline(EPipelineType pipeline)
        {
            var pipelineDB = Resources.Load<PipelineResources>("PipelineResources");
            if (pipelineDB)
            {
                RenderPipelineAsset pipelineAsset = null;
                switch (pipeline)
                {
                    case EPipelineType.EPT_HDRP:
                        pipelineAsset = pipelineDB.HDRPPipelineAsset;

                        break;
                    case EPipelineType.EPT_URP:
                        pipelineAsset = pipelineDB.URPPipelineAsset;
                        break;
                }
                if (pipelineAsset != null)
                {                    
                    GraphicsSettings.defaultRenderPipeline = pipelineAsset;
                    QualitySettings.renderPipeline = pipelineAsset;
                }
            }

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