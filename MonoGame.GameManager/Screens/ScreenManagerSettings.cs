using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Screens.Transitions;

namespace MonoGame.GameManager.Screens
{
    /// <summary>
    /// The configuration of a <see cref="ScreenManager"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyGame : ScreenManager
    /// {
    ///     public MyGame() : base(new ScreenManagerSettings
    ///     {
    ///         VirtualResolution = new Point(320, 180),
    ///         WindowSize = new Point(1280, 720),
    ///         AllowUserResizing = true,
    ///         PixelArt = true,
    ///         DefaultTransition = new FadeTransition(),
    ///         ApplicationName = "MyGame"
    ///     }, new MainMenuScreen()) { }
    /// }
    /// </code>
    /// </example>
    public class ScreenManagerSettings
    {
        /// <summary>The resolution the game is designed for. It is scaled to fit the window.</summary>
        public Point VirtualResolution { get; set; } = new Point(1280, 720);

        /// <summary>The initial size of the window (null = MonoGame default).</summary>
        public Point? WindowSize { get; set; }

        public bool IsFullScreen { get; set; }

        public bool AllowUserResizing { get; set; }

        public DisplayOrientation SupportedOrientations { get; set; } = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

        public string ContentRootDirectory { get; set; } = "Content";

        public bool IsMouseVisible { get; set; } = true;

        /// <summary>The title of the window (null = MonoGame default).</summary>
        public string Title { get; set; }

        /// <summary>
        /// When true, textures are drawn with point sampling (no blur when scaled), for pixel art games.
        /// </summary>
        public bool PixelArt { get; set; }

        /// <summary>The blend state used to draw the controls (null = alpha blend for premultiplied textures).</summary>
        public BlendState BlendState { get; set; }

        /// <summary>The color behind the controls, inside the virtual screen.</summary>
        public Color ScreenBackgroundColor { get; set; } = Color.Black;

        /// <summary>The color of the window outside of the virtual screen (letterbox bars).</summary>
        public Color WindowBackgroundColor { get; set; } = Color.Black;

        /// <summary>The transition used by <see cref="ScreenManager.ChangeScreen(Screen)"/>.</summary>
        public ITransition DefaultTransition { get; set; }

        /// <summary>The name of the folder where save games are stored (defaults to the name of the game assembly).</summary>
        public string ApplicationName { get; set; }

        /// <summary>When true, the input is still processed when the window is not focused.</summary>
        public bool ProcessInputWhenInactive { get; set; }

        /// <summary>MonoGame fixed time step (null = MonoGame default).</summary>
        public bool? IsFixedTimeStep { get; set; }

        /// <summary>Vertical synchronization (null = MonoGame default).</summary>
        public bool? SynchronizeWithVerticalRetrace { get; set; }
    }
}
