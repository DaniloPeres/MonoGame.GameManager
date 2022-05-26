using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Screens.Transitions;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Screens
{
    public class ScreenManager : Game
    {
        public readonly GraphicsDeviceManager Graphics;
        private ControlManager controlManager;
        private Screen actualScreen;
        public GameWindowManager WindowManager => ServiceProvider.GameWindowManager;
        public Point ScreenSize => WindowManager.ScreenSize;
        public Rectangle ScreenRectangle => WindowManager.GetScreenRectangle();
        public ITransition DefaultTransition { get; set; }
        public Color ScreenBackgroundColor { get; set; } = Color.Black;
        public Color WindowBackgroundColor { get; set; } = Color.Black;
        public GameTime GameTime { get; private set; }
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        private readonly Point screenSizeInit;
        public ScreenManager(Point screenSize, Screen initialScreen)
        {
            screenSizeInit = screenSize;
            Graphics = new GraphicsDeviceManager(this)
            {
                SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            actualScreen = initialScreen;
            ServiceProvider.SetScreenManager(this);

            WindowManager.Init(screenSizeInit);
        }

        public void ChangeScreen(Screen screen) => ChangeScreen(screen, DefaultTransition);
        public void ChangeScreenWithNoTransition(Screen screen) => ChangeScreen(screen, null);
        public void ChangeScreen(Screen screen, ITransition transition = null)
        {
            if (transition != null)
                transition.CreateTransitionIn(() => DisposeAndChangeActualScreen(screen, transition));
            else
                DisposeAndChangeActualScreen(screen, transition);
        }

        protected override void Initialize()
        {
            WindowManager.UpdateRootSize();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderTarget = new RenderTarget2D(GraphicsDevice, WindowManager.ScreenSize.X, WindowManager.ScreenSize.Y);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            controlManager = ServiceProvider.ControlManager;
            OpenActualScreen(DefaultTransition);

            WindowManager.ClientSizeChanged();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;
            controlManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            controlManager.OnBeforeDraw();

            // Draw in the render target and after that draw in the correct position and scale
            GraphicsDevice.SetRenderTarget(renderTarget);
            controlManager.Draw();

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            GraphicsDevice.Clear(WindowBackgroundColor);
            var drawPosition = WindowManager.DrawScreenPosition;
            var drawSize = WindowManager.GetScreenDrawSize() * WindowManager.DrawScreenScale;
            spriteBatch.Draw(renderTarget, new Rectangle(drawPosition.X, drawPosition.Y, (int)drawSize.X, (int)drawSize.Y), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DisposeAndChangeActualScreen(Screen screen, ITransition transition)
        {
            actualScreen.Dispose();
            actualScreen = screen;
            OpenActualScreen(transition);
        }

        private void OpenActualScreen(ITransition transition)
        {
            if (ServiceProvider.MemoryManager.CleanMemoryType == Enums.CleanMemoryType.OnChangeScreen)
                ServiceProvider.MemoryManager.CleanMemory();

            ServiceProvider.RootPanel.IterateChildren(x => x.Dispose());
            ServiceProvider.RootPanel.ClearChildren();
            actualScreen.LoadContent();
            actualScreen.OnInit();
            transition?.CreateTransitionOut();
        }
    }
}
