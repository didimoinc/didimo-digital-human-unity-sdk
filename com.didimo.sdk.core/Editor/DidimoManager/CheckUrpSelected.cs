using Didimo.Core.Config;
using Didimo.Core.Utility;
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
            ResourcesLoader.SetPipeline(EPipelineType.EPT_URP);
         }

        public string ResolveText()
        {
            return REASON;
        }

    }
}