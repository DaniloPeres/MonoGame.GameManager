using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Changes the <see cref="IControl.Color"/> of a control to <see cref="ColorEnd"/>. Unless
    /// <see cref="ColorStart"/> is set explicitly, it starts from the color of the control when it is played.
    /// </summary>
    public class ColorAnimation : AnimationAbstract<ColorAnimation>
    {
        private Color colorStart;
        private bool isStartExplicit;

        public ColorAnimation(IControl control, float duration, Color colorEnd)
            : base(control ?? throw new ArgumentNullException(nameof(control)), duration)
        {
            colorStart = control.Color;
            ColorEnd = colorEnd;
        }

        public Color ColorStart
        {
            get => colorStart;
            set
            {
                colorStart = value;
                isStartExplicit = true;
            }
        }

        public Color ColorEnd { get; set; }

        public ColorAnimation SetColorStart(Color colorStart)
        {
            ColorStart = colorStart;
            return this;
        }

        public ColorAnimation SetColorEnd(Color colorEnd)
        {
            ColorEnd = colorEnd;
            return this;
        }

        /// <inheritdoc />
        protected override void CaptureStartValues()
        {
            if (!isStartExplicit)
                colorStart = Control.Color;
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
            => Control.Color = Color.Lerp(colorStart, ColorEnd, progress);
    }
}
