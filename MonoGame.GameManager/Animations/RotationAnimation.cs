using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Animations
{
    public class RotationAnimation : AnimationAbstract<RotationAnimation>
    {
        public float RoationInDegreeStart { get; set; }
        public float RotationInDegreeEnd { get ; set; }

        public RotationAnimation(IControl control, float duration, float rotationInDegreeEnd)
            : base(control, duration)
        {
            SetRotationInDegreeStart(MathHelper.ToDegrees(control.Rotation));
            SetRotationInDegreeEnd(rotationInDegreeEnd);
        }

        public RotationAnimation SetRotationInDegreeStart(float rotationInDegreeStart)
        {
            RoationInDegreeStart = rotationInDegreeStart;
            return this;
        }

        public RotationAnimation SetRotationInDegreeEnd(float rotationInDegreeEnd)
        {
            RotationInDegreeEnd = rotationInDegreeEnd;
            return this;
        }

        protected override void OnUpdateAnimation(float durationRate)
        {
            var start = RoationInDegreeStart;
            var end = RotationInDegreeEnd;
            if (IsReverse)
            {
                start = RotationInDegreeEnd;
                end = RoationInDegreeStart;
            }

            var rotationInDegree = start + (end - start) * durationRate;
            Control.SetRotationInDegree(rotationInDegree);
        }
    }
}
