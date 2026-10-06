using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Moves a control linearly. Kept for compatibility with version 1.x.
    /// </summary>
    [Obsolete("EaseAnimation is a linear move. Use MoveAnimation and SetEasing(...) instead.")]
    public class EaseAnimation : PositionAnimationBase<EaseAnimation>
    {
        public EaseAnimation(IControl control, float duration, Vector2 positionEnd)
            : base(control, duration, positionEnd) { }
    }
}
