using System;
using System.Globalization;
using UnityEngine;

namespace DigitalSalmon.Extensions
{
    [Flags]
    public enum RGBA
    {
        Red,
        Green,
        Blue,
        Alpha
    }

    public static class ColorExtensions
    {
        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public static Vector4 ToVector4(this Color self) => new Vector4(self.r, self.g, self.b, self.a);

        /// <summary>
        /// Lerps between multiple colors.
        /// </summary>
        /// <param name="self">The colors.</param>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static Color Lerp(this Color[] self, float t)
        {
            t = Mathf.Clamp(t, 0, 1) * (self.Length - 1);
            int a = (int) t;
            int b = Mathf.Min((int) t + 1, self.Length - 1);
            return Color.Lerp(self[a], self[b], t - (int) t);
        }

        /// <summary>
        /// Moves the towards implementation for Color.
        /// </summary>
        /// <param name="self">From color.</param>
        /// <param name="b">To color.</param>
        /// <param name="maxDelta">The maximum delta.</param>
        public static Color MoveTowards(this Color self, Color b, float maxDelta)
        {
            Color result = new Color {r = Mathf.MoveTowards(self.r, b.r, maxDelta), g = Mathf.MoveTowards(self.g, b.g, maxDelta), b = Mathf.MoveTowards(self.b, b.b, maxDelta), a = Mathf.MoveTowards(self.a, b.a, maxDelta)};

            self.r = result.r;
            self.g = result.g;
            self.b = result.b;
            self.a = result.a;

            return result;
        }

        /// <summary>
        /// Converts a color to a string formatted to c#
        /// </summary>
        /// <param name="self">The color.</param>
        /// <returns>new Color(r, g, b, a)</returns>
        public static string ToCSharpColor(this Color self) => "new Color(" + TrimFloat(self.r) + "f, " + TrimFloat(self.g) + "f, " + TrimFloat(self.b) + "f, " + TrimFloat(self.a) + "f)";

        /// <summary>
        /// Pows the color with the specified factor.
        /// </summary>
        /// <param name="self">The color.</param>
        /// <param name="factor">The factor.</param>
        public static Color Pow(this Color self, float factor)
        {
            self.r = Mathf.Pow(self.r, factor);
            self.g = Mathf.Pow(self.g, factor);
            self.b = Mathf.Pow(self.b, factor);
            self.a = Mathf.Pow(self.a, factor);
            return self;
        }

        /// <summary>
        /// Normalizes the RGB values of the color ignoring the alpha value.
        /// </summary>
        /// <param name="self">The color.</param>
        public static Color NormalizeRGB(this Color self)
        {
            Vector3 c = new Vector3(self.r, self.g, self.b).normalized;
            self.r = c.x;
            self.g = c.y;
            self.b = c.z;
            return self;
        }

        /// <summary>
        /// Convert an rgb color to hsv.
        /// </summary>
        public static Vector3 ToHsv(this Color self)
        {
            float r = self.r / 255;
            float g = self.r / 255;
            float b = self.r / 255;

            float h = 0; // default to black
            float s = 0;
            float v = Mathf.Max(r, g);
            v = Mathf.Max(v, b);
            float m = Mathf.Min(r, g);
            m = Mathf.Min(m, b);
            float l = (m + v) / 2;
            if (l <= 0)
            {
                return new Vector3(h, s, l);
            }

            float vm = v - m;
            s = vm;
            if (s > 0)
            {
                s /= l <= 0.5f ? v + m : 2 - v - m;
            }
            else
            {
                return new Vector3(h, s, l);
            }

            float r2 = (v - r) / vm;
            float g2 = (v - g) / vm;
            float b2 = (v - b) / vm;
            if (r == v)
            {
                h = g == m ? 5 + b2 : 1 - g2;
            }
            else if (g == v)
            {
                h = b == m ? 1 + r2 : 3 - b2;
            }
            else
            {
                h = r == m ? 3 + g2 : 5 - r2;
            }

            h /= 6;

            return new Vector3(h, s, l);
        }

        /// <summary>
        /// Convert an rgb color to hsb.
        /// </summary>
        public static Vector3 ToHsb(this Color self)
        {
            Vector3 ret = Vector3.zero;

            float r = self.r;
            float g = self.g;
            float b = self.b;

            float max = Mathf.Max(r, Mathf.Max(g, b));

            if (max <= 0)
            {
                return ret;
            }

            float min = Mathf.Min(r, Mathf.Min(g, b));
            float dif = max - min;

            if (max > min)
            {
                if (g == max)
                {
                    ret.x = (b - r) / dif * 60f + 120f;
                }
                else if (b == max)
                {
                    ret.x = (r - g) / dif * 60f + 240f;
                }
                else if (b > g)
                {
                    ret.x = (g - b) / dif * 60f + 360f;
                }
                else
                {
                    ret.x = (g - b) / dif * 60f;
                }

                if (ret.x < 0)
                {
                    ret.x = ret.x + 360f;
                }
            }
            else
            {
                ret.x = 0;
            }

            ret.x *= 1f / 360f;
            ret.y = dif / max * 1f;
            ret.z = max;

            return ret;
        }

        /// <summary>
        /// Return 'color' where 'color'.a = alpha.
        /// </summary>
        public static Color WithAlpha(this Color self, float alpha)
        {
            self.a = alpha;
            return self;
        }

        public static Color WithMultipliedAlpha(this Color self, float alpha)
        {
            self.a *= alpha;
            return self;
        }

        /// <summary>
        /// Return 'color' where 'color'.r = red.
        /// </summary>
        public static Color WithRed(this Color self, float red)
        {
            self.r = red;
            return self;
        }

        /// <summary>
        /// Return 'color' where 'color'.g = green.
        /// </summary>
        public static Color WithGreen(this Color self, float green)
        {
            self.g = green;
            return self;
        }

        /// <summary>
        /// Return 'color' where 'color'.b = blue.
        /// </summary>
        public static Color WithBlue(this Color self, float blue)
        {
            self.b = blue;
            return self;
        }

        /// <summary>
        /// Return 'color' where 'color'.component = value.
        /// </summary>
        public static Color WithComponent(this Color self, RGBA component, float value)
        {
            switch (component)
            {
                case RGBA.Red:
                    self.r = value;
                    break;
                case RGBA.Green:
                    self.g = value;
                    break;
                case RGBA.Blue:
                    self.b = value;
                    break;
                case RGBA.Alpha:
                    self.a = value;
                    break;
            }

            return self;
        }

        //-----------------------------------------------------------------------------------------
        // Private Methods:
        //-----------------------------------------------------------------------------------------

        private static string TrimFloat(float value)
        {
            string str = value.ToString("F3", CultureInfo.InvariantCulture).TrimEnd('0');
            char lastChar = str[str.Length - 1];
            if (lastChar == '.' || lastChar == ',')
            {
                str = str.Substring(0, str.Length - 1);
            }

            return str;
        }
    }
}