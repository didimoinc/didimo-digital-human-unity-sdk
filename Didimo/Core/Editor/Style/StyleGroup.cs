using System.Collections.Generic;
using UnityEngine;

namespace DigitalSalmon.UI
{
    public class StyleGroup
    {
        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private readonly GUIStyle baseStyle;
        private          GUIStyle generatedStyle;

        private StyleGroupVariantInfo variantInfo;

        private static readonly Dictionary<Color, Texture2D> backgroundTextureLookup = new Dictionary<Color, Texture2D>();

        private static readonly Dictionary<string, StyleGroup> instanceLookup = new Dictionary<string, StyleGroup>();

        private bool isDirty = true;

        //-----------------------------------------------------------------------------------------
        // Public Proeprties:
        //-----------------------------------------------------------------------------------------

        public GUIStyle Style => GetOrCreateStyle(variantInfo);

        //-----------------------------------------------------------------------------------------
        // Constructors:
        //-----------------------------------------------------------------------------------------

        public StyleGroup(GUIStyle baseStyle)
        {
            this.baseStyle = baseStyle;
            variantInfo = VariantInfoFromGUIStyle(baseStyle);
        }

        public StyleGroup(StyleGroup toCopy)
        {
            baseStyle = toCopy.baseStyle;
            variantInfo = toCopy.variantInfo;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public StyleGroup Instance(string key) => Instance(key, out bool exists);

        public StyleGroup Instance(string key, out bool exists)
        {
            if (instanceLookup.ContainsKey(key))
            {
                exists = true;
                return instanceLookup[key];
            }

            StyleGroup newgroup = new StyleGroup(this);
            instanceLookup.Add(key, newgroup);
            exists = false;
            return newgroup;
        }

        public StyleGroup SetAnchor(TextAnchor anchor)
        {
            if (variantInfo.Anchor == anchor) return this;
            isDirty = true;
            variantInfo.Anchor = anchor;

            return this;
        }

        public StyleGroup SetColor(Color color)
        {
            if (variantInfo.HasFixedColor() && variantInfo.NormalColor == color)
            {
                return this;
            }

            variantInfo = StyleGroupVariantInfo.WithFixedColor(variantInfo, color);
            isDirty = true;
            return this;
        }

        public StyleGroup SetColor(Color normal, Color hover) => SetColor(normal, normal, normal);

        public StyleGroup SetColor(Color normal, Color hover, Color active)
        {
            if (variantInfo.NormalColor != normal)
            {
                variantInfo.NormalColor = normal;
                isDirty = true;
            }

            if (variantInfo.OnNormalColor != normal)
            {
                variantInfo.OnNormalColor = normal;
                isDirty = true;
            }

            if (variantInfo.HoverColor != hover)
            {
                variantInfo.HoverColor = hover;
                isDirty = true;
            }

            if (variantInfo.OnHoverColor != hover)
            {
                variantInfo.OnHoverColor = hover;
                isDirty = true;
            }

            if (variantInfo.ActiveColor != active)
            {
                variantInfo.ActiveColor = active;
                isDirty = true;
            }

            if (variantInfo.OnActiveColor != active)
            {
                variantInfo.OnActiveColor = active;
                isDirty = true;
            }

            return this;
        }

        public StyleGroup SetFontSize(int fontSize)
        {
            if (variantInfo.FontSize == fontSize) return this;

            variantInfo.FontSize = fontSize;
            isDirty = true;
            return this;
        }

        public StyleGroup SetFontWeight(FontWeights fontWeight)
        {
            if (variantInfo.FontWeight == fontWeight) return this;

            variantInfo.FontWeight = fontWeight;
            isDirty = true;
            return this;
        }

        public StyleGroup SetMargin(Vector4Int margin)
        {
            if (variantInfo.Margin == margin) return this;

            variantInfo.Margin = margin;
            isDirty = true;
            return this;
        }

        public StyleGroup SetPadding(Vector4Int padding)
        {
            if (variantInfo.Padding == padding) return this;

            variantInfo.Padding = padding;
            isDirty = true;
            return this;
        }

        public StyleGroup SetWordWrap(bool wrap)
        {
            if (variantInfo.WordWrap == wrap) return this;

            variantInfo.WordWrap = wrap;
            isDirty = true;
            return this;
        }

        public StyleGroup SetBackgroundColor(Color color)
        {
            if (variantInfo.BackgroundColor == color) return this;

            variantInfo.BackgroundColor = color;
            isDirty = true;
            return this;
        }

        public StyleGroup SetBackground(Texture2D texture)
        {
            variantInfo.Background = texture;
            isDirty = true;
            return this;
        }

        public StyleGroup SetBackgroundHovered(Texture2D texture)
        {
            variantInfo.BackgroundHovered = texture;
            isDirty = true;
            return this;
        }

        public StyleGroup SetBackgroundActive(Texture2D texture)
        {
            variantInfo.BackgroundActive = texture;
            isDirty = true;
            return this;
        }

        public StyleGroup SetFontGroup(FontGroup fontGroup)
        {
            variantInfo.FontGroup = fontGroup;
            return this;
        }

        public static void ClearCache()
        {
            backgroundTextureLookup.Clear();
            instanceLookup.Clear();
        }

        //-----------------------------------------------------------------------------------------
        // Private Methods:
        //-----------------------------------------------------------------------------------------

        private GUIStyle GetOrCreateStyle(StyleGroupVariantInfo info)
        {
            if (isDirty)
            {
                generatedStyle = CreateGUIStyle(info);
                isDirty = false;
            }

            return generatedStyle;
        }

        private GUIStyle CreateGUIStyle(StyleGroupVariantInfo info)
        {
            GUIStyle newStyle = new GUIStyle(baseStyle);

            newStyle.alignment = info.Anchor;
            newStyle.normal.textColor = info.NormalColor;
            newStyle.onNormal.textColor = info.NormalColor;
            newStyle.hover.textColor = info.HoverColor;
            newStyle.onHover.textColor = info.HoverColor;
            newStyle.active.textColor = info.ActiveColor;
            newStyle.onActive.textColor = info.ActiveColor;
            newStyle.fontSize = info.FontSize;
            newStyle.font = GetFontFromFontGroupAndWeight(info.FontGroup, info.FontWeight);
            newStyle.margin = new RectOffset(info.Margin.x, info.Margin.y, info.Margin.z, info.Margin.w);
            newStyle.padding = new RectOffset(info.Padding.x, info.Padding.y, info.Padding.z, info.Padding.w);
            newStyle.wordWrap = variantInfo.WordWrap;
            newStyle.normal.background = variantInfo.Background;
            newStyle.hover.background = variantInfo.BackgroundHovered;
            newStyle.active.background = variantInfo.BackgroundActive;

            if (variantInfo.Background != null) return newStyle;

            if (!backgroundTextureLookup.ContainsKey(variantInfo.BackgroundColor))
            {
                Texture2D background = UI.Style.Textures.FlatTexture(variantInfo.BackgroundColor);
                backgroundTextureLookup.Add(variantInfo.BackgroundColor, background);
            }

            newStyle.normal.background = backgroundTextureLookup[variantInfo.BackgroundColor];

            return newStyle;
        }

        //-----------------------------------------------------------------------------------------
        // Static Methods:
        //-----------------------------------------------------------------------------------------

        private static StyleGroupVariantInfo VariantInfoFromGUIStyle(GUIStyle style)
        {
            StyleGroupVariantInfo newInfo = StyleGroupVariantInfo.Default;
            newInfo.Anchor = style.alignment;
            newInfo.FontSize = style.fontSize;
            newInfo.Margin = new Vector4Int(style.margin.top, style.margin.right, style.margin.bottom, style.margin.left);
            newInfo.Padding = new Vector4Int(style.padding.top, style.padding.right, style.padding.bottom, style.padding.left);
            newInfo.WordWrap = style.wordWrap;
            newInfo.Background = style.normal.background;
            newInfo.BackgroundHovered = style.hover.background;
            newInfo.BackgroundActive = style.active.background;
            newInfo.NormalColor = style.normal.textColor;
            newInfo.HoverColor = style.hover.textColor;
            newInfo.ActiveColor = style.active.textColor;
            return newInfo;
        }

        private static Font GetFontFromFontGroupAndWeight(FontGroup group, FontWeights weight)
        {
            switch (weight)
            {
                case FontWeights.Light:
                    return group.Light;
                case FontWeights.Normal:
                    return group.Standard;
                case FontWeights.Bold:
                    return group.Bold;
            }

            return null;
        }
    }
}