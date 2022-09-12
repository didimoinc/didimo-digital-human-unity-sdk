using System;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public static class TypeColors
    {
        public const string Alpha = "BB";
        public const string Mesh = "#B02006" + Alpha;
        public const string Transform = "#00CC4D" + Alpha;
        public const string SkinnedMeshRenderer = "#209030" + Alpha;
        public const string TPSWeights = "#752050" + Alpha;
        public const string BoneIndexRemap = "#432616" + Alpha;
        public const string GameObject = "#4040B0" + Alpha;
        public const string Material = "#1040B0" + Alpha;
        public const string String = "#F0D817" + Alpha;
        public const string Texture = "#5D6D7E" + Alpha;

        public static bool GetColorByType(Type type, out Color color, float brightness = 1f, float alpha = 1f)
        {
            string name = type.Name.Replace("[]", "");
            FieldInfo field = typeof(TypeColors).GetField(name, BindingFlags.Static | BindingFlags.Public);
            string value = field?.GetValue(null) as string;
            bool result = ColorUtility.TryParseHtmlString(value, out Color c);
            if (result)
            {
                Color.RGBToHSV(c, out float h, out float s, out float v);
                Color c2 = Color.HSVToRGB(h, s, v * brightness);
                color = new Color(c2.r, c2.g, c2.b, alpha);
            }
            else color = Color.black;
            return result;
        }
    }
}
