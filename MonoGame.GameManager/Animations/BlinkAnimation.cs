using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Makes a control blink by switching its <see cref="IControl.Opacity"/> between
    /// <see cref="HiddenOpacity"/> and <see cref="VisibleOpacity"/>. The control is visible at the end.
    /// </summary>
    /// <example>
    /// <code>
    /// new BlinkAnimation(winnerLabel, 0.6f, blinkCount: 3).Play();
    /// </code>
    /// </example>
    public class BlinkAnimation : AnimationAbstract<BlinkAnimation>
    {
        /// <param name="control">The control to blink.</param>
        /// <param name="duration">The total duration of the blinks, in seconds.</param>
        /// <param name="blinkCount">How many times the control disappears.</param>
        public BlinkAnimation(IControl control, float duration, int blinkCount = 3)
            : base(control ?? throw new ArgumentNullException(nameof(control)), duration)
        {
            BlinkCount = Math.Max(1, blinkCount);
        }

        /// <summary>How many times the control disappears during the animation.</summary>
        public int BlinkCount { get; set; }

        /// <summary>The opacity used while the control is "off". Defaults to 0.</summary>
        public float HiddenOpacity { get; set; }

        /// <summary>The opacity used while the control is "on". Defaults to 1.</summary>
        public float VisibleOpacity { get; set; } = 1f;

        public BlinkAnimation SetBlinkCount(int blinkCount)
        {
            BlinkCount = Math.Max(1, blinkCount);
            return this;
        }

        public BlinkAnimation SetOpacities(float hiddenOpacity, float visibleOpacity)
        {
            HiddenOpacity = hiddenOpacity;
            VisibleOpacity = visibleOpacity;
            return this;
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
        {
            if (progress <= 0f || progress >= 1f)
            {
                Control.Opacity = progress <= 0f ? HiddenOpacity : VisibleOpacity;
                return;
            }

            var phase = (int)(progress * BlinkCount * 2);
            Control.Opacity = phase % 2 == 0 ? HiddenOpacity : VisibleOpacity;
        }
    }
}
