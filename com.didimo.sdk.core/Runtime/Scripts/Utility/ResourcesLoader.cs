using System.IO;
using Didimo.Core.Config;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo.Core.Utility
{
    public static class ResourcesLoader
    {
        public static string[] PipelineName = { "URP", "HDRP", "SRP", "Unknown" };

        private static string PIPELINE_RESOURCES_NAME = "PipelineResources";
        private static string RENDER_PIPELINE_MATERIALS = "RenderPipelineMaterials";
        private static string RENDER_PIPELINE_MATERIALS_PATH = "Assets/Didimo/Resources";
        private static string DEFAULT_MATERIAL_RESOURCES_URP_PATH =
            "Packages/com.didimo.sdk.core/Runtime/Pipeline/MaterialResourcesURP.asset";
        
        private static string DEFAULT_MATERIAL_RESOURCES_HDRP_PATH =
            "Packages/com.didimo.sdk.core/Runtime/Pipeline/MaterialResourcesHDRP.asset"; 
        
        private static string DEFAULT_PIPELINE_RESOURCES_URP_PATH =
            "Packages/com.didimo.sdk.core/Runtime/Pipeline/URP/UniversalRP-HighQuality.asset"; 

        // You should only have a single PipelineResources file on your project
        public static PipelineResources GetPipelineResources()
        {
            PipelineResources pipelineResources = Resources.Load<PipelineResources>(PIPELINE_RESOURCES_NAME);
#if UNITY_EDITOR
            if (pipelineResources == null)
            {
                // Debug.Log("Creating scriptable object PipelineResources.");
                pipelineResources = ScriptableObject.CreateInstance<PipelineResources>();
                pipelineResources.URPPipelineAsset =
                    AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(DEFAULT_PIPELINE_RESOURCES_URP_PATH);
                
                Directory.CreateDirectory(RENDER_PIPELINE_MATERIALS_PATH);
                AssetDatabase.CreateAsset(pipelineResources, GetRenderPipelineMaterialsPath());
            }
#endif
            return pipelineResources;
        }
        
        public static RenderPipelineMaterials GetRenderPipelineMaterials()
        {
            RenderPipelineMaterials renderPipelineMaterials = Resources.Load<RenderPipelineMaterials>(RENDER_PIPELINE_MATERIALS);
#if UNITY_EDITOR
            if (renderPipelineMaterials == null)
            {
                // Debug.Log("Creating scriptable object RenderPipelineMaterials.");
                renderPipelineMaterials = ScriptableObject.CreateInstance<RenderPipelineMaterials>();
                renderPipelineMaterials.URPMaterials =
                    AssetDatabase.LoadAssetAtPath<MaterialResources>(DEFAULT_MATERIAL_RESOURCES_URP_PATH);
                renderPipelineMaterials.HDRPMaterials =
                    AssetDatabase.LoadAssetAtPath<MaterialResources>(DEFAULT_MATERIAL_RESOURCES_HDRP_PATH);
                
                Directory.CreateDirectory(RENDER_PIPELINE_MATERIALS_PATH);
                AssetDatabase.CreateAsset(renderPipelineMaterials, GetRenderPipelineMaterialsPath());
            }
#endif
            return renderPipelineMaterials;
        }
        
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
            var pipelineDB = GetPipelineResources();
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

        // public static string GetAppropriateIDString() { return ResourceIDs[(int) GetAppropriateID()]; }

        #if UNITY_EDITOR
        public static string GetMaterialResourcesPath(EPipelineType id = EPipelineType.EPT_UNKNOWN)
        {
            if (id == EPipelineType.EPT_UNKNOWN)
                id = GetAppropriateID();

            MaterialResources materialResources = GetMaterialResources(id);
            return materialResources != null ? AssetDatabase.GetAssetPath(materialResources) : null;
        }
        
        #endif
        
        public static MaterialResources GetMaterialResources(EPipelineType id = EPipelineType.EPT_UNKNOWN)
        {
            if (id == EPipelineType.EPT_UNKNOWN)
                id = GetAppropriateID();
            
            RenderPipelineMaterials renderPipelineMaterials = GetRenderPipelineMaterials();

            switch (id)
            {
                case EPipelineType.EPT_URP:
                    return renderPipelineMaterials.URPMaterials;
                case EPipelineType.EPT_HDRP:
                    return renderPipelineMaterials.HDRPMaterials;
                default: return null; // TODO: Add EPT_SRP
            }
            
//             // Might be required for the first time the project is loaded
// #if UNITY_EDITOR
//             if (shaderResources == null)
//             {
//                 AssetDatabase.ImportAsset(ResourceFullPaths[(int)id], ImportAssetOptions.ForceSynchronousImport);
//                 shaderResources = AssetDatabase.LoadAssetAtPath<ShaderResources>(ResourceFullPaths[(int)id]);
//             }
// #endif

        }

        public static string GetRenderPipelineMaterialsPath()
        {
            return Path.Combine(RENDER_PIPELINE_MATERIALS_PATH,  RENDER_PIPELINE_MATERIALS + ".asset").Replace("\\", "/");
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