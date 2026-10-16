using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Video;
using System;

namespace MonoGame.GameManager.Screens
{
    /// <summary>The options of a <see cref="VideoScreen"/>.</summary>
    public class VideoScreenOptions : VideoPlaybackOptions
    {
        /// <summary>When true a tap, a click or the keys Escape, Space and Enter end the video.</summary>
        public bool Skippable { get; set; }

        /// <summary>The color behind the video (the bars of <see cref="VideoScaleMode.Fit"/>).</summary>
        public Color BackgroundColor { get; set; } = Color.Black;

        public VideoScreenOptions SetSkippable(bool skippable)
        {
            Skippable = skippable;
            return this;
        }

        public VideoScreenOptions SetBackgroundColor(Color color)
        {
            BackgroundColor = color;
            return this;
        }
    }

    /// <summary>
    /// A screen that plays a video of the content folder and goes on when it ends (intro, cutscene, logo animation).
    /// Derive from it and override <see cref="OnFinished"/> to decide what comes next, or pass an action to the
    /// constructor.
    /// </summary>
    /// <remarks>
    /// The video is played by the <see cref="IVideoPlayer"/> that the <see cref="IVideoPlayerFactory"/> registered by
    /// the game creates (<see cref="ServiceProvider.Video"/>). When the platform has no player or the file cannot be
    /// opened the screen finishes at once, so the game never gets stuck on a video. The video pauses while the game is
    /// not the active window and resumes with it.
    /// </remarks>
    public class VideoScreen : Screen
    {
        private readonly string contentPath;
        private readonly VideoScreenOptions options;
        private readonly Action onFinished;
        private IVideoPlayer player;
        private Image frameImage;
        private Point placedVideoSize;
        private bool isFinished;
        private bool pausedByFocus;

        /// <param name="contentPath">The video, relative to the content folder, with the extension (eg: <c>Videos/Intro.mp4</c>).</param>
        /// <param name="onFinished">Runs when the video ends, is skipped or fails (after <see cref="OnFinished"/>).</param>
        /// <param name="options">The options (null = the defaults: not skippable, with sound, fitted, black background).</param>
        public VideoScreen(string contentPath, Action onFinished = null, VideoScreenOptions options = null)
        {
            if (string.IsNullOrEmpty(contentPath))
                throw new ArgumentException("The video path cannot be empty.", nameof(contentPath));

            this.contentPath = contentPath;
            this.onFinished = onFinished;
            this.options = options ?? new VideoScreenOptions();
        }

        /// <summary>The player of this screen (null before <see cref="LoadContent"/>).</summary>
        protected IVideoPlayer Player => player;

        /// <summary>True once the video ended, was skipped or failed.</summary>
        public bool IsFinished => isFinished;

        public override void LoadContent()
        {
            player = RegisterDisposable(ServiceProvider.Video.Create(contentPath, options));
        }

        public override void OnInit()
        {
            var size = Root.Size;
            var background = new RectangleControl(Vector2.Zero, size, options.BackgroundColor);
            if (options.Skippable)
                background.AddOnClick(args => Skip());
            else
                background.BlockMouseEvents();
            background.AddToScreen(Root);

            player.Finished += OnPlayerFinished;
            var game = ServiceProvider.Game;
            game.Deactivated += OnGameDeactivated;
            game.Activated += OnGameActivated;

            player.Play();
        }

        public override void Update(GameTime gameTime)
        {
            if (player == null || isFinished)
                return;

            player.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (isFinished)
                return;

            PlaceFrame();

            if (options.Skippable)
            {
                var keyboard = Input.Keyboard;
                if (keyboard.WasKeyPressed(Keys.Escape) || keyboard.WasKeyPressed(Keys.Space) || keyboard.WasKeyPressed(Keys.Enter))
                    Skip();
            }
        }

        /// <summary>Ends the video now (a skip), even when it is not <see cref="VideoScreenOptions.Skippable"/>.</summary>
        public void Skip()
        {
            if (isFinished)
                return;

            player?.Stop();
            Finish();
        }

        /// <summary>
        /// Called once when the video ends, is skipped or fails. The default runs the action of the constructor.
        /// Override it to open the next screen.
        /// </summary>
        protected virtual void OnFinished() => onFinished?.Invoke();

        public override void Dispose()
        {
            if (player != null)
                player.Finished -= OnPlayerFinished;
            var game = ServiceProvider.Game;
            if (game != null)
            {
                game.Deactivated -= OnGameDeactivated;
                game.Activated -= OnGameActivated;
            }

            base.Dispose();
        }

        private void OnPlayerFinished(IVideoPlayer finishedPlayer) => Finish();

        private void Finish()
        {
            if (isFinished)
                return;

            isFinished = true;
            OnFinished();
        }

        private void OnGameDeactivated(object sender, EventArgs args)
        {
            if (player != null && player.State == VideoPlayerState.Playing)
            {
                player.Pause();
                pausedByFocus = true;
            }
        }

        private void OnGameActivated(object sender, EventArgs args)
        {
            if (player != null && pausedByFocus)
            {
                pausedByFocus = false;
                player.Resume();
            }
        }

        /// <summary>Shows the frame texture over the background, fitted into the screen, once the video size is known.</summary>
        private void PlaceFrame()
        {
            var texture = player.FrameTexture;
            if (texture == null)
                return;

            if (frameImage == null)
            {
                frameImage = new Image(texture).SetIsVisible(false);
                if (options.Skippable)
                    frameImage.AddOnClick(args => Skip());
                else
                    frameImage.BlockMouseEvents();
                frameImage.AddToScreen(Root);
            }
            else if (!ReferenceEquals(frameImage.Texture, texture))
                frameImage.SetTexture(texture);

            var videoSize = player.Size;
            if (videoSize == placedVideoSize || videoSize.X <= 0 || videoSize.Y <= 0)
                return;

            placedVideoSize = videoSize;
            var area = Root.Size.ToPoint();
            var rectangle = VideoLayout.Place(videoSize, area, options.ScaleMode, options.ContentOffset);
            frameImage
                .SetPosition(rectangle.Location.ToVector2())
                .SetScale(new Vector2(rectangle.Width / (float)texture.Width, rectangle.Height / (float)texture.Height))
                .SetIsVisible(true);
        }
    }
}
