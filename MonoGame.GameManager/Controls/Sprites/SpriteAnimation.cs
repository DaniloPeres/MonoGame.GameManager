using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using System;

namespace MonoGame.GameManager.Controls.Sprites
{
    /// <summary>
    /// Plays the cycles of a <see cref="SpriteAnimationInfo"/> (frame by frame animation).
    /// The animation advances while the control is on the screen.
    /// </summary>
    public class SpriteAnimation : ScalableControlAbstract<SpriteAnimation>
    {
        private SpriteAnimationCycle currentCycle;
        private SpriteAnimationInfo pendingInfo;
        private string pendingCycleName;
        private double time;
        private int frameIndex;
        private Action onAnimationEndEvent;
        private Action onAnimationPlay;
        private Action onAnimationStop;
        private Action onFrameChanged;

        public SpriteAnimation(SpriteAnimationInfo spriteAnimationInfo)
            : this(spriteAnimationInfo, null) { }

        /// <param name="spriteAnimationInfo">The cycles.</param>
        /// <param name="cycleName">The cycle shown before <see cref="Play(bool)"/> (null = the first cycle).</param>
        public SpriteAnimation(SpriteAnimationInfo spriteAnimationInfo, string cycleName)
        {
            SpriteAnimationInfo = spriteAnimationInfo ?? throw new ArgumentNullException(nameof(spriteAnimationInfo));
            CurrentCycle = ResolveCycle(spriteAnimationInfo, cycleName);
        }

        public SpriteAnimationInfo SpriteAnimationInfo { get; private set; }

        /// <summary>The cycle being played.</summary>
        public SpriteAnimationCycle CurrentCycle
        {
            get => currentCycle;
            private set
            {
                currentCycle = value;
                MarkAsDirty();
            }
        }

        /// <summary>The frame being shown.</summary>
        public SpriteAnimationFrame CurrentFrame => CurrentCycle?.Frames[FrameIndex];

        [Obsolete("Use CurrentCycle instead.")]
        public SpriteAnimationCycle ActualCycle => CurrentCycle;

        [Obsolete("Use CurrentFrame instead.")]
        public SpriteAnimationFrame ActualFrame => CurrentFrame;

        public bool IsLooping { get; set; } = true;
        public bool IsReverse { get; set; }
        public bool IsPingPong { get; set; }
        public bool ShouldRemoveFromScreenOnAnimationEnd { get; set; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        /// <summary>Playback speed multiplier (1 = normal speed).</summary>
        public float Speed { get; set; } = 1f;

        public int FrameIndex
        {
            get => MathHelper.Clamp(frameIndex, 0, Math.Max(0, (CurrentCycle?.Frames.Length ?? 1) - 1));
            set
            {
                frameIndex = value;
                onFrameChanged?.Invoke();
            }
        }

        public bool IsLastFrame => FrameIndex == 0 && IsReverse || FrameIndex == CurrentCycle?.Frames.Length - 1 && !IsReverse;

        /// <summary>
        /// Switches to another animation (and cycle) when the current cycle ends, eg: play "jump" and then "run".
        /// </summary>
        public void ChangeSpriteAnimationInfoOnAnimationEnd(SpriteAnimationInfo spriteAnimationInfo)
            => ChangeSpriteAnimationInfoOnAnimationEnd(spriteAnimationInfo, null);

        /// <summary>
        /// Switches to another animation (and cycle) when the current cycle ends, eg: play "jump" and then "run".
        /// </summary>
        public void ChangeSpriteAnimationInfoOnAnimationEnd(SpriteAnimationInfo spriteAnimationInfo, string cycleName)
        {
            pendingInfo = spriteAnimationInfo ?? throw new ArgumentNullException(nameof(spriteAnimationInfo));
            pendingCycleName = cycleName;
        }

        /// <summary>Switches to another cycle of the same animation when the current cycle ends.</summary>
        public void ChangeCycleOnAnimationEnd(string cycleName) => ChangeSpriteAnimationInfoOnAnimationEnd(SpriteAnimationInfo, cycleName);

        public override void Draw(SpriteBatch spriteBatch)
        {
            var frame = CurrentFrame;
            if (frame != null)
                DrawTexture(spriteBatch, frame.Texture, DestinationRectangle, frame.SourceRectangle, OriginWithoutScale);
        }

        /// <summary>
        /// Plays the current cycle (the first cycle when none was chosen).
        /// </summary>
        /// <param name="resetAnimation">Should reset the sprite animation</param>
        public SpriteAnimation Play(bool resetAnimation = true) => Play(CurrentCycle?.Name ?? SpriteAnimationInfo.GetSpriteAnimationCycleByIndex(0).Name, resetAnimation);

        public SpriteAnimation Play(int cycleIndex, bool resetAnimation = true) => Play(SpriteAnimationInfo.GetSpriteAnimationCycleByIndex(cycleIndex).Name, resetAnimation);

        public SpriteAnimation Play(string cycleName, bool resetAnimation = true)
        {
            var cycle = SpriteAnimationInfo.GetSpriteAnimationCycle(cycleName)
                ?? throw new ArgumentException($"The sprite animation has no cycle named '{cycleName}'.", nameof(cycleName));
            CurrentCycle = cycle;

            if (resetAnimation)
                ResetAnimation();

            AddOnUpdateEvent(UpdateSprite);
            IsPlaying = true;
            IsPaused = false;
            onAnimationPlay?.Invoke();

            return this;
        }

        public void Stop()
        {
            RemoveOnUpdateEvent(UpdateSprite);
            IsPlaying = false;
            IsPaused = false;
            onAnimationStop?.Invoke();
        }

        /// <summary>Pauses the animation on the current frame.</summary>
        public SpriteAnimation Pause()
        {
            if (IsPlaying)
                IsPaused = true;
            return this;
        }

        public SpriteAnimation Resume()
        {
            IsPaused = false;
            return this;
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
            SpriteAnimationInfo = spriteAnimationInfo ?? throw new ArgumentNullException(nameof(spriteAnimationInfo));
            CurrentCycle = ResolveCycle(spriteAnimationInfo, cycleName);

            if (resetAnimation)
                ResetAnimation();
        }

        public void SetFrame(int frameIndex)
        {
            FrameIndex = frameIndex;
            MarkAsDirty();
        }

        protected override Vector2 CalculateSize() => CurrentFrame?.SourceRectangle.Size.ToVector2() ?? Vector2.Zero;

        /// <inheritdoc />
        protected override int? GetContentSignature()
        {
            var frame = CurrentFrame;
            return frame == null ? 0 : HashCode.Combine(frame.Texture, frame.SourceRectangle);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                onAnimationEndEvent = null;
                onAnimationPlay = null;
                onAnimationStop = null;
                onFrameChanged = null;
            }
            base.Dispose(disposing);
        }

