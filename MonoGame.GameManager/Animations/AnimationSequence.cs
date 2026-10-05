using System.Linq;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Plays animations one after the other.
    /// </summary>
    /// <example>
    /// <code>
    /// new AnimationSequence()
    ///     .Add(new ScaleAnimation(star, 0.15f, 1.5f).SetEasing(Easing.QuadOut))
    ///     .Add(new ScaleAnimation(star, 0.1f, 1f))
    ///     .AddDelay(0.5f)
    ///     .AddCallback(() => ShowScore())
    ///     .Play();
    /// </code>
    /// </example>
    public class AnimationSequence : CompositeAnimation<AnimationSequence>
    {
        private int currentIndex;

        /// <summary>The sum of the durations of the children.</summary>
        public override float Duration => Children.Sum(animation => animation.Duration);

        /// <summary>The child that is currently playing, or null.</summary>
        public IAnimation CurrentAnimation => currentIndex < Children.Count ? Children[currentIndex] : null;

        /// <inheritdoc />
        protected override void StartChildren()
        {
            currentIndex = 0;
            StartCurrentChild();
        }

        /// <inheritdoc />
        protected override bool UpdateChildren(float deltaSeconds)
        {
            if (currentIndex >= Children.Count)
                return true;

            var current = Children[currentIndex];
            current.Update(deltaSeconds);
            if (current.IsCompleted)
            {
                currentIndex++;
                StartCurrentChild();
            }

            return currentIndex >= Children.Count;
        }

        private void StartCurrentChild()
        {
            // Instant children (callbacks, zero delays) complete when they start: move on to the next one.
            while (currentIndex < Children.Count)
            {
                var child = Children[currentIndex];
                child.Start();
                if (!child.IsCompleted)
                    return;
                currentIndex++;
            }
        }
    }
}
