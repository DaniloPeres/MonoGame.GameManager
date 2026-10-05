using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Screens.Transitions
{
    /// <summary>
    /// Fades to a color, changes the screen and fades back. The input is blocked during the transition.
    /// </summary>
    public class FadeTransition : ITransition
    {
        private RectangleControl blocker;
        private EasingFunction easingFunction = Easing.Linear;

        public FadeTransition() : this(0.4f) { }

        /// <param name="duration">The duration of each half of the transition, in seconds.</param>
        /// <param name="color">The color of the fade (black by default).</param>
        public FadeTransition(float duration, Color? color = null)
        {
            Duration = duration;
            Color = color ?? Color.Black;
        }

        public float Duration { get; }

        public Color Color { get; }

        /// <summary>The easing of the fades (linear by default).</summary>
        public EasingFunction EasingFunction
        {
            get => easingFunction;
            set => easingFunction = value ?? Easing.Linear;
        }

        public void TransitionOut(Action onComplete)
        {
            var overlay = GetBlocker(0f);
            Fade(overlay, overlay.Opacity, 1f, onComplete);
        }

        public void TransitionIn(Action onComplete)
        {
            var overlay = GetBlocker(1f);
            Fade(overlay, overlay.Opacity, 0f, () =>
            {
                RemoveBlocker();
                onComplete?.Invoke();
            });
        }

        [Obsolete("Use TransitionOut instead (it hides the current screen).")]
        public void CreateTransitionIn(Action onComplete) => TransitionOut(onComplete);

        [Obsolete("Use TransitionIn instead (it reveals the new screen).")]
        public void CreateTransitionOut() => TransitionIn(null);

        private RectangleControl GetBlocker(float opacity)
        {
            if (blocker != null && !blocker.IsDisposed)
                return blocker;

            var stage = ServiceProvider.ControlManager?.RootPanel
                ?? throw new InvalidOperationException("Transitions can only run after the ScreenManager was initialized.");

            blocker = new RectangleControl(Vector2.Zero, stage.Size, Color)
                .BlockMouseEvents()
                .SetOpacity(opacity)
                .SetZIndex(ZIndexLayers.Transition);
            stage.AddChild(blocker);
            return blocker;
        }

        private void RemoveBlocker()
        {
            blocker?.Dispose();
            blocker = null;
        }

        private void Fade(RectangleControl overlay, float from, float to, Action onComplete)
        {
            new OpacityAnimation(overlay, Duration, to)
                .SetOpacityStart(from)
                .SetEasing(easingFunction)
                .SetScheduler(ServiceProvider.GlobalScheduler)
                .AddOnCompleted(() => onComplete?.Invoke())
                .Play();
        }
    }
}
