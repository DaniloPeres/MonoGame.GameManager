using FontStashSharp;
using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Text
{
    /// <summary>
    /// Keeps scaled text sharp: instead of stretching the glyphs of the font, the text is drawn with the same font
    /// rasterized at the final size (<see cref="FontSystem.GetFont(float)"/> caches one font per size).
    /// </summary>
    /// <remarks>
    /// The layout does not change: the controls are measured with their own font and only the draw uses the
    /// rasterized font, with the scale and the origin converted to give the same result.
    /// </remarks>
    public static class FontScaling
    {
        /// <summary>When false, scaled text stretches the glyphs of its font (blurry when enlarged).</summary>
        public static bool CrispScaling { get; set; } = true;

        /// <summary>The largest font size rasterized for scaled text (bigger scales stretch this size).</summary>
        public static float MaxFontSize { get; set; } = 512f;

        /// <summary>
        /// Returns the font to draw <paramref name="font"/> with at <paramref name="scale"/>, and converts the scale
        /// and the origin (in font pixels) for that font.
        /// </summary>
        public static SpriteFontBase Resolve(SpriteFontBase font, ref Vector2 scale, ref Vector2 origin)
        {
            var drawFont = Resolve(font, ref scale, out var originFactor);
            origin *= originFactor;
            return drawFont;
        }

        /// <summary>
        /// Returns the font to draw <paramref name="font"/> with at <paramref name="scale"/> and converts the scale
        /// for that font. Multiply the origins (in font pixels) by <paramref name="originFactor"/>.
        /// </summary>
        public static SpriteFontBase Resolve(SpriteFontBase font, ref Vector2 scale, out float originFactor)
        {
            originFactor = 1f;
            if (!CrispScaling || font?.FontSystem == null || font.FontSize <= 0f)
                return font;

            var uniformScale = Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y));
            var size = (float)Math.Round(font.FontSize * uniformScale);
            size = Math.Max(1f, Math.Min(size, Math.Max(font.FontSize, MaxFontSize)));
            if (size == font.FontSize)
                return font;

            var scaledFont = font.FontSystem.GetFont(size);
            originFactor = scaledFont.FontSize / font.FontSize;
            scale /= originFactor;
            return scaledFont;
        }
    }
}
