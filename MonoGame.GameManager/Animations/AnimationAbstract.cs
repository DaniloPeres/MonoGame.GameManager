using Microsoft.Xna.Framework;
using MonoGame.GameManager.Timers;
using System;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Animations
{
    public abstract class AnimationAbstract<TAnimation>
    {
        public readonly IControl Control;
        private IControl parent;
        public float Duration { get; private set; }
        private float durationPlaying;
        private Action onAnimationEnd;
        private Action onAnimationUpdated;
        public bool IsLooping { get; set; }
        public bool IsPingPong { get; set; }
        public bool IsReverse { get; set; }
        public bool IsPlaying { get; private set; }
        public float LoopingDelayTimeDuration { get; private set; }
        private DelayTime pingPongDelayTime;
        public bool IsCompleted => durationPlaying >= Duration;
        public bool ShouldRemoveControlOnAnimationEnd { get; set; }
        private TAnimation ThiasAsT => (TAnimation)(object)this;

        public AnimationAbstract(IControl control, float duration)
        {
            Control = control;
            Duration = duration;
        }

        public TAnimation SetDuration(float duration)
        {
            Duration = duration;
            return ThiasAsT;
        }

        public TAnimation SetIsLooping(bool isLooping)
        {
            IsLooping = isLooping;
            return ThiasAsT;
        }

        public TAnimation SetIsPingPong(bool isPingPong)
        {
            IsPingPong = isPingPong;
            return ThiasAsT;
        }

        public TAnimation SetIsReverse(bool isReverse)
        {
            IsReverse = isReverse;
            return ThiasAsT;
        }

        public TAnimation SetParent(IControl parent)
        {
            if (this.parent != null)
            {
                var isPlaying = IsPlaying;
                Stop();
                if (isPlaying)
                    Play();
            }
            this.parent = parent;
            return ThiasAsT;
        }

        public TAnimation Stop()
        {
            pingPongDelayTime?.Stop();
            pingPongDelayTime = null;
            parent?.RemoveOnUpdateEvent(Update);
            IsPlaying = false;
            return ThiasAsT;
        }

        public TAnimation Play()
        {
            parent = parent ?? ServiceProvider.RootPanel;
            parent.AddOnUpdateEvent(Update);
            IsPlaying = true;
            Update(new GameTime());
            return ThiasAsT;
        }

        public TAnimation AddOnAnimationUpdated(Action onAnimationUpdated)
        {
            this.onAnimationUpdated += onAnimationUpdated;
            return ThiasAsT;
        }

        public TAnimation AddOnAnimationEnd(Action onAnimationEnd)
        {
            this.onAnimationEnd += onAnimationEnd;
            return ThiasAsT;
        }

        public TAnimation ResetAnimation()
        {
            durationPlaying = 0;
            // Update the animation to not wait until next frame to re-calculate
            Update(new GameTime());
            return ThiasAsT;
        }

        public TAnimation SetLoopingDelayTimeDuration(float delayTimeDuration)
        {
            LoopingDelayTimeDuration = delayTimeDuration;
            return ThiasAsT;
        }

        public TAnimation SetShouldRemoveControlOnAnimationEnd(bool shouldRemoveControlOnAnimationEnd)
        {
            ShouldRemoveControlOnAnimationEnd = shouldRemoveControlOnAnimationEnd;
            return ThiasAsT;
        }

        protected abstract void OnUpdateAnimation(float durationRate);

        private void Update(GameTime gameTime)
        {
            durationPlaying = Math.Min(durationPlaying + (float)gameTime.ElapsedGameTime.TotalSeconds, Duration);
            var durationRate = durationPlaying / Duration;

            OnUpdateAnimation(durationRate);

            if (IsCompleted)
            {
                if (IsLooping)
                {
                    if (IsPingPong)
                        IsReverse = !IsReverse;

                    if (IsPlaying && LoopingDelayTimeDuration > 0)
                    {
                        // stop the animation and call a delay to continue
                        Stop();
                        // mark that it still is playing
                        IsPlaying = true;
                        pingPongDelayTime = new DelayTime(LoopingDelayTimeDuration, () =>
                            {
                                ResetAnimation();
                                Play();
                            })
                            .Play();
                    }
                    else
                        ResetAnimation();


                }
                else
                {
                    Stop();
                    if (ShouldRemoveControlOnAnimationEnd)
                        RemoveControl();
                }

                onAnimationEnd?.Invoke();
            }

            onAnimationUpdated?.Invoke();
        }
        private void RemoveControl() => Control.RemoveFromScreen();
    }
}
