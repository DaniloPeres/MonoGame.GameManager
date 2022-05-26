using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Controls
{
    public class ControlManager
    {
        private readonly ControlMouseEventHandler controlMouseEventHandler;
        private readonly SpriteBatch spriteBatch;
        public readonly Panel RootPanel;
        private GraphicsDevice graphicsDevice => ServiceProvider.GraphicsDevice;

        public ControlManager(ControlMouseEventHandler controlMouseEventHandler)
        {
            this.controlMouseEventHandler = controlMouseEventHandler;
            spriteBatch = new SpriteBatch(graphicsDevice);
            var graphics = ServiceProvider.GraphicsDeviceManager;
            RootPanel = new Panel(new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            controlMouseEventHandler.AddRootPanel(RootPanel);
        }

        public void Update(GameTime gameTime)
        {
            controlMouseEventHandler.Update(gameTime);
            RootPanel.FireOnUpdateEvent(gameTime);
        }

        public void OnBeforeDraw() => RootPanel.OnBeforeDraw();

        public void Draw()
        {
            graphicsDevice.Clear(ServiceProvider.ScreenManager.ScreenBackgroundColor);
            spriteBatch.Begin();
            RootPanel.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
