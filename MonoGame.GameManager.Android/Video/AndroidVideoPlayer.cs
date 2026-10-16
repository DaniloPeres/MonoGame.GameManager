using Android.App;
using Android.Graphics;
using Android.Media;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Video;
using Point = Microsoft.Xna.Framework.Point;
using System;

namespace MonoGame.GameManager.Droid.Video
{
    /// <summary>
    /// Plays a video asset with the Android <see cref="MediaPlayer"/> on a <see cref="TextureView"/> that covers the
    /// game. The game view is never detached: replacing it would destroy the GL surface and stop the game loop.
    /// </summary>
    /// <remarks>
    /// Every Android view and media call runs on the UI thread. The callbacks only set flags; the state and
    /// <see cref="Finished"/> are published from <see cref="Update"/>, on the game thread. The video is drawn by
    /// Android, so <see cref="FrameTexture"/> is always null.
    /// </remarks>
    public sealed class AndroidVideoPlayer : IVideoPlayer
    {
        private readonly Activity activity;
        private readonly string assetPath;
        private readonly VideoPlaybackOptions options;

        // UI thread only.
        private RelativeLayout overlay;
        private TextureView textureView;
        private Surface surface;
        private MediaPlayer mediaPlayer;
        private bool isPrepared;
        private bool isSurfaceRequested;
        private bool startRequested;
        private bool isPaused;
        private bool isStopped;

        private volatile VideoPlayerState state = VideoPlayerState.Created;
        private volatile bool endReached;
        private volatile string failure;
        private volatile bool isMuted;
        private volatile bool isDisposed;
        private int videoWidth;
        private int videoHeight;
        private bool finishedRaised;

        internal AndroidVideoPlayer(Activity activity, string assetPath, VideoPlaybackOptions options)
        {
            this.activity = activity;
            this.assetPath = assetPath;
            this.options = options;
            isMuted = options.IsMuted;
            RunOnUi(CreateViews);
        }

        public VideoPlayerState State => state;

        public string Error => failure;

        public bool IsPlaying => state == VideoPlayerState.Playing;

