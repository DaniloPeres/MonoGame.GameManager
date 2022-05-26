using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Timers
{
    public class DelayTime
    {
        public float DelayDuration { get; private set; }
        private float durationPlaying;
        public bool IsPlaying { get; private set; }
        private IControl parent;
        private Action onTimeEnd;
        public bool IsLooping { get; set; }
        public bool IsCompleted => durationPlaying >= DelayDuration;

        public DelayTime(float delayDuration, Action onTimeEnd)
        {
            DelayDuration = delayDuration;
            this.onTimeEnd = onTimeEnd;
        }

        public static DelayTime DelayAction(float delayDuration, Action onTimeEnd)
        {
            return new DelayTime(delayDuration, onTimeEnd)
                .Play();
        }

        public DelayTime SetDelayDuration(float delayDuration)
        {
            DelayDuration = delayDuration;
            return this;
        }
        public DelayTime SetOnTimeEnd(Action onTimeEnd)
        {
            this.onTimeEnd = onTimeEnd;
            return this;
        }

        public DelayTime SetParent(IControl parent)
        {
            if (parent != null)
            {
                var isPlaying = IsPlaying;
                Stop();
                if (isPlaying)
                    Play();
            }
            this.parent = parent;
            return this;
        }

        public DelayTime SetIsLoop(bool loop)
        {
            IsLooping = loop;
            return this;
        }

        public DelayTime Play()
        {
            parent = parent ?? ServiceProvider.RootPanel;
            parent.AddOnUpdateEvent(Update);
            IsPlaying = true;
            return this;
        }

        public DelayTime Stop()
        {
            parent?.RemoveOnUpdateEvent(Update);
            IsPlaying = false;
            return this;
        }

        public DelayTime Reset()
        {
            durationPlaying = 0;
            return this;
        }

        private void Update(GameTime gameTime)
        {
            durationPlaying += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsCompleted)
            {
                if (!IsLooping)
                    Stop();
                else
                    Reset();
                onTimeEnd?.Invoke();
            }
        }
    }
}
