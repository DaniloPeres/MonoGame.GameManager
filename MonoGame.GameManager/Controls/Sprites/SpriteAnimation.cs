using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls.Sprites
{
    public class SpriteAnimation : ScalableControlAbstract<SpriteAnimation>
    {
        public SpriteAnimationInfo SpriteAnimationInfo { get; private set; }
        private SpriteAnimationCycle actualCycle;
        public SpriteAnimationCycle ActualCycle {
            get => actualCycle;
            private set
            {
                actualCycle = value;
                MarkAsDirty();
            }
        }
        public SpriteAnimationFrame ActualFrame => ActualCycle?.Frames[FrameIndex];
        public bool IsLooping { get; set; } = true;
        public bool IsReverse { get; set; }
        public bool IsPingPong { get; set; }
        public bool ShouldRemoveFromScreenOnAnimationEnd { get; set; }
        public bool IsPlaying { get; private set; }
        public float Speed { get; set; } = 1f;
        private double time;
        private int frameIndex;
        public int FrameIndex {
            get => MathExtension.CapValue(frameIndex, 0, ActualCycle?.Frames.Length - 1 ?? 0);
            set
            {
                frameIndex = value;
                onFrameChanged?.Invoke();
            }
        }
        public bool IsLastFrame => FrameIndex == 0 && IsReverse || FrameIndex == ActualCycle?.Frames.Length - 1 && !IsReverse;
        private Action onAnimationEndEvent;
        private Action onAnimationPlay;
        private Action onAnimationStop;
        private Action onFrameChanged;


        public SpriteAnimation(SpriteAnimationInfo spriteAnimationInfo)
            : this(spriteAnimationInfo, SpriteAnimationCycle.DefaultCycleName) { }

        public SpriteAnimation(SpriteAnimationInfo spriteAnimationInfo, string cycleName)
        {
            SpriteAnimationInfo = spriteAnimationInfo;
        }

        public void ChangeSpriteAnimationInfoOnAnimationEnd(SpriteAnimationInfo spriteAnimationInfo)
            => ChangeSpriteAnimationInfoOnAnimationEnd(spriteAnimationInfo, SpriteAnimationCycle.DefaultCycleName);
        public void ChangeSpriteAnimationInfoOnAnimationEnd(SpriteAnimationInfo spriteAnimationInfo, string cycleName)
        {
            throw new System.NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch) => DrawTexture(spriteBatch, ActualFrame.Texture, DestinationRectangle, ActualFrame.SourceRectangle, OriginWithoutScale);

        /// <summary>
        /// Play the sprite animation getting the first cycle in the list of Sprite animation info
        /// </summary>
        /// <param name="resetAnimation">Should reset the sprite animation</param>
        public SpriteAnimation Play(bool resetAnimation = true) => Play(actualCycle?.Name ?? SpriteAnimationInfo.GetSpriteAnimationCycleByIndex(0).Name, resetAnimation);

        public SpriteAnimation Play(int cycleIndex, bool resetAnimation = true) => Play(SpriteAnimationInfo.GetSpriteAnimationCycleByIndex(cycleIndex).Name, resetAnimation);

        public SpriteAnimation Play(string cycleName, bool resetAnimation = true)
        {
            var cycle = SpriteAnimationInfo.GetSpriteAnimationCycle(cycleName);
            ActualCycle = cycle;

            if (resetAnimation)
                ResetAnimation();

            AddOnUpdateEvent(UpdateSprite);
            IsPlaying = true;
            onAnimationPlay?.Invoke();

            return this;
        }

        public void Stop()
        {
            RemoveOnUpdateEvent(UpdateSprite);
            IsPlaying = false;
            onAnimationStop?.Invoke();
        }

        public SpriteAnimation AddOnAnimationEndEvent(Action onAnimationEndEvent)
        {
            this.onAnimationEndEvent += onAnimationEndEvent;
            return this;
        }

        public SpriteAnimation RemoveOnAnimationEndEvent(Action onAnimationEndEvent)
        {
            this.onAnimationEndEvent -= onAnimationEndEvent;
            return this;
        }

        public SpriteAnimation AddOnAnimationPlay(Action onAnimationPlay)
        {
            this.onAnimationPlay += onAnimationPlay;
            return this;
        }

        public SpriteAnimation RemoveOnAnimationPlay(Action onAnimationPlay)
        {
            this.onAnimationPlay -= onAnimationPlay;
            return this;
        }

        public SpriteAnimation AddOnAnimationStop(Action onAnimationStop)
        {
            this.onAnimationStop += onAnimationStop;
            return this;
        }

        public SpriteAnimation RemoveOnAnimationStop(Action onAnimationStop)
        {
            this.onAnimationStop -= onAnimationStop;
            return this;
        }

        public SpriteAnimation AddOnFrameChanged(Action onFrameChanged)
        {
            this.onFrameChanged += onFrameChanged;
            return this;
        }

        public SpriteAnimation RemoveOnFrameChanged(Action onFrameChanged)
        {
            this.onFrameChanged -= onFrameChanged;
            return this;
        }

        public void ResetAnimation()
        {
            time = 0;
            SetFrame(GetFirstFrameIndex());
        }

        public void ChangeAnimationSpriteInfo(SpriteAnimationInfo spriteAnimationInfo, bool resetAnimation = true)
            => ChangeAnimationSpriteInfo(spriteAnimationInfo, null, resetAnimation);

        public void ChangeAnimationSpriteInfo(SpriteAnimationInfo spriteAnimationInfo, string cycleName, bool resetAnimation = true)
        {
            SpriteAnimationInfo = spriteAnimationInfo;
            if (string.IsNullOrEmpty(cycleName))
                ActualCycle = SpriteAnimationInfo.GetSpriteAnimationCycleByIndex(0);

            if (resetAnimation)
                ResetAnimation();
        }

        public void SetFrame(int frameIndex)
        {
            FrameIndex = frameIndex;
            MarkAsDirty();
        }

        protected override Vector2 CalculateSize() => ActualFrame.SourceRectangle.Size.ToVector2();

        private void UpdateSprite(GameTime gameTime)
        {
            time += (gameTime.ElapsedGameTime.TotalSeconds * Speed);

            while (time >= ActualCycle.Frames[FrameIndex].Duration)
            {
                // Update to the next frame
                time -= ActualCycle.Frames[FrameIndex].Duration;
                UpdateToNextFrame();
            }
        }

        private int GetFirstFrameIndex() => IsReverse ? ActualCycle.Frames.Length - 1 : 0;

        private void UpdateToNextFrame()
        {
            var newFrameIndex = FrameIndex + (IsReverse ? -1 : 1);

            // check if the animation is over
            if (newFrameIndex >= ActualCycle.Frames.Length || newFrameIndex == -1)
            {
                onAnimationEndEvent?.Invoke();

                if (IsPingPong)
                    IsReverse = !IsReverse;

                if (ShouldRemoveFromScreenOnAnimationEnd)
                    RemoveFromScreen();

                // TODO change sprite animation info on the animation end
                // TODO change cycle on the animation end
                
                if (!IsLooping)
                {
                    Stop();
                    newFrameIndex = FrameIndex; // keep the previous frame index to stop on the last frame
                } else
                {
                    newFrameIndex = GetFirstFrameIndex(); // Set as first frame
                }

                onAnimationEndEvent?.Invoke();
            }

            SetFrame(newFrameIndex);
        }
    }
}
