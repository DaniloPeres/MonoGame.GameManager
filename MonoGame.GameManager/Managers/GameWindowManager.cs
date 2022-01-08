using Microsoft.Xna.Framework;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Managers
{
    public class GameWindowManager
    {
        private Game game => ServiceProvider.Game;
        public Point ScreenSize { get; set; }
        public Rectangle GetScreenRectangle() => new Rectangle(Point.Zero, ScreenSize);
        private Vector2 screenScale = Vector2.One;
        public Vector2 ScreenScale
        {
            get => screenScale;
            set
            {
                screenScale = value;
                ClientSizeChanged();
            }
        }
        private Vector4 screenMargin;
        public Vector4 ScreenMargin
        {
            get => screenMargin;
            set
            {
                screenMargin = value;
                ClientSizeChanged();
            }
        }
        public Action OnClientSizeChanged { get; set; }
        public Point DrawScreenPosition { get; private set; }
        public Vector2 DrawScreenScale { get; private set; }
        private Point ClientBoundsSize => game.Window.ClientBounds.Size;

        public void Init(Point screenSize)
        {
            ScreenSize = screenSize;
            game.Window.ClientSizeChanged += new EventHandler<EventArgs>(ClientSizeChanged);
            ClientSizeChanged();
        }

        private void ClientSizeChanged(object sender, EventArgs e)
        {
            ClientSizeChanged();
        }

        public void ClientSizeChanged()
        {
            CalculateScreenDrawInfo();
        }

        public void UpdateRootSize() => ServiceProvider.RootPanel.SetSize(ScreenSize.ToVector2());

        private void CalculateScreenDrawInfo()
        {
            var screenDrawSizeWithMargin = GetScreenDrawSizeWithMargin();

            var drawScaleCalc = ClientBoundsSize.ToVector2() / screenDrawSizeWithMargin;
            // use the smallest direction
            DrawScreenScale = new Vector2(Math.Min(drawScaleCalc.X, drawScaleCalc.Y));
            DrawScreenPosition = (PositionCalculations.CenterVerticalAndHorizontal(GetScreenDrawSize() * DrawScreenScale, ClientBoundsSize) + new Vector2(ScreenMargin.X, ScreenMargin.Y)).ToPoint();
        }

        public Vector2 GetScreenDrawSize()
        {
            return ScreenSize.ToVector2() * ScreenScale;
        }

        public Vector2 GetScreenDrawSizeWithMargin()
        {
            return GetScreenDrawSize() + GetScreenMarginByVector();
        }

        /// <summary>
        /// Summarize the margin Left and Right in the X, and Top and Bottom in the Y
        /// </summary>
        /// <returns>The screen margins as vector2</returns>
        public Vector2 GetScreenMarginByVector()
            => new Vector2(ScreenMargin.X + ScreenMargin.Z, ScreenMargin.Y + ScreenMargin.W);

        /// <summary>
        /// Calculate the correct position after the screen has a new position and scale
        /// </summary>
        /// <param name="positionOnWindow">The position from window</param>
        /// <returns>The position from Screen Game</returns>
        public Vector2 GetPositionOnScreen(Vector2 positionOnWindow)
            => (positionOnWindow - DrawScreenPosition.ToVector2()) / DrawScreenScale / ScreenScale;

        public void SetMarginLeft(float marginLeft)
            => ScreenMargin = new Vector4(marginLeft, ScreenMargin.Y, ScreenMargin.Z, ScreenMargin.W);

        public void SetMarginTop(float marginTop)
            => ScreenMargin = new Vector4(ScreenMargin.X, marginTop, ScreenMargin.Z, ScreenMargin.W);

        public void SetMarginRight(float marginRight)
            => ScreenMargin = new Vector4(ScreenMargin.X, ScreenMargin.Y, marginRight, ScreenMargin.W);

        public void SetMarginBottom(float marginBottom)
            => ScreenMargin = new Vector4(ScreenMargin.X, ScreenMargin.Y, ScreenMargin.Z, marginBottom);
    }
}
