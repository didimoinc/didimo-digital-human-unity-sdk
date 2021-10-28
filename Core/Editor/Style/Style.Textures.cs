using UnityEngine;

namespace DigitalSalmon.UI
{
    public static partial class Style
    {
        public class Textures
        {
            //-----------------------------------------------------------------------------------------
            // Helper Methods:
            //-----------------------------------------------------------------------------------------

            public static Texture2D FlatTexture(Color color)
            {
                Texture2D returnTex = new Texture2D(1, 1, TextureFormat.RGBA32, false, IsLinear) {filterMode = FilterMode.Point};
                returnTex.SetPixel(0, 0, color);
                returnTex.Apply();
                return returnTex;
            }

            private static RenderTexture GetTemporaryRenderTexture(Vector2Int size) => RenderTexture.GetTemporary(size.x, size.y, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        }
    }
}