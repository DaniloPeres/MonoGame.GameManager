using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Moves a control from its current position to <see cref="PositionAnimationBase{TAnimation}.PositionEnd"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// new MoveAnimation(panel, 0.4f, new Vector2(0, 120))
    ///     .SetEasing(Easing.CubicOut)
    ///     .Play();
    /// </code>
    /// </example>
    public class MoveAnimation : PositionAnimationBase<MoveAnimation>
    {
        public MoveAnimation(IControl control, float duration, Vector2 positionEnd)
            : base(control, duration, positionEnd) { }
    }
}
