using FontStashSharp;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.GameMath;

namespace MonoGame.GameManager.Text
{
    /// <summary>Measures where the glyphs of a text are actually drawn.</summary>
    internal static class TextInk
    {
        /// <summary>
        /// The box of the pixels drawn by a line of text at a position (font units): the union of the box of each glyph,
        /// which includes the swashes and overhangs that go past the advance of the last character.
        /// </summary>
        public static RectangleF GetBounds(SpriteFontBase font, string text, Vector2 position, float characterSpacing)
        {
            var measured = font.TextBounds(text, position, null, characterSpacing);
            var left = measured.X;
            var top = measured.Y;
            var right = measured.X2;
            var bottom = measured.Y2;
            foreach (var glyph in font.GetGlyphs(text, position, Vector2.Zero, null, characterSpacing))
            {
                var box = glyph.Bounds;
                if (box.Width <= 0 || box.Height <= 0)
                    continue;
                left = System.Math.Min(left, box.X);
                top = System.Math.Min(top, box.Y);
                right = System.Math.Max(right, box.X + box.Width);
                bottom = System.Math.Max(bottom, box.Y + box.Height);
            }

            return new RectangleF(left, top, right - left, bottom - top);
        }
    }
}
