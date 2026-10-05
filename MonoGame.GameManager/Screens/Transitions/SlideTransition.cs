using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Screens.Transitions
{
    /// <summary>The direction of a <see cref="SlideTransition"/>.</summary>
    public enum SlideDirection
    {
        /// <summary>The screens move to the left (the new screen comes from the right).</summary>
        Left,
        /// <summary>The screens move to the right (the new screen comes from the left).</summary>
        Right,
        /// <summary>The screens move up (the new screen comes from the bottom).</summary>
        Up,
        /// <summary>The screens move down (the new screen comes from the top).</summary>
        Down
    }

    /// <summary>
    /// Slides the current screen out and the new screen in. The input is blocked during the transition.
    /// </summary>
    public class SlideTransition : ITransition
    {
        private RectangleControl inputBlocker;

        /// <param name="direction">The direction the screens move to.</param>
        /// <param name="duration">The duration of each half of the transition, in seconds.</param>
        public SlideTransition(SlideDirection direction = SlideDirection.Left, float duration = 0.35f)
        {
            Direction = direction;
            Duration = duration;
        }

        public SlideDirection Direction { get; }

        public float Duration { get; }

        public void TransitionOut(Action onComplete)
        {
            var root = ServiceProvider.ScreenManager?.CurrentScreen?.Root;
            BlockInput();
            if (root == null)
            {
                onComplete?.Invoke();
                return;
            }

            new MoveAnimation(root, Duration, GetOffset() * -1f)
                .SetPositionStart(Vector2.Zero)
                .SetEasing(Easing.CubicIn)
                .SetScheduler(ServiceProvider.GlobalScheduler)
                .AddOnCompleted(() => onComplete?.Invoke())
                .Play();
        }

        public void TransitionIn(Action onComplete)
        {
            var root = ServiceProvider.ScreenManager?.CurrentScreen?.Root;
            if (root == null)
            {
                UnblockInput();
                onComplete?.Invoke();
                return;
            }

            BlockInput();
            new MoveAnimation(root, Duration, Vector2.Zero)
                .SetPositionStart(GetOffset())
                .SetEasing(Easing.CubicOut)
                .SetScheduler(ServiceProvider.GlobalScheduler)
                .AddOnCompleted(() =>
                {
                    UnblockInput();
                    onComplete?.Invoke();
                })
                .Play();
        }

        /// <summary>The position of the new screen when the transition starts.</summary>
        private Vector2 GetOffset()
        {
            var size = (ServiceProvider.ScreenManager?.ScreenSize ?? Point.Zero).ToVector2();
            switch (Direction)
            {
                case SlideDirection.Right: return new Vector2(-size.X, 0f);
                case SlideDirection.Up: return new Vector2(0f, size.Y);
                case SlideDirection.Down: return new Vector2(0f, -size.Y);
                default: return new Vector2(size.X, 0f);
            }
        }

        private void BlockInput()
        {
            if (inputBlocker != null && !inputBlocker.IsDisposed)
                return;

            var stage = ServiceProvider.ControlManager?.RootPanel;
            if (stage == null)
                return;

            inputBlocker = new RectangleControl(Vector2.Zero, stage.Size, Color.Transparent)
                .BlockMouseEvents()
                .SetZIndex(ZIndexLayers.Transition);
            stage.AddChild(inputBlocker);
        }

        private void UnblockInput()
        {
            inputBlocker?.Dispose();
            inputBlocker = null;
        }
    }
}
