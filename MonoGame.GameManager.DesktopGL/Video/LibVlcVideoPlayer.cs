using LibVLCSharp.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Video;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MonoGame.GameManager.DesktopGL.Video
{
    /// <summary>
    /// Plays a video with LibVLC and copies each frame into <see cref="FrameTexture"/>.
    /// </summary>
    /// <remarks>
    /// The file is parsed first, to know the size of the video and ask LibVLC for frames of exactly that size. LibVLC
    /// then decodes on its own threads: its callbacks only write into a pinned buffer and set flags. Everything else
    /// (state, texture upload, <see cref="Finished"/>) happens in <see cref="Update"/>, on the game thread. Two buffers
    /// are used: LibVLC fills one while the other one, the last complete frame, waits to be uploaded.
    /// </remarks>
    public sealed class LibVlcVideoPlayer : IVideoPlayer
    {
        private const string Chroma = "RGBA";
        private const int BytesPerPixel = 4;

        private readonly object frameLock = new object();
        private readonly Media media;
        private readonly MediaPlayer mediaPlayer;
        private readonly Task parseTask;
        private readonly byte[][] buffers = new byte[2][];
        private readonly GCHandle[] handles = new GCHandle[2];

        private VideoPlayerState state = VideoPlayerState.Created;
        private Texture2D texture;
        private int width;
        private int height;
        private int writeIndex;
        private int readIndex = -1;
        private volatile bool endReached;
        private volatile bool hasError;
        private bool isConfigured;
        private bool isStarted;
        private bool finishedRaised;
        private bool isMuted;
        private bool isDisposed;

        internal LibVlcVideoPlayer(LibVLC libVlc, string fullPath, VideoPlaybackOptions options)
        {
            media = new Media(libVlc, fullPath, FromType.FromPath);
            mediaPlayer = new MediaPlayer(media);
            mediaPlayer.EndReached += OnEndReached;
            mediaPlayer.EncounteredError += OnEncounteredError;
            IsMuted = options.IsMuted;
            parseTask = media.Parse(MediaParseOptions.ParseLocal);
        }

        public VideoPlayerState State => state;

        public string Error { get; private set; }

        public bool IsPlaying => state == VideoPlayerState.Playing;

        public bool IsPaused => state == VideoPlayerState.Paused;

        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                // Only the mute flag: LibVLC maps its volume to the Windows session volume, which on some devices
                // (Bluetooth absolute volume) also changes the master volume.
                if (!isDisposed)
                    mediaPlayer.Mute = value;
            }
        }

        public Point Size { get; private set; }

        public Texture2D FrameTexture => texture;

        public event Action<IVideoPlayer> Finished;

        public void Play()
        {
            if (isDisposed || state == VideoPlayerState.Failed)
                return;

            if (state == VideoPlayerState.Paused)
            {
                Resume();
                return;
            }

            if (state == VideoPlayerState.Playing)
                return;

            endReached = false;
            finishedRaised = false;
            state = VideoPlayerState.Playing;
            StartIfReady();
        }

        public void Pause()
        {
            if (isDisposed || state != VideoPlayerState.Playing)
                return;

            state = VideoPlayerState.Paused;
            if (isStarted)
                mediaPlayer.SetPause(true);
        }

        public void Resume()
        {
            if (isDisposed || state != VideoPlayerState.Paused)
                return;

            state = VideoPlayerState.Playing;
            if (isStarted)
                mediaPlayer.SetPause(false);
            else
                StartIfReady();
        }

        public void Stop()
        {
            if (isDisposed || state == VideoPlayerState.Ended || state == VideoPlayerState.Failed)
                return;

            state = VideoPlayerState.Ended;
            if (isStarted)
                StopInBackground();
        }

        public void Reset()
        {
            if (isDisposed || state == VideoPlayerState.Failed)
                return;

            if (isStarted && (state == VideoPlayerState.Playing || state == VideoPlayerState.Paused))
                mediaPlayer.Time = 0;
            else
                state = VideoPlayerState.Created;
        }

        public void Update(float deltaSeconds)
        {
            if (isDisposed)
                return;

            if (!isConfigured && parseTask.IsCompleted)
                Configure();

            UploadFrame();
            PollPlayerState();

            if (hasError && state != VideoPlayerState.Failed)
                Fail("LibVLC could not play the video.");
            else if (endReached && (state == VideoPlayerState.Playing || state == VideoPlayerState.Paused))
                state = VideoPlayerState.Ended;

            if ((state == VideoPlayerState.Ended || state == VideoPlayerState.Failed) && !finishedRaised)
            {
                finishedRaised = true;
                Finished?.Invoke(this);
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            Finished = null;
            mediaPlayer.EndReached -= OnEndReached;
            mediaPlayer.EncounteredError -= OnEncounteredError;

            texture?.Dispose();
            texture = null;

            // Stopping waits for the decoder: never on the game thread, and never from a LibVLC callback. The
            // pinned buffers stay alive until LibVLC no longer writes into them.
            var handlesToFree = handles;
            Task.Run(() =>
            {
                try
                {
                    parseTask.Wait();
                }
                catch (Exception)
                {
                    // A failed parse is already reported by Update.
                }

                try
                {
                    mediaPlayer.Stop();
                    mediaPlayer.Dispose();
                    media.Dispose();
                }
                finally
                {
                    lock (frameLock)
                    {
                        for (var i = 0; i < handlesToFree.Length; i++)
                        {
                            if (handlesToFree[i].IsAllocated)
                                handlesToFree[i].Free();
                        }
                    }
                }
            });
        }

        /// <summary>Once the file is parsed: reads the size of the video and asks LibVLC for frames of that size.</summary>
        private void Configure()
        {
            isConfigured = true;
            if (parseTask.IsFaulted || media.ParsedStatus != MediaParsedStatus.Done)
            {
                Fail("LibVLC could not read the video file.");
                return;
            }

            var videoTrack = media.Tracks.FirstOrDefault(track => track.TrackType == TrackType.Video);
            if (videoTrack.TrackType != TrackType.Video || videoTrack.Data.Video.Width == 0 || videoTrack.Data.Video.Height == 0)
            {
                Fail("The file has no video track.");
                return;
            }

            var videoWidth = videoTrack.Data.Video.Width;
            var videoHeight = videoTrack.Data.Video.Height;
            var frameBytes = checked((int)(videoWidth * videoHeight * BytesPerPixel));
            lock (frameLock)
            {
                for (var i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = new byte[frameBytes];
                    handles[i] = GCHandle.Alloc(buffers[i], GCHandleType.Pinned);
                }

                width = (int)videoWidth;
                height = (int)videoHeight;
            }

            mediaPlayer.SetVideoCallbacks(OnLock, OnUnlock, OnDisplay);
            mediaPlayer.SetVideoFormat(Chroma, videoWidth, videoHeight, videoWidth * BytesPerPixel);

            StartIfReady();
        }

        /// <summary>
        /// The end and the errors are read from the state of the player: its end event is not reliable (it may never
        /// come), while the state is.
        /// </summary>
        private void PollPlayerState()
        {
            if (!isStarted)
                return;

            switch (mediaPlayer.State)
            {
                case VLCState.Ended:
                    endReached = true;
                    break;
                case VLCState.Error:
                    hasError = true;
                    break;
            }
        }

        private void StartIfReady()
        {
            if (!isConfigured || isStarted || state != VideoPlayerState.Playing)
                return;

            isStarted = true;
            mediaPlayer.Play();
        }

        private void Fail(string error)
        {
            Error = error;
            state = VideoPlayerState.Failed;
        }

        private void StopInBackground()
        {
            var player = mediaPlayer;
            Task.Run(() =>
            {
                try
                {
                    player.Stop();
                }
                catch (Exception)
                {
                    // The player was disposed meanwhile.
                }
            });
        }

        /// <summary>Copies the last complete frame to the texture (creating it when the size is known).</summary>
        private void UploadFrame()
        {
            if (width <= 0 || height <= 0)
                return;

            if (texture == null)
            {
                texture = new Texture2D(ServiceProvider.GraphicsDevice, width, height, false, SurfaceFormat.Color);
                Size = new Point(width, height);
            }

            lock (frameLock)
            {
                if (readIndex < 0)
                    return;

                texture.SetData(buffers[readIndex]);
                readIndex = -1;
            }
        }

        private IntPtr OnLock(IntPtr opaque, IntPtr planes)
        {
            Marshal.WriteIntPtr(planes, handles[writeIndex].AddrOfPinnedObject());
            return IntPtr.Zero;
        }

        private void OnUnlock(IntPtr opaque, IntPtr picture, IntPtr planes)
        {
        }

        private void OnDisplay(IntPtr opaque, IntPtr picture)
        {
            // The frame is complete: make it opaque (the alpha LibVLC leaves is undefined) and offer it to the game thread.
            var pixels = MemoryMarshal.Cast<byte, uint>(buffers[writeIndex].AsSpan());
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] |= 0xFF000000u;

            lock (frameLock)
            {
                readIndex = writeIndex;
                writeIndex ^= 1;
            }
        }

        private void OnEndReached(object sender, EventArgs args) => endReached = true;

        private void OnEncounteredError(object sender, EventArgs args) => hasError = true;
    }
}
