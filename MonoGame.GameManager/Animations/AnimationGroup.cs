using System.Linq;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Plays animations at the same time. The group completes when every child completed.
    /// </summary>
    /// <example>
    /// <code>
    /// new AnimationGroup()
    ///     .Add(new MoveAnimation(card, 0.3f, target))
    ///     .Add(new RotationAnimation(card, 0.3f, 180f))
    ///     .AddOnCompleted(OnCardFlipped)
    ///     .Play();
    /// </code>
    /// </example>
    public class AnimationGroup : CompositeAnimation<AnimationGroup>
    {
        /// <summary>The longest duration of the children.</summary>
        public override float Duration => Children.Count == 0 ? 0f : Children.Max(animation => animation.Duration);

        /// <inheritdoc />
        protected override void StartChildren()
        {
            for (var i = 0; i < Children.Count; i++)
                Children[i].Start();
        }

        /// <inheritdoc />
        protected override bool UpdateChildren(float deltaSeconds)
        {
            var allCompleted = true;
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child.IsCompleted)
                    continue;

                child.Update(deltaSeconds);
                if (!child.IsCompleted)
                    allCompleted = false;
            }

            return allCompleted;
        }
    }
}
