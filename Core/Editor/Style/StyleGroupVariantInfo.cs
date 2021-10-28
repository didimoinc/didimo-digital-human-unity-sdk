using UnityEngine;

namespace DigitalSalmon.UI
{
    public enum FontWeights
    {
        Light,
        Normal,
        Bold
    }

    public struct StyleGroupVariantInfo
    {
        public FontGroup  FontGroup;
        public TextAnchor Anchor;

        public Color NormalColor;
        public Color HoverColor;
        public Color ActiveColor;

        public Color OnNormalColor;
        public Color OnHoverColor;
        public Color OnActiveColor;

        public int         FontSize;
        public FontWeights FontWeight;
        public Vector4Int  Margin;
        public Vector4Int  Padding;
        public bool        WordWrap;

        public Color     BackgroundColor;
        public Texture2D Background;
        public Texture2D BackgroundHovered;
        public Texture2D BackgroundActive;

        public static StyleGroupVariantInfo Default = WithFixedColor(new StyleGroupVariantInfo
            {
                FontGroup = FontGroup.Default,
                Anchor = TextAnchor.UpperLeft,
                FontSize = 12,
                FontWeight = FontWeights.Normal,
                Margin = Vector4Int.Zero,
                Padding = Vector4Int.Zero,
                WordWrap = true,
                Background = null,
                BackgroundHovered = null,
                BackgroundActive = null,
                BackgroundColor = Color.clear
            },
            Color.white);

        public static StyleGroupVariantInfo WithFixedColor(StyleGroupVariantInfo self, Color color)
        {
            self.NormalColor = color;
            self.HoverColor = color;
            self.ActiveColor = color;
            self.OnNormalColor = color;
            self.OnHoverColor = color;
            self.OnActiveColor = color;
            return self;
        }

        public bool Equals(StyleGroupVariantInfo other) => Anchor == other.Anchor && NormalColor == other.NormalColor && FontSize == other.FontSize && FontWeight == other.FontWeight && Margin.Equals(other.Margin) && Padding.Equals(other.Padding) && WordWrap == other.WordWrap && BackgroundColor.Equals(other.BackgroundColor) && Equals(Background, other.Background) && Equals(BackgroundHovered, other.BackgroundHovered);

        public bool HasFixedColor() => NormalColor == HoverColor && NormalColor == ActiveColor && NormalColor == HoverColor && NormalColor == OnNormalColor && NormalColor == OnHoverColor && NormalColor == OnActiveColor;
    }
}