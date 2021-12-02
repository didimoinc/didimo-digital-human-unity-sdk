using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

#if USING_UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Didimo.Core.Editor
{
    public class CheckUrpSelected : IProjectSettingIssue
    {
        private const string PATH = "Assets/Didimo/Core/Pipeline/" +
            "Universal Render Pipeline/UniversalRP-HighQuality.asset";

        public const string REASON =
            "didimos can only be rendered with URP.\n" +
            "Please select a UniversalRenderPipelineAsset in" +
            " the graphics settings.";

        public bool CheckOk()
        {

#if USING_UNITY_URP
            if (GraphicsSettings.renderPipelineAsset != null &&
                typeof(UniversalRenderPipelineAsset)
                == GraphicsSettings.renderPipelineAsset.GetType())
            {
                return true;
            }
            
            EditorGUILayout.HelpBox(ResolveText(), MessageType.Error);
#endif


            return false;
        }

        public void Resolve()
        {
            RenderPipelineAsset obj = AssetDatabase.LoadAssetAtPath(
                PATH, typeof(RenderPipelineAsset)) as RenderPipelineAsset;

            GraphicsSettings.renderPipelineAsset = obj;
            QualitySettings.renderPipeline = obj;
        }

        public string ResolveText()
        {
            return REASON;
        }

    }
}
