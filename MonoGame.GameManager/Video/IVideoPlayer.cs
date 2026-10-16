using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Core;
using System;

namespace MonoGame.GameManager.Video
{
    /// <summary>
    /// Plays one video file. The platform projects implement it (LibVLC on DesktopGL, the native media player on
    /// Android); a game only talks to this interface, usually through <see cref="Screens.VideoScreen"/>.
    /// </summary>
    /// <remarks>
    /// The player must be updated every frame with <see cref="IUpdatable.Update"/> (the screen does it). That is when
    /// it publishes its state, copies the frame into <see cref="FrameTexture"/> and raises <see cref="Finished"/>,
    /// so everything is seen on the game thread even if the decoder works on other threads.
    /// </remarks>
    public interface IVideoPlayer : IPlayable, IUpdatable, IDisposable
    {
        VideoPlayerState State { get; }

        /// <summary>Why the playback failed (null unless <see cref="State"/> is <see cref="VideoPlayerState.Failed"/>).</summary>
        string Error { get; }

        bool IsMuted { get; set; }

        /// <summary>The size of the video in pixels (zero until the file is opened).</summary>
        Point Size { get; }

        /// <summary>
        /// The current frame, for players that decode into a texture (DesktopGL). Null when the platform draws the
        /// video itself, over the game (Android). It is created when the size is known and belongs to the player.
        /// </summary>
        Texture2D FrameTexture { get; }

        /// <summary>
        /// Raised once, from <see cref="IUpdatable.Update"/>, when the video ends, is stopped or fails. Not raised
        /// after <see cref="IDisposable.Dispose"/>.
        /// </summary>
        event Action<IVideoPlayer> Finished;
    }
}
