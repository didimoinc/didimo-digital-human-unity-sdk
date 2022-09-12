using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public static class TextureTools
    {
        public static Texture2D CreateReadable(this Texture2D texture)
        {
            int width = texture.width, height = texture.height;
            RenderTexture.active = RenderTexture.GetTemporary(width, height);
            RenderTexture.active.filterMode = texture.filterMode;
            Graphics.Blit(texture, RenderTexture.active);

            Texture2D output = new Texture2D(width, height);
            output.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            output.Apply();

            RenderTexture.active = null;
            return output;
        }
    }
}
