using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.GameMath;

namespace MonoGame.GameManager.Animations
{
    public class FadeAnimation : AnimationAbstract<FadeAnimation>
    {
        public float TransparencyStart { get; set; }
        public float TransparencyEnd { get; set; }
        private Color baseColor;

        public FadeAnimation(IControl control, float duration, float transparencyEnd)
            : base(control, duration)
        {
            baseColor = control.Color.A == 0 ? Color.White : control.Color;
            SetTransparencyEnd(transparencyEnd);
            if (transparencyEnd == 0f)
                SetTransparencyStart(1f);
        }

        public FadeAnimation SetTransparencyStart(float transparencyStart)
        {
            this.TransparencyStart = MathExtension.CapValue(transparencyStart, 0, 1);
            return this;
        }

        public FadeAnimation SetTransparencyEnd(float transparencyEnd)
        {
            this.TransparencyEnd = MathExtension.CapValue(transparencyEnd, 0, 1);
            return this;
        }

        public FadeAnimation SetBaseColor(Color baseColor)
        {
            this.baseColor = baseColor;
            return this;
        }

        protected override void OnUpdateAnimation(float durationRate)
        {
            var start = TransparencyStart;
            var end = TransparencyEnd;
            if (IsReverse)
            {
                start = TransparencyEnd;
                end = TransparencyStart;
            }

            var transparencyRate = start + (end - start) * durationRate;
            Control.SetColor(baseColor * transparencyRate);
        }
    }
}
