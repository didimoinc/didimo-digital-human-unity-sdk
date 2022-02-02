using UnityEngine;

namespace Didimo.Core.Config
{
    //[CreateAssetMenu(fileName = "ShaderResources", menuName = "Didimo/Shader Resources")]
    public class ShaderResources : ScriptableObject
    {
        // "Pipeline Shader resources required to load a didimo"
        [Header("Shader Resources")]
        public Shader Eye;
        public Shader Skin;
        public Shader SkinMergedTextures;
        public Shader SkinMergedAtlasedTextures;
        public Shader Mouth;
        public Shader Eyelash;
        public Shader UnlitTexture;
        public Shader Hair;
        public Shader HairOpaque;
    }
}