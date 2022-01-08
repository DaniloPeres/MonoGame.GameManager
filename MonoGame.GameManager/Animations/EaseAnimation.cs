using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Animations
{
    public class EaseAnimation : AnimationAbstract<EaseAnimation>
    {
        public Vector2 PositionStart { get; set; }
        public Vector2 PositionEnd { get ; set; }

        public EaseAnimation(IControl control, float duration, Vector2 positionEnd)
            : base(control, duration)
        {
            SetPositionStart(control.PositionAnchor);
            SetPositionEnd(positionEnd);
        }

        public EaseAnimation SetPositionStart(Vector2 positionStart)
        {
            PositionStart = positionStart;
            return this;
        }

        public EaseAnimation SetPositionEnd(Vector2 positionEnd)
        {
            PositionEnd = positionEnd;
            return this;
        }

        protected override void OnUpdateAnimation(float durationRate)
        {
            var start = PositionStart;
            var end = PositionEnd;
            if (IsReverse)
            {
                start = PositionEnd;
                end = PositionStart;
            }

            var position = start + (end - start) * durationRate;
            Control.SetPosition(position);
        }
    }
}
