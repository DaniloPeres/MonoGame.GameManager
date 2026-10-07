using MonoGame.GameManager.Animations;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// A value that changes along the life of a particle: keyframes (time from 0 = born to 1 = dead, value) with a
    /// linear interpolation between them. Used by <see cref="ParticleSettings.ScaleOverLifetime"/> and
    /// <see cref="ParticleSettings.AlphaOverLifetime"/>. Evaluating a curve does not create garbage.
    /// </summary>
    /// <example>
    /// <code>
    /// settings.ScaleOverLifetime = ParticleCurve.Peak(0.2f, 1.5f, start: 0.2f, end: 0f);
    /// settings.AlphaOverLifetime = ParticleCurve.FadeInOut(0.1f, 0.4f);
    /// settings.ScaleOverLifetime = new ParticleCurve().AddKey(0f, 0f).AddKey(0.3f, 1f).AddKey(1f, 0.5f);
    /// </code>
    /// </example>
    public class ParticleCurve
    {
        private float[] times = new float[4];
        private float[] values = new float[4];
        private int count;

        /// <summary>The number of keyframes.</summary>
        public int KeyCount => count;

        /// <summary>The time (from 0 to 1) of a keyframe.</summary>
        public float GetKeyTime(int index) => times[CheckIndex(index)];

        /// <summary>The value of a keyframe.</summary>
        public float GetKeyValue(int index) => values[CheckIndex(index)];

        /// <summary>Adds a keyframe. The keyframes are kept sorted by time.</summary>
        /// <param name="time">The life progress, from 0 (born) to 1 (dead); clamped to that range.</param>
        /// <param name="value">The value at that time.</param>
        public ParticleCurve AddKey(float time, float value)
        {
            time = MathUtils.Clamp01(time);
            if (count == times.Length)
            {
                Array.Resize(ref times, count * 2);
                Array.Resize(ref values, count * 2);
            }

            var index = count;
            while (index > 0 && times[index - 1] > time)
            {
                times[index] = times[index - 1];
                values[index] = values[index - 1];
                index--;
            }

            times[index] = time;
            values[index] = value;
            count++;
            return this;
        }

        /// <summary>
        /// The value at a time from 0 to 1. Before the first keyframe the value of the first one is returned, after
        /// the last keyframe the value of the last one. A curve without keyframes returns 1.
        /// </summary>
        public float Evaluate(float time)
        {
            if (count == 0)
                return 1f;
            if (time <= times[0])
                return values[0];
            if (time >= times[count - 1])
                return values[count - 1];

            for (var i = 1; i < count; i++)
            {
                if (time > times[i])
                    continue;

                var span = times[i] - times[i - 1];
                var amount = span > 0f ? (time - times[i - 1]) / span : 1f;
                return MathUtils.Lerp(values[i - 1], values[i], amount);
            }

            return values[count - 1];
        }

        /// <summary>A curve that always returns the same value.</summary>
        public static ParticleCurve Constant(float value) => new ParticleCurve().AddKey(0f, value);

        /// <summary>A straight line from a value when the particle is born to another when it dies.</summary>
        public static ParticleCurve Linear(float from, float to) => new ParticleCurve().AddKey(0f, from).AddKey(1f, to);

        /// <summary>
        /// A curve that goes from 0 to <paramref name="peak"/> during the first <paramref name="fadeIn"/> of the life,
        /// stays there, and goes back to 0 during the last <paramref name="fadeOut"/> (both as fractions of the life).
        /// </summary>
        public static ParticleCurve FadeInOut(float fadeIn, float fadeOut, float peak = 1f)
        {
            fadeIn = MathUtils.Clamp01(fadeIn);
            fadeOut = MathUtils.Clamp01(fadeOut);
            var curve = new ParticleCurve();
            if (fadeIn > 0f)
                curve.AddKey(0f, 0f);
            curve.AddKey(fadeIn, peak);
            curve.AddKey(Math.Max(fadeIn, 1f - fadeOut), peak);
            if (fadeOut > 0f)
                curve.AddKey(1f, 0f);
            return curve;
        }

        /// <summary>
        /// A curve that starts at <paramref name="start"/>, reaches <paramref name="peak"/> at <paramref name="time"/>
        /// (from 0 to 1) and ends at <paramref name="end"/>.
        /// </summary>
        public static ParticleCurve Peak(float time, float peak, float start = 0f, float end = 0f)
            => new ParticleCurve().AddKey(0f, start).AddKey(time, peak).AddKey(1f, end);

        /// <summary>
        /// Bakes an easing function (eg: <see cref="Easing.BackOut"/>) into a curve going from <paramref name="from"/>
        /// to <paramref name="to"/>, so the function is not called for every particle.
        /// </summary>
        public static ParticleCurve FromEasing(EasingFunction easing, float from, float to, int samples = 16)
        {
            if (easing == null)
                throw new ArgumentNullException(nameof(easing));

            samples = Math.Max(2, samples);
            var curve = new ParticleCurve();
            for (var i = 0; i < samples; i++)
            {
                var time = i / (float)(samples - 1);
                curve.AddKey(time, MathUtils.Lerp(from, to, easing(time)));
            }

            return curve;
        }

        /// <summary>
        /// A curve with <paramref name="count"/> pulses from 0 to 1 and back (fireflies, twinkling stars); each pulse
        /// lasts <paramref name="duty"/> of its period.
        /// </summary>
        public static ParticleCurve Blink(int count, float duty = 0.5f)
        {
            count = Math.Max(1, count);
            duty = MathUtils.Clamp(duty, 0.05f, 1f);
            var curve = new ParticleCurve();
            var period = 1f / count;
            for (var i = 0; i < count; i++)
            {
                var start = i * period;
                var length = period * duty;
                curve.AddKey(start, 0f).AddKey(start + length / 2f, 1f).AddKey(start + length, 0f);
            }

            return curve;
        }

        private int CheckIndex(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return index;
        }
    }
}
