using UnityEngine;

namespace Didimo.Core.Config
{
    //[CreateAssetMenu(fileName = "DidimoBuildConfig", menuName = "Didimo/New DidimoBuildConfig")]
    public class DidimoBuildConfig : ScriptableObject
    {
        public SupportedRenderPipelines Pipeline;
    }
}