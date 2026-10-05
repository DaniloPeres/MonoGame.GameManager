using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Animates any value of type <typeparamref name="T"/> through a setter callback, so any property of any
    /// object can be animated with easing, delays, repetitions and callbacks.
    /// </summary>
    /// <example>
    /// <code>
    /// // Animate a score counter
    /// Tween.Int(value => scoreLabel.SetText(value.ToString()), 0, 1500, 1f)
    ///     .SetEasing(Easing.QuadOut)
    ///     .Play();
    /// </code>
    /// </example>
    public class Tween<T> : AnimationAbstract<Tween<T>>
    {
        private readonly Action<T> apply;
        private readonly Interpolator<T> interpolator;

        /// <param name="apply">Receives the animated value on every update.</param>
        /// <param name="from">The start value.</param>
        /// <param name="to">The end value.</param>
        /// <param name="duration">The duration of one cycle, in seconds.</param>
        /// <param name="interpolator">Computes intermediate values (see <see cref="Interpolators"/>).</param>
        /// <param name="control">Optional control: the tween stops automatically when it is disposed.</param>
        public Tween(Action<T> apply, T from, T to, float duration, Interpolator<T> interpolator, IControl control = null)
            : base(control, duration)
        {
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
            this.interpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));
            From = from;
            To = to;
            Value = from;
        }

        public T From { get; set; }

        public T To { get; set; }

        /// <summary>The last value computed by the tween.</summary>
        public T Value { get; private set; }

        public Tween<T> SetFrom(T from)
        {
            From = from;
            return this;
        }

        public Tween<T> SetTo(T to)
        {
            To = to;
            return this;
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
        {
            Value = interpolator(From, To, progress);
            apply(Value);
        }
    }

    /// <summary>
    /// Factory methods for <see cref="Tween{T}"/>.
    /// </summary>
    public static class Tween
    {
        public static Tween<float> Float(Action<float> apply, float from, float to, float duration, IControl control = null)
            => new Tween<float>(apply, from, to, duration, Interpolators.Float, control);

        public static Tween<int> Int(Action<int> apply, int from, int to, float duration, IControl control = null)
            => new Tween<int>(apply, from, to, duration, Interpolators.Int, control);

        public static Tween<Vector2> Vector2(Action<Vector2> apply, Vector2 from, Vector2 to, float duration, IControl control = null)
            => new Tween<Vector2>(apply, from, to, duration, Interpolators.Vector2, control);

        public static Tween<Color> Color(Action<Color> apply, Color from, Color to, float duration, IControl control = null)
            => new Tween<Color>(apply, from, to, duration, Interpolators.Color, control);

        /// <summary>Animates an angle in radians through the shortest path.</summary>
        public static Tween<float> Angle(Action<float> apply, float from, float to, float duration, IControl control = null)
            => new Tween<float>(apply, from, to, duration, Interpolators.Angle, control);
    }
}
