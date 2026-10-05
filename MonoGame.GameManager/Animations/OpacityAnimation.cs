using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Fades a control by animating its <see cref="IControl.Opacity"/>. The opacity is applied to the control
    /// and all its children, and it does not change the <see cref="IControl.Color"/> of the control.
    /// </summary>
    /// <example>
    /// <code>
    /// // Fade out and remove the control at the end
    /// new OpacityAnimation(label, 0.3f, 0f)
    ///     .SetShouldRemoveControlOnAnimationEnd(true)
    ///     .Play();
    /// </code>
    /// </example>
    public class OpacityAnimation : AnimationAbstract<OpacityAnimation>
    {
        private float opacityStart;
        private bool isStartExplicit;

        /// <summary>
        /// Creates the animation. Unless <see cref="OpacityStart"/> is set explicitly, the animation starts from the
        /// current opacity of the control, or from the opposite of <paramref name="opacityEnd"/> when the control
        /// already has that opacity (so a visible control fades in from 0 when the end is 1).
        /// </summary>
        public OpacityAnimation(IControl control, float duration, float opacityEnd)
            : base(control ?? throw new ArgumentNullException(nameof(control)), duration)
        {
            OpacityEnd = MathHelper.Clamp(opacityEnd, 0f, 1f);
            opacityStart = GetDefaultStart();
        }

        /// <summary>The opacity at the start of the animation, from 0 to 1.</summary>
        public float OpacityStart
        {
            get => opacityStart;
            set
            {
                opacityStart = MathHelper.Clamp(value, 0f, 1f);
                isStartExplicit = true;
            }
        }

        /// <summary>The opacity at the end of the animation, from 0 to 1.</summary>
        public float OpacityEnd { get; set; }

        public OpacityAnimation SetOpacityStart(float opacityStart)
        {
            OpacityStart = opacityStart;
            return this;
        }

        public OpacityAnimation SetOpacityEnd(float opacityEnd)
        {
            OpacityEnd = MathHelper.Clamp(opacityEnd, 0f, 1f);
            return this;
        }

        /// <inheritdoc />
        protected override void CaptureStartValues()
        {
            if (!isStartExplicit)
                opacityStart = GetDefaultStart();
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
            => Control.Opacity = MathHelper.Clamp(opacityStart + (OpacityEnd - opacityStart) * progress, 0f, 1f);

        private float GetDefaultStart()
        {
            var current = Control.Opacity;
            return Math.Abs(current - OpacityEnd) < 0.0001f ? 1f - OpacityEnd : current;
        }
    }
}
