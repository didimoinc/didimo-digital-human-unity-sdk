using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DigitalSalmon.UI
{
    public static partial class Style
    {
        public class Box
        {
            //-----------------------------------------------------------------------------------------
            // Public GUIStyles:
            //-----------------------------------------------------------------------------------------

            private static readonly Dictionary<int, GUIStyle> styleLookup = new Dictionary<int, GUIStyle>();

            //-----------------------------------------------------------------------------------------
            // Public Methods:
            //-----------------------------------------------------------------------------------------

            // ReSharper disable once MemberHidesStaticFromOuterClass
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