        public bool IsPaused => state == VideoPlayerState.Paused;

        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                RunOnUi(ApplyVolume);
            }
        }

        public Point Size => new Point(videoWidth, videoHeight);

        public Texture2D FrameTexture => null;

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
            RunOnUi(() =>
            {
                startRequested = true;
                isPaused = false;
                StartIfReady();
            });
        }

        public void Pause()
        {
            if (isDisposed || state != VideoPlayerState.Playing)
                return;

            state = VideoPlayerState.Paused;
            RunOnUi(() =>
            {
                isPaused = true;
                if (mediaPlayer != null && isPrepared && mediaPlayer.IsPlaying)
                    mediaPlayer.Pause();
            });
        }

        public void Resume()
        {
            if (isDisposed || state != VideoPlayerState.Paused)
                return;

            state = VideoPlayerState.Playing;
            RunOnUi(() =>
            {
                isPaused = false;
                StartIfReady();
            });
        }

        public void Stop()
        {
            if (isDisposed || state == VideoPlayerState.Ended || state == VideoPlayerState.Failed)
                return;

            state = VideoPlayerState.Ended;
            RunOnUi(() =>
            {
                isStopped = true;
                startRequested = false;
                try
                {
                    if (mediaPlayer != null && isPrepared)
                        mediaPlayer.Stop();
                }
                catch (Java.Lang.IllegalStateException)
                {
                    // Not in a state that can stop: the overlay is removed on Dispose anyway.
                }
            });
        }

        public void Reset()
        {
            if (isDisposed || state == VideoPlayerState.Failed)
                return;

            RunOnUi(() =>
            {
                if (mediaPlayer != null && isPrepared)
                    mediaPlayer.SeekTo(0);
            });
        }

        public void Update(float deltaSeconds)
        {
            if (isDisposed)
                return;

            if (failure != null)
                state = VideoPlayerState.Failed;
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
            RunOnUi(ReleaseViews);
        }

        private void RunOnUi(Action action)
        {
            activity.RunOnUiThread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    failure = failure ?? exception.Message;
                }
            });
        }

        private void CreateViews()
        {
            if (isDisposed)
                return;

            overlay = new RelativeLayout(activity)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            overlay.SetBackgroundColor(Android.Graphics.Color.Black);

            // Not clickable: taps go through to the game view, which handles skipping on every platform.
            textureView = new TextureView(activity)
            {
                Clickable = false,
                SurfaceTextureListener = new SurfaceListener(this)
            };
            textureView.LayoutChange += (sender, args) => PlaceTexture();
            overlay.AddView(textureView, new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            // Over the game view: replacing the content view would detach its surface and stop the game loop.
            activity.AddContentView(overlay, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            mediaPlayer = new MediaPlayer();
            mediaPlayer.Prepared += (sender, args) => OnPrepared();
            mediaPlayer.Completion += (sender, args) => endReached = true;
            mediaPlayer.VideoSizeChanged += (sender, args) => OnVideoSizeChanged(args.Width, args.Height);
            mediaPlayer.Error += (sender, args) =>
            {
                failure = $"The media player failed (what {args.What}, extra {args.Extra}).";
                args.Handled = true;
            };

            using (var descriptor = activity.Assets.OpenFd(assetPath))
                mediaPlayer.SetDataSource(descriptor.FileDescriptor, descriptor.StartOffset, descriptor.Length);

            ApplyVolume();
        }

        /// <summary>Called when the texture view has a surface: the media player needs it before it prepares.</summary>
        private void OnSurfaceAvailable(SurfaceTexture surfaceTexture)
        {
            // The view can hand out a new surface texture more than once; the first one starts the video.
            if (isSurfaceRequested || mediaPlayer == null || isDisposed)
                return;

            isSurfaceRequested = true;
            surface = new Surface(surfaceTexture);
            mediaPlayer.SetSurface(surface);
            mediaPlayer.PrepareAsync();
        }

        private void OnPrepared()
        {
            isPrepared = true;
            if (mediaPlayer.VideoWidth > 0 && mediaPlayer.VideoHeight > 0)
                OnVideoSizeChanged(mediaPlayer.VideoWidth, mediaPlayer.VideoHeight);
            StartIfReady();
        }

        private void OnVideoSizeChanged(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return;

            videoWidth = width;
            videoHeight = height;
            PlaceTexture();
        }

        private void StartIfReady()
        {
            if (mediaPlayer == null || !isPrepared || !startRequested || isPaused || isStopped || isDisposed)
                return;

            ApplyVolume();
            mediaPlayer.Start();
        }

        private void ApplyVolume()
        {
            if (mediaPlayer == null)
                return;

            var volume = isMuted ? 0f : 1f;
            mediaPlayer.SetVolume(volume, volume);
        }

        /// <summary>
        /// Places the video inside the texture view with the scale mode of the options. The view always covers the whole
        /// overlay and the video is moved and scaled inside it by a transform: a view larger than its parent (the
        /// <see cref="VideoScaleMode.Fill"/> case) would be cut to the parent by the layout and drawn off center.
        /// </summary>
        private void PlaceTexture()
        {
            if (textureView == null || videoWidth <= 0 || videoHeight <= 0 || textureView.Width <= 0 || textureView.Height <= 0)
                return;

            var area = new Point(textureView.Width, textureView.Height);
            var rectangle = VideoLayout.Place(new Point(videoWidth, videoHeight), area, options.ScaleMode, options.ContentOffset);

            // Without a transform the video is stretched to the view: scale it back to its rectangle, then move it there.
            var transform = new Android.Graphics.Matrix();
            transform.SetScale(rectangle.Width / (float)area.X, rectangle.Height / (float)area.Y);
            transform.PostTranslate(rectangle.X, rectangle.Y);
            textureView.SetTransform(transform);
        }

        private void ReleaseViews()
        {
            if (textureView != null)
                textureView.SurfaceTextureListener = null;

            if (mediaPlayer != null)
            {
                try
                {
                    mediaPlayer.Release();
                }
                catch (Java.Lang.IllegalStateException)
                {
                    // Already released.
                }

                mediaPlayer.Dispose();
                mediaPlayer = null;
            }

            surface?.Release();
            surface?.Dispose();
            surface = null;

            (overlay?.Parent as ViewGroup)?.RemoveView(overlay);
            overlay?.Dispose();
            overlay = null;
            textureView = null;
        }

        private sealed class SurfaceListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
        {
            private readonly AndroidVideoPlayer owner;

            public SurfaceListener(AndroidVideoPlayer owner) => this.owner = owner;

            public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) => owner.OnSurfaceAvailable(surface);

            public bool OnSurfaceTextureDestroyed(SurfaceTexture surface) => false;

            public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) => owner.PlaceTexture();

            public void OnSurfaceTextureUpdated(SurfaceTexture surface) { }
        }
    }
}
