using MonoGame.GameManager.Core;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Common contract of every animation, including composites such as <see cref="AnimationSequence"/> and
    /// <see cref="AnimationGroup"/>.
    /// </summary>
    public interface IAnimation : IUpdatable, IPlayable
    {
        /// <summary>The duration of one cycle in seconds (for composites, the total duration).</summary>
        float Duration { get; }

        /// <summary>True when the animation reached its end and is not running anymore.</summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Starts the animation from the beginning without registering it in a scheduler.
        /// The caller is then responsible for calling <see cref="IUpdatable.Update"/> (used by composites).
        /// </summary>
        void Start();

        /// <summary>Raised once when the animation completes (never raised by animations that loop forever).</summary>
        event Action Completed;
    }
}
