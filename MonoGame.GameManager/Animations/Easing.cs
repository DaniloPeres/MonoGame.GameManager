using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Maps a linear progress <c>t</c> (from 0 to 1) to an eased progress.
    /// Most functions return 0 for t = 0 and 1 for t = 1; Back and Elastic overshoot in between.
    /// </summary>
    /// <param name="t">The linear progress, from 0 to 1.</param>
    /// <returns>The eased progress.</returns>
    public delegate float EasingFunction(float t);

    /// <summary>
    /// The standard easing curves, usable by name from data (for example a JSON file).
    /// </summary>
    public enum EasingType
    {
        Linear,
        QuadIn, QuadOut, QuadInOut,
        CubicIn, CubicOut, CubicInOut,
        QuartIn, QuartOut, QuartInOut,
        QuintIn, QuintOut, QuintInOut,
        SineIn, SineOut, SineInOut,
        ExpoIn, ExpoOut, ExpoInOut,
        CircIn, CircOut, CircInOut,
        BackIn, BackOut, BackInOut,
        ElasticIn, ElasticOut, ElasticInOut,
        BounceIn, BounceOut, BounceInOut
    }

    /// <summary>
    /// Standard easing functions (Robert Penner's equations) used by every animation through
    /// <c>SetEasing</c>. Each function is a <see cref="EasingFunction"/>, so custom curves can be plugged in too
    /// (Strategy pattern).
    /// </summary>
    /// <example>
    /// <code>
    /// new MoveAnimation(button, 0.5f, new Vector2(100, 0)).SetEasing(Easing.BackOut).Play();
    /// </code>
    /// </example>
    public static class Easing
    {
        private const float BackOvershoot = 1.70158f;
        private const float BackOvershootInOut = BackOvershoot * 1.525f;
        private const float ElasticPeriod = (float)(2 * Math.PI / 3);
        private const float ElasticPeriodInOut = (float)(2 * Math.PI / 4.5);

        /// <summary>No easing: the progress is linear.</summary>
        public static readonly EasingFunction Linear = t => t;

        public static readonly EasingFunction QuadIn = t => t * t;
        public static readonly EasingFunction QuadOut = t => t * (2f - t);
        public static readonly EasingFunction QuadInOut = t => t < 0.5f ? 2f * t * t : 1f - Pow(-2f * t + 2f, 2) / 2f;

        public static readonly EasingFunction CubicIn = t => t * t * t;
        public static readonly EasingFunction CubicOut = t => 1f - Pow(1f - t, 3);
        public static readonly EasingFunction CubicInOut = t => t < 0.5f ? 4f * t * t * t : 1f - Pow(-2f * t + 2f, 3) / 2f;

        public static readonly EasingFunction QuartIn = t => t * t * t * t;
        public static readonly EasingFunction QuartOut = t => 1f - Pow(1f - t, 4);
        public static readonly EasingFunction QuartInOut = t => t < 0.5f ? 8f * t * t * t * t : 1f - Pow(-2f * t + 2f, 4) / 2f;

        public static readonly EasingFunction QuintIn = t => t * t * t * t * t;
        public static readonly EasingFunction QuintOut = t => 1f - Pow(1f - t, 5);
        public static readonly EasingFunction QuintInOut = t => t < 0.5f ? 16f * t * t * t * t * t : 1f - Pow(-2f * t + 2f, 5) / 2f;

        public static readonly EasingFunction SineIn = t => 1f - (float)Math.Cos(t * Math.PI / 2);
        public static readonly EasingFunction SineOut = t => (float)Math.Sin(t * Math.PI / 2);
        public static readonly EasingFunction SineInOut = t => -((float)Math.Cos(Math.PI * t) - 1f) / 2f;

        public static readonly EasingFunction ExpoIn = t => t <= 0f ? 0f : Pow(2f, 10f * t - 10f);
        public static readonly EasingFunction ExpoOut = t => t >= 1f ? 1f : 1f - Pow(2f, -10f * t);
        public static readonly EasingFunction ExpoInOut = t =>
            t <= 0f ? 0f
            : t >= 1f ? 1f
            : t < 0.5f ? Pow(2f, 20f * t - 10f) / 2f
            : (2f - Pow(2f, -20f * t + 10f)) / 2f;

        public static readonly EasingFunction CircIn = t => 1f - (float)Math.Sqrt(Math.Max(0f, 1f - t * t));
        public static readonly EasingFunction CircOut = t => (float)Math.Sqrt(Math.Max(0f, 1f - Pow(t - 1f, 2)));
        public static readonly EasingFunction CircInOut = t => t < 0.5f
            ? (1f - (float)Math.Sqrt(Math.Max(0f, 1f - Pow(2f * t, 2)))) / 2f
            : ((float)Math.Sqrt(Math.Max(0f, 1f - Pow(-2f * t + 2f, 2))) + 1f) / 2f;

        public static readonly EasingFunction BackIn = t => (BackOvershoot + 1f) * t * t * t - BackOvershoot * t * t;
        public static readonly EasingFunction BackOut = t => 1f + (BackOvershoot + 1f) * Pow(t - 1f, 3) + BackOvershoot * Pow(t - 1f, 2);
        public static readonly EasingFunction BackInOut = t => t < 0.5f
            ? Pow(2f * t, 2) * ((BackOvershootInOut + 1f) * 2f * t - BackOvershootInOut) / 2f
            : (Pow(2f * t - 2f, 2) * ((BackOvershootInOut + 1f) * (t * 2f - 2f) + BackOvershootInOut) + 2f) / 2f;

        public static readonly EasingFunction ElasticIn = t =>
            t <= 0f ? 0f
            : t >= 1f ? 1f
            : -Pow(2f, 10f * t - 10f) * (float)Math.Sin((t * 10f - 10.75f) * ElasticPeriod);
        public static readonly EasingFunction ElasticOut = t =>
            t <= 0f ? 0f
            : t >= 1f ? 1f
            : Pow(2f, -10f * t) * (float)Math.Sin((t * 10f - 0.75f) * ElasticPeriod) + 1f;
        public static readonly EasingFunction ElasticInOut = t =>
            t <= 0f ? 0f
            : t >= 1f ? 1f
            : t < 0.5f ? -(Pow(2f, 20f * t - 10f) * (float)Math.Sin((20f * t - 11.125f) * ElasticPeriodInOut)) / 2f
            : Pow(2f, -20f * t + 10f) * (float)Math.Sin((20f * t - 11.125f) * ElasticPeriodInOut) / 2f + 1f;

        public static readonly EasingFunction BounceOut = BounceOutFunction;
        public static readonly EasingFunction BounceIn = t => 1f - BounceOutFunction(1f - t);
        public static readonly EasingFunction BounceInOut = t => t < 0.5f
            ? (1f - BounceOutFunction(1f - 2f * t)) / 2f
            : (1f + BounceOutFunction(2f * t - 1f)) / 2f;

        /// <summary>
        /// Returns the easing function of the given type.
        /// </summary>
        public static EasingFunction Get(EasingType type)
        {
            switch (type)
            {
                case EasingType.QuadIn: return QuadIn;
                case EasingType.QuadOut: return QuadOut;
                case EasingType.QuadInOut: return QuadInOut;
                case EasingType.CubicIn: return CubicIn;
                case EasingType.CubicOut: return CubicOut;
                case EasingType.CubicInOut: return CubicInOut;
                case EasingType.QuartIn: return QuartIn;
                case EasingType.QuartOut: return QuartOut;
                case EasingType.QuartInOut: return QuartInOut;
                case EasingType.QuintIn: return QuintIn;
                case EasingType.QuintOut: return QuintOut;
                case EasingType.QuintInOut: return QuintInOut;
                case EasingType.SineIn: return SineIn;
                case EasingType.SineOut: return SineOut;
                case EasingType.SineInOut: return SineInOut;
                case EasingType.ExpoIn: return ExpoIn;
                case EasingType.ExpoOut: return ExpoOut;
                case EasingType.ExpoInOut: return ExpoInOut;
                case EasingType.CircIn: return CircIn;
                case EasingType.CircOut: return CircOut;
                case EasingType.CircInOut: return CircInOut;
                case EasingType.BackIn: return BackIn;
                case EasingType.BackOut: return BackOut;
                case EasingType.BackInOut: return BackInOut;
                case EasingType.ElasticIn: return ElasticIn;
                case EasingType.ElasticOut: return ElasticOut;
                case EasingType.ElasticInOut: return ElasticInOut;
                case EasingType.BounceIn: return BounceIn;
                case EasingType.BounceOut: return BounceOut;
                case EasingType.BounceInOut: return BounceInOut;
                default: return Linear;
            }
        }

        /// <summary>
        /// Mirrors an easing function: an "In" curve becomes the matching "Out" curve and vice versa.
        /// </summary>
        public static EasingFunction Reverse(EasingFunction easing)
        {
            if (easing == null)
                throw new ArgumentNullException(nameof(easing));
            return t => 1f - easing(1f - t);
        }

        /// <summary>
        /// Builds an "InOut" curve from an "In" curve: the first half uses the curve, the second half its mirror.
        /// </summary>
        public static EasingFunction InOut(EasingFunction easeIn)
        {
            if (easeIn == null)
                throw new ArgumentNullException(nameof(easeIn));
            return t => t < 0.5f ? easeIn(2f * t) / 2f : 1f - easeIn(2f - 2f * t) / 2f;
        }

        private static float Pow(float value, float power) => (float)Math.Pow(value, power);

        private static float BounceOutFunction(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
                return n1 * t * t;
            if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }
            if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }

            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }
}
