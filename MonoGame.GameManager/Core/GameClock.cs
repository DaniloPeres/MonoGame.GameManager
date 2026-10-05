using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Core
{
    /// <summary>
    /// Default <see cref="IClock"/> implementation, updated once per frame by the <see cref="Screens.ScreenManager"/>.
    /// </summary>
    public class GameClock : IClock
    {
        private float timeScale = 1f;

        /// <inheritdoc />
        public GameTime GameTime { get; private set; } = new GameTime();

        /// <inheritdoc />
        public float DeltaSeconds => UnscaledDeltaSeconds * timeScale;

        /// <inheritdoc />
        public float UnscaledDeltaSeconds { get; private set; }

        /// <inheritdoc />
        public float TotalSeconds { get; private set; }

        /// <inheritdoc />
        public float TimeScale
        {
            get => timeScale;
            set => timeScale = Math.Max(0f, value);
        }

        /// <inheritdoc />
        public long FrameCount { get; private set; }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            GameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
            UnscaledDeltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TotalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            FrameCount++;
        }
    }
}
