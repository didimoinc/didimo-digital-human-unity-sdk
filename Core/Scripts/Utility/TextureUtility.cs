using UnityEngine;

namespace Didimo
{
    public static class TextureUtility
    {
        public static Texture OpacityToDiffuseAlpha(Texture texture) => EncodeToAlpha(texture, Color.white);

        public static Texture RoughnessToMetallicAlpha(Texture texture) => EncodeToAlpha(texture, Color.black, true);

        public static Texture EncodeToAlpha(Texture input, Color rgb, bool invert = false)
        {
            RenderTexture output = new RenderTexture(input.width, input.height, 32, RenderTextureFormat.ARGB32, input.mipmapCount);
            Material mat = new Material(Shader.Find("Hidden/Didimo/Utility/EncodeToAlpha"));
            mat.SetColor("_Base", rgb);
            mat.SetInt("_Invert", invert ? 1 : 0);
            Graphics.Blit(input, output, mat);
            return output;
        }
    }
}