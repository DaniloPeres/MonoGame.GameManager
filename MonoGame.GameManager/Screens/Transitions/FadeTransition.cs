using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Screens.Transitions
{
    public class FadeTransition : ITransition
    {
        private readonly float duration;
        private readonly Color color;

        public FadeTransition() : this(0.4f) { }

        public FadeTransition(float duration, Color? color = null)
        {
            this.duration = duration;
            this.color = color ?? Color.Black;
        }

        public void CreateTransitionIn(Action onComplete) => CreateFadeEffect(0, 1, onComplete);
        public void CreateTransitionOut() => CreateFadeEffect(1, 0, null);

        private void CreateFadeEffect(float transparencyFrom, float transparencyTo, Action onAnimationComplete)
        {
            var recBlocker = new RectangleControl(ServiceProvider.ScreenManager.ScreenRectangle, color * transparencyFrom)
                .BlockMouseEvents()
                .AddToScreen()
                .SetZIndex(float.MaxValue);

            new FadeAnimation(recBlocker, duration, transparencyTo)
                .SetBaseColor(color)
                .SetTransparencyStart(transparencyFrom)
                .AddOnAnimationEnd(onAnimationComplete)
                .SetShouldRemoveControlOnAnimationEnd(true)
                .Play();
        }
    }
}
