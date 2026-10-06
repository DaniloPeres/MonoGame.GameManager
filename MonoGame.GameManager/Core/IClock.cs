using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Core
{
    /// <summary>
    /// Provides the game time of the current frame and a global time scale.
    /// </summary>
    public interface IClock
    {
        /// <summary>The <see cref="Microsoft.Xna.Framework.GameTime"/> of the current frame.</summary>
        GameTime GameTime { get; }

        /// <summary>Elapsed time of the current frame in seconds, multiplied by <see cref="TimeScale"/>.</summary>
        float DeltaSeconds { get; }

        /// <summary>Elapsed time of the current frame in seconds, ignoring <see cref="TimeScale"/>.</summary>
        float UnscaledDeltaSeconds { get; }

        /// <summary>Total game time in seconds, ignoring <see cref="TimeScale"/>.</summary>
        float TotalSeconds { get; }

        /// <summary>Multiplier applied to <see cref="DeltaSeconds"/> (1 = normal speed, 0 = frozen).</summary>
        float TimeScale { get; set; }

        /// <summary>Number of frames updated so far.</summary>
        long FrameCount { get; }

        /// <summary>Advances the clock with the time of a new frame.</summary>
        void Update(GameTime gameTime);
    }
}
