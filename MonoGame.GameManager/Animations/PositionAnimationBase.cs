using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Base class of the animations that move a control (its <see cref="IControl.PositionAnchor"/>).
    /// </summary>
    public abstract class PositionAnimationBase<TAnimation> : AnimationAbstract<TAnimation>
        where TAnimation : PositionAnimationBase<TAnimation>
    {
        private Vector2 positionStart;
        private bool isStartExplicit;

        /// <summary>
        /// Creates the animation. Unless <see cref="PositionStart"/> is set explicitly, the animation starts from
        /// the position of the control at the moment it is played for the first time.
        /// </summary>
        protected PositionAnimationBase(IControl control, float duration, Vector2 positionEnd)
            : base(control ?? throw new ArgumentNullException(nameof(control)), duration)
        {
            positionStart = control.PositionAnchor;
            PositionEnd = positionEnd;
        }

        /// <summary>The start position (relative to the anchor of the control).</summary>
        public Vector2 PositionStart
        {
            get => positionStart;
            set
            {
                positionStart = value;
                isStartExplicit = true;
            }
        }

        /// <summary>The end position (relative to the anchor of the control).</summary>
        public Vector2 PositionEnd { get; set; }

        public TAnimation SetPositionStart(Vector2 positionStart)
        {
            PositionStart = positionStart;
            return ThisAsT;
        }

        public TAnimation SetPositionEnd(Vector2 positionEnd)
        {
            PositionEnd = positionEnd;
            return ThisAsT;
        }

        /// <inheritdoc />
        protected override void CaptureStartValues()
        {
            if (!isStartExplicit)
                positionStart = Control.PositionAnchor;
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
            => Control.SetPosition(positionStart + (PositionEnd - positionStart) * progress);
    }
}
