using Microsoft.Xna.Framework;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// A color that changes along the life of a particle: color stops (time from 0 = born to 1 = dead, color) with a
    /// linear interpolation between them. Used by <see cref="ParticleSettings.ColorOverLifetime"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// settings.ColorOverLifetime = ParticleGradient.FromColors(Color.White, Color.Yellow, Color.OrangeRed, Color.Transparent);
    /// settings.ColorOverLifetime = new ParticleGradient().AddStop(0f, Color.White).AddStop(0.8f, Color.Red).AddStop(1f, Color.Transparent);
    /// </code>
    /// </example>
    public class ParticleGradient
    {
        private float[] times = new float[4];
        private Color[] colors = new Color[4];
        private int count;

        /// <summary>The number of color stops.</summary>
        public int StopCount => count;

        /// <summary>The time (from 0 to 1) of a color stop.</summary>
        public float GetStopTime(int index) => times[CheckIndex(index)];

        /// <summary>The color of a color stop.</summary>
        public Color GetStopColor(int index) => colors[CheckIndex(index)];

        /// <summary>Adds a color stop. The stops are kept sorted by time.</summary>
        /// <param name="time">The life progress, from 0 (born) to 1 (dead); clamped to that range.</param>
        /// <param name="color">The color at that time.</param>
        public ParticleGradient AddStop(float time, Color color)
        {
            time = MathUtils.Clamp01(time);
            if (count == times.Length)
            {
                Array.Resize(ref times, count * 2);
                Array.Resize(ref colors, count * 2);
            }

            var index = count;
            while (index > 0 && times[index - 1] > time)
            {
                times[index] = times[index - 1];
                colors[index] = colors[index - 1];
                index--;
            }

            times[index] = time;
            colors[index] = color;
            count++;
            return this;
        }

        /// <summary>
        /// The color at a time from 0 to 1. Before the first stop the first color is returned, after the last stop
        /// the last color. A gradient without stops returns white.
        /// </summary>
        public Color Evaluate(float time)
        {
            if (count == 0)
                return Color.White;
            if (time <= times[0])
                return colors[0];
            if (time >= times[count - 1])
                return colors[count - 1];

            for (var i = 1; i < count; i++)
            {
                if (time > times[i])
                    continue;

                var span = times[i] - times[i - 1];
                var amount = span > 0f ? (time - times[i - 1]) / span : 1f;
                return Color.Lerp(colors[i - 1], colors[i], amount);
            }

            return colors[count - 1];
        }

        /// <summary>A gradient with the colors evenly spaced along the life.</summary>
        public static ParticleGradient FromColors(params Color[] colors)
        {
            if (colors == null || colors.Length == 0)
                throw new ArgumentException("At least one color is needed.", nameof(colors));

            var gradient = new ParticleGradient();
            for (var i = 0; i < colors.Length; i++)
                gradient.AddStop(colors.Length == 1 ? 0f : i / (float)(colors.Length - 1), colors[i]);
            return gradient;
        }

        /// <summary>A gradient from a color to transparent.</summary>
        public static ParticleGradient Fade(Color color) => FromColors(color, Color.Transparent);

        private int CheckIndex(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return index;
        }
    }
}
