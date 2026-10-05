using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Fades a control by multiplying its <see cref="Controls.Interfaces.IRenderable.Color"/> (behavior of version 1.x).
    /// </summary>
    /// <remarks>
    /// Because it overwrites the color every frame, it conflicts with other color changes such as
    /// <see cref="IControl.SetMouseEventsColor"/>. Prefer <see cref="OpacityAnimation"/>.
    /// </remarks>
    [Obsolete("FadeAnimation overwrites the control Color every frame. Use OpacityAnimation instead.")]
    public class FadeAnimation : AnimationAbstract<FadeAnimation>
    {
        private Color baseColor;

        public FadeAnimation(IControl control, float duration, float transparencyEnd)
            : base(control ?? throw new ArgumentNullException(nameof(control)), duration)
        {
            baseColor = control.Color.A == 0 ? Color.White : control.Color;
            SetTransparencyEnd(transparencyEnd);
            if (transparencyEnd == 0f)
                SetTransparencyStart(1f);
        }

        /// <summary>The alpha multiplier at the start (0 = transparent, 1 = opaque).</summary>
        public float TransparencyStart { get; set; }

        /// <summary>The alpha multiplier at the end (0 = transparent, 1 = opaque).</summary>
        public float TransparencyEnd { get; set; }

        public FadeAnimation SetTransparencyStart(float transparencyStart)
        {
            TransparencyStart = MathHelper.Clamp(transparencyStart, 0f, 1f);
            return this;
        }

        public FadeAnimation SetTransparencyEnd(float transparencyEnd)
        {
            TransparencyEnd = MathHelper.Clamp(transparencyEnd, 0f, 1f);
            return this;
        }

        public FadeAnimation SetBaseColor(Color baseColor)
        {
            this.baseColor = baseColor;
            return this;
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
        {
            var transparency = MathHelper.Clamp(TransparencyStart + (TransparencyEnd - TransparencyStart) * progress, 0f, 1f);
            Control.SetColor(baseColor * transparency);
        }
    }
}
