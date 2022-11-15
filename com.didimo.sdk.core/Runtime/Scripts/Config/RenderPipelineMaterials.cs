using UnityEngine;

namespace Didimo.Core.Config
{
    [CreateAssetMenu(fileName = "RenderPipelineMaterials", menuName = "Didimo/Render Pipeline Materials")]
    public class RenderPipelineMaterials : ScriptableObject
    {
        public MaterialResources URPMaterials;
        public MaterialResources HDRPMaterials;
    }
}