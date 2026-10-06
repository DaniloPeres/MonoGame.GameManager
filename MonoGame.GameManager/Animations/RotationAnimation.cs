using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Rotates a control to <see cref="RotationInDegreeEnd"/> (in degrees). Unless
    /// <see cref="RotationInDegreeStart"/> is set explicitly, it starts from the rotation of the control when it
    /// is played.
    /// </summary>
    public class RotationAnimation : AnimationAbstract<RotationAnimation>
    {
        private float rotationInDegreeStart;
        private bool isStartExplicit;

        public RotationAnimation(IControl control, float duration, float rotationInDegreeEnd)
            : base(control ?? throw new ArgumentNullException(nameof(control)), duration)
        {
            rotationInDegreeStart = MathHelper.ToDegrees(control.Rotation);
            RotationInDegreeEnd = rotationInDegreeEnd;
        }

        /// <summary>The rotation at the start, in degrees.</summary>
        public float RotationInDegreeStart
        {
            get => rotationInDegreeStart;
            set
            {
                rotationInDegreeStart = value;
                isStartExplicit = true;
            }
        }

        /// <summary>The rotation at the end, in degrees.</summary>
        public float RotationInDegreeEnd { get; set; }

        [Obsolete("Use RotationInDegreeStart instead.")]
        public float RoationInDegreeStart
        {
            get => RotationInDegreeStart;
            set => RotationInDegreeStart = value;
        }

        public RotationAnimation SetRotationInDegreeStart(float rotationInDegreeStart)
        {
            RotationInDegreeStart = rotationInDegreeStart;
            return this;
        }

        public RotationAnimation SetRotationInDegreeEnd(float rotationInDegreeEnd)
        {
            RotationInDegreeEnd = rotationInDegreeEnd;
            return this;
        }

        /// <inheritdoc />
        protected override void CaptureStartValues()
        {
            if (!isStartExplicit)
                rotationInDegreeStart = MathHelper.ToDegrees(Control.Rotation);
        }

        /// <inheritdoc />
        protected override void OnUpdateAnimation(float progress)
            => Control.SetRotationInDegree(rotationInDegreeStart + (RotationInDegreeEnd - rotationInDegreeStart) * progress);
    }
}
