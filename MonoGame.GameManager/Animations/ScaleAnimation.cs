using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Scales a control to <see cref="ScaleEnd"/>. Unless <see cref="ScaleStart"/> is set explicitly, it starts
    /// from the scale of the control when it is played.
    /// </summary>
    public class ScaleAnimation : AnimationAbstract<ScaleAnimation>
    {
        private readonly IScalableControl scalableControl;
        private Vector2 scaleStart;
        private bool isStartExplicit;

        public ScaleAnimation(IScalableControl control, float duration, Vector2 scaleEnd)
            : base(control ?? throw new ArgumentNullException(nameof(control)), duration)
        {
            scalableControl = control;
            scaleStart = control.Scale;
            ScaleEnd = scaleEnd;
        }

        public ScaleAnimation(IScalableControl control, float duration, float scaleEnd)
            : this(control, duration, new Vector2(scaleEnd)) { }

        public Vector2 ScaleStart
        {
            get => scaleStart;
            set
            {
                scaleStart = value;
                isStartExplicit = true;
            }
        }

        public Vector2 ScaleEnd { get; set; }

        public ScaleAnimation SetScaleStart(Vector2 scaleStart)
        {
            ScaleStart = scaleStart;
            return this;
        }

        public ScaleAnimation SetScaleEnd(Vector2 scaleEnd)
        {
            ScaleEnd = scaleEnd;
            return this;
        }

        /// <inheritdoc />
        protected override void CaptureStartValues()
        {
            if (!isStartExplicit)
                scaleStart = scalableControl.Scale;
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
            => scalableControl.SetScale(scaleStart + (ScaleEnd - scaleStart) * progress);
    }
}
