using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Extensions
{
    /// <summary>
    /// Helpers for <see cref="Color"/>.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Multiplies two colors channel by channel, like a tint (white leaves the color unchanged, transparent makes it
        /// transparent). Works with premultiplied colors.
        /// </summary>
        public static Color Multiply(this Color color, Color tint)
        {
            if (tint.PackedValue == uint.MaxValue)
                return color;
            return new Color(color.R * tint.R / 255, color.G * tint.G / 255, color.B * tint.B / 255, color.A * tint.A / 255);
        }
    }
}
