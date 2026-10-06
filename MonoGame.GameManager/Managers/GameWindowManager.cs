using Microsoft.Xna.Framework;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Managers
{
    /// <summary>
    /// Default <see cref="IGameWindowManager"/> implementation.
    /// </summary>
    public class GameWindowManager : IGameWindowManager
    {
        private readonly Game game;
        private Point screenSize;
        private Vector2 screenScale = Vector2.One;
        private Vector4 screenMargin;
        private bool isSubscribedToWindow;

        public GameWindowManager() : this(null) { }

        /// <param name="game">The game whose window is managed (defaults to the current screen manager).</param>
        public GameWindowManager(Game game)
        {
            this.game = game;
        }

        public Point ScreenSize
        {
            get => screenSize;
            set
            {
                if (screenSize == value)
                    return;
                screenSize = value;
                UpdateLayout();
                ScreenSizeChanged?.Invoke();
            }
        }

        public Vector2 ScreenScale
        {
            get => screenScale;
            set
            {
                screenScale = value;
                UpdateLayout();
            }
        }

        public Vector4 ScreenMargin
        {
            get => screenMargin;
            set
            {
                screenMargin = value;
                UpdateLayout();
            }
        }

        public Point DrawScreenPosition { get; private set; }

        public Vector2 DrawScreenScale { get; private set; } = Vector2.One;

        public event Action LayoutChanged;

        public event Action ScreenSizeChanged;

        [Obsolete("Subscribe to the LayoutChanged event instead.")]
        public Action OnClientSizeChanged { get; set; }

        private Game Game => game ?? ServiceProvider.Game;

        /// <summary>Sets the virtual resolution and starts following the size of the window.</summary>
        public void Init(Point screenSize)
        {
            this.screenSize = screenSize;
            var window = Game?.Window;
            if (window != null && !isSubscribedToWindow)
            {
                window.ClientSizeChanged += OnWindowClientSizeChanged;
                isSubscribedToWindow = true;
            }
            UpdateLayout();
        }

        [Obsolete("Use UpdateLayout instead.")]
        public void ClientSizeChanged() => UpdateLayout();

        /// <summary>Resizes the root panels to the virtual resolution.</summary>
        public void UpdateRootSize()
        {
            var controlManager = ServiceProvider.ControlManager;
            controlManager?.RootPanel.SetSize(ScreenSize.ToVector2());
            var screens = ServiceProvider.ScreenManager?.Screens;
            if (screens == null)
                return;
            foreach (var screen in screens)
                screen.Root?.SetSize(ScreenSize.ToVector2());
        }

        public Rectangle GetScreenRectangle() => new Rectangle(Point.Zero, ScreenSize);

        public Rectangle GetScreenDrawRectangle()
        {
            var size = GetScreenDrawSize() * DrawScreenScale;
            return new Rectangle(DrawScreenPosition.X, DrawScreenPosition.Y, (int)Math.Round(size.X), (int)Math.Round(size.Y));
        }

        public Vector2 GetScreenDrawSize() => ScreenSize.ToVector2() * ScreenScale;

        public Vector2 GetScreenDrawSizeWithMargin() => GetScreenDrawSize() + GetScreenMarginByVector();

        /// <summary>
        /// Summarize the margin Left and Right in the X, and Top and Bottom in the Y.
        /// </summary>
        /// <returns>The screen margins as vector2</returns>
        public Vector2 GetScreenMarginByVector()
            => new Vector2(ScreenMargin.X + ScreenMargin.Z, ScreenMargin.Y + ScreenMargin.W);

        public Vector2 GetPositionOnScreen(Vector2 positionOnWindow)
        {
            var scale = DrawScreenScale * ScreenScale;
            if (scale.X == 0f || scale.Y == 0f)
                return positionOnWindow;
            return (positionOnWindow - DrawScreenPosition.ToVector2()) / scale;
        }

        public Vector2 GetPositionOnWindow(Vector2 positionOnScreen)
            => positionOnScreen * DrawScreenScale * ScreenScale + DrawScreenPosition.ToVector2();

        public void SetMarginLeft(float marginLeft)
            => ScreenMargin = new Vector4(marginLeft, ScreenMargin.Y, ScreenMargin.Z, ScreenMargin.W);

        public void SetMarginTop(float marginTop)
            => ScreenMargin = new Vector4(ScreenMargin.X, marginTop, ScreenMargin.Z, ScreenMargin.W);

        public void SetMarginRight(float marginRight)
            => ScreenMargin = new Vector4(ScreenMargin.X, ScreenMargin.Y, marginRight, ScreenMargin.W);

        public void SetMarginBottom(float marginBottom)
            => ScreenMargin = new Vector4(ScreenMargin.X, ScreenMargin.Y, ScreenMargin.Z, marginBottom);

        public void SetWindowSize(Point size)
        {
            var graphics = GetGraphicsDeviceManager();
            if (graphics == null || size.X <= 0 || size.Y <= 0)
                return;
            graphics.PreferredBackBufferWidth = size.X;
            graphics.PreferredBackBufferHeight = size.Y;
            graphics.ApplyChanges();
            UpdateLayout();
        }

        public void SetFullScreen(bool isFullScreen)
        {
            var graphics = GetGraphicsDeviceManager();
            if (graphics == null || graphics.IsFullScreen == isFullScreen)
                return;
            graphics.IsFullScreen = isFullScreen;
            graphics.ApplyChanges();
            UpdateLayout();
        }

        public void ToggleFullScreen()
        {
            var graphics = GetGraphicsDeviceManager();
            if (graphics != null)
                SetFullScreen(!graphics.IsFullScreen);
        }

        /// <summary>
        /// Recalculates the scale and the position of the virtual screen so it fits in the window, centered, with
        /// the margins (in virtual units, scaled together with the screen).
        /// </summary>
        public void UpdateLayout()
        {
            var window = Game?.Window;
            if (window == null)
                return;

            var clientSize = window.ClientBounds.Size.ToVector2();
            var totalSize = GetScreenDrawSizeWithMargin();

            // A minimized window has no size: keep the last valid layout.
            if (clientSize.X <= 0f || clientSize.Y <= 0f || totalSize.X <= 0f || totalSize.Y <= 0f)
                return;

            var scale = Math.Min(clientSize.X / totalSize.X, clientSize.Y / totalSize.Y);
            DrawScreenScale = new Vector2(scale);

            var offset = (clientSize - totalSize * scale) / 2f;
            DrawScreenPosition = (offset + new Vector2(ScreenMargin.X, ScreenMargin.Y) * scale).ToPoint();

            LayoutChanged?.Invoke();
#pragma warning disable CS0618 // compatibility with version 1.x
            OnClientSizeChanged?.Invoke();
#pragma warning restore CS0618
        }

        private void OnWindowClientSizeChanged(object sender, EventArgs e) => UpdateLayout();

        private GraphicsDeviceManager GetGraphicsDeviceManager()
            => Game?.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
    }
}