        private static SpriteAnimationCycle ResolveCycle(SpriteAnimationInfo info, string cycleName)
        {
            if (string.IsNullOrEmpty(cycleName))
                return info.GetSpriteAnimationCycleByIndex(0);

            return info.GetSpriteAnimationCycle(cycleName)
                ?? throw new ArgumentException($"The sprite animation has no cycle named '{cycleName}'.", nameof(cycleName));
        }

        private void UpdateSprite(GameTime gameTime)
        {
            if (!IsPlaying || IsPaused || CurrentCycle == null)
                return;

            time += GetScaledDeltaSeconds(gameTime) * Speed;

            // Advance at most one full cycle per update, so a long frame cannot loop forever.
            var steps = 0;
            while (IsPlaying && time >= CurrentFrame.EffectiveDuration && steps <= CurrentCycle.Frames.Length)
            {
                // Update to the next frame
                time -= CurrentFrame.EffectiveDuration;
                UpdateToNextFrame();
                steps++;
            }

            if (steps > CurrentCycle.Frames.Length)
                time = 0;
        }

        private int GetFirstFrameIndex() => IsReverse ? CurrentCycle.Frames.Length - 1 : 0;

        private void UpdateToNextFrame()
        {
            var newFrameIndex = FrameIndex + (IsReverse ? -1 : 1);

            // check if the animation is over
            if (newFrameIndex >= CurrentCycle.Frames.Length || newFrameIndex < 0)
            {
                if (IsPingPong)
                    IsReverse = !IsReverse;

                if (pendingInfo != null)
                {
                    var info = pendingInfo;
                    var cycleName = pendingCycleName;
                    pendingInfo = null;
                    pendingCycleName = null;
                    SpriteAnimationInfo = info;
                    CurrentCycle = ResolveCycle(info, cycleName);
                    newFrameIndex = GetFirstFrameIndex();
                }
                else if (!IsLooping)
                {
                    Stop();
                    newFrameIndex = FrameIndex; // keep the previous frame index to stop on the last frame
                }
                else
                {
                    newFrameIndex = GetFirstFrameIndex(); // Set as first frame
                }

                if (ShouldRemoveFromScreenOnAnimationEnd)
                    RemoveFromScreen();

                onAnimationEndEvent?.Invoke();
            }

            SetFrame(newFrameIndex);
        }
    }
}
