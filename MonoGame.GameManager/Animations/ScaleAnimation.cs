using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Animations
{
    public class ScaleAnimation : AnimationAbstract<ScaleAnimation>
    {
        public Vector2 ScaleStart { get; set; }
        public Vector2 ScaleEnd { get; set; }

        public ScaleAnimation(IScalableControl control, float duration, Vector2 scaleEnd)
            : base(control, duration)
        {
            SetScaleStart(control.Scale);
            SetScaleEnd(scaleEnd);
        }

        public ScaleAnimation SetScaleStart(Vector2 scaleStart)
        {
            this.ScaleStart = scaleStart;
            return this;
        }

        public ScaleAnimation SetScaleEnd(Vector2 scaleEnd)
        {
            this.ScaleEnd = scaleEnd;
            return this;
        }

        protected override void OnUpdateAnimation(float durationRate)
        {
            var start = ScaleStart;
            var end = ScaleEnd;
            if (IsReverse)
            {
                start = ScaleEnd;
                end = ScaleStart;
            }

            var scale = start + (end - start) * durationRate;
            ((IScalableControl)Control).SetScale(scale);
        }
    }
}
