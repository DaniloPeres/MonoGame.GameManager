using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Computes a value between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The start value.</param>
    /// <param name="to">The end value.</param>
    /// <param name="amount">The (eased) progress, usually from 0 to 1. Overshooting curves can go beyond.</param>
    public delegate T Interpolator<T>(T from, T to, float amount);

    /// <summary>
    /// Ready-made <see cref="Interpolator{T}"/> functions for the common MonoGame types.
    /// </summary>
    public static class Interpolators
    {
        public static readonly Interpolator<float> Float = (from, to, amount) => from + (to - from) * amount;

        public static readonly Interpolator<double> Double = (from, to, amount) => from + (to - from) * amount;

        public static readonly Interpolator<int> Int = (from, to, amount) => (int)Math.Round(from + (to - from) * (double)amount);

        public static readonly Interpolator<Microsoft.Xna.Framework.Vector2> Vector2 = (from, to, amount) => from + (to - from) * amount;

        public static readonly Interpolator<Microsoft.Xna.Framework.Point> Point = (from, to, amount) => new Microsoft.Xna.Framework.Point(
            (int)Math.Round(from.X + (to.X - from.X) * (double)amount),
            (int)Math.Round(from.Y + (to.Y - from.Y) * (double)amount));

        public static readonly Interpolator<Microsoft.Xna.Framework.Color> Color = Microsoft.Xna.Framework.Color.Lerp;

        public static readonly Interpolator<Microsoft.Xna.Framework.Rectangle> Rectangle = (from, to, amount) => new Microsoft.Xna.Framework.Rectangle(
            (int)Math.Round(from.X + (to.X - from.X) * (double)amount),
            (int)Math.Round(from.Y + (to.Y - from.Y) * (double)amount),
            (int)Math.Round(from.Width + (to.Width - from.Width) * (double)amount),
            (int)Math.Round(from.Height + (to.Height - from.Height) * (double)amount));

        /// <summary>Interpolates angles in radians through the shortest path.</summary>
        public static readonly Interpolator<float> Angle = (from, to, amount) => from + MathHelper.WrapAngle(to - from) * amount;
    }
}
