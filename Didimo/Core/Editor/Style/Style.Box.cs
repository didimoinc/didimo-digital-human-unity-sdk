using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Didimo.UI
{
    public static partial class Style
    {
        public class Box
        {
            private static readonly Dictionary<int, GUIStyle> styleLookup = new Dictionary<int, GUIStyle>();

            public static void ClearCache()
            {
                FieldInfo[] fields = typeof(Box).GetFields(BindingFlags.NonPublic | BindingFlags.Static);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsLiteral || field.IsInitOnly) continue;
                    field.SetValue(null, null);
                }
            }

            private static int GetBoxHashCode(Color fill, Color outline, float cornerRadius = 0, float outlineThickness = 0) => fill.GetHashCode() ^ outline.GetHashCode() ^ cornerRadius.GetHashCode() ^ outlineThickness.GetHashCode();
        }
    }
}