using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Owns the root panel of the stage (which contains the root panel of every open screen), dispatches pointer
    /// input to the controls and draws the control tree with a stack of <see cref="SpriteBatchState"/>.
    /// </summary>
    public class ControlManager : IDisposable
    {
        private readonly Stack<SpriteBatchState> states = new Stack<SpriteBatchState>();
        private readonly RasterizerState scissorRasterizerState;
        private bool isDisposed;

        public ControlManager(GraphicsDevice graphicsDevice, Point screenSize)
        {
            GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            SpriteBatch = new SpriteBatch(graphicsDevice);
            scissorRasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };

            RootPanel = new Panel(Vector2.Zero, screenSize.ToVector2());
            RootPanel.IsStageRoot = true;
            MouseEventHandler = new ControlMouseEventHandler(RootPanel);
            BaseState = SpriteBatchState.Default;
        }

        public GraphicsDevice GraphicsDevice { get; }

        /// <summary>The sprite batch used to draw the controls.</summary>
        public SpriteBatch SpriteBatch { get; }

        /// <summary>The root of the stage. Each open screen adds its own root panel to it.</summary>
        public Panel RootPanel { get; }

        /// <summary>Delivers pointer input (mouse and touch) to the controls.</summary>
        public ControlMouseEventHandler MouseEventHandler { get; }

        /// <summary>The state used to begin the sprite batch every frame.</summary>
        public SpriteBatchState BaseState { get; set; }

        /// <summary>The state of the sprite batch at the current point of the drawing.</summary>
        public SpriteBatchState CurrentState => states.Count > 0 ? states.Peek() : BaseState;

        /// <summary>Fires the update event of every control.</summary>
        public void Update(GameTime gameTime) => RootPanel.FireOnUpdateEvent(gameTime);

        /// <summary>Updates the layout of every control before drawing.</summary>
        public void OnBeforeDraw() => RootPanel.OnBeforeDraw();

        public void Draw() => Draw(ServiceProvider.ScreenManager?.ScreenBackgroundColor ?? Color.Black);

        /// <summary>Clears the render target and draws the whole control tree.</summary>
        public void Draw(Color clearColor)
        {
            GraphicsDevice.Clear(clearColor);
            states.Clear();
            states.Push(BaseState);
            Begin(SpriteBatch, BaseState);
            if (RootPanel.IsVisible)
                RootPanel.Draw(SpriteBatch);
            SpriteBatch.End();
            states.Clear();
        }

        /// <summary>
        /// Ends the current batch and begins a new one with <paramref name="state"/>. Call <see cref="PopState"/>
        /// when done.
        /// </summary>
        public void PushState(SpriteBatch spriteBatch, SpriteBatchState state)
        {
            spriteBatch.End();
            states.Push(state);
            Begin(spriteBatch, state);
        }

        /// <summary>Ends the current batch and begins again with the previous state.</summary>
        public void PopState(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            if (states.Count > 0)
                states.Pop();
            Begin(spriteBatch, CurrentState);
        }

        /// <summary>
        /// Pushes a clipping area (in the coordinate space of the current transformation) intersected with the
        /// current clipping area. Returns false, without pushing anything, when the result is empty.
        /// </summary>
        public bool TryPushClip(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (!TryGetClipArea(bounds, out var area))
                return false;

            PushState(spriteBatch, CurrentState.WithScissor(area));
            return true;
        }

        /// <summary>
        /// Computes the clipping area of <paramref name="bounds"/> (in the coordinate space of the current
        /// transformation) intersected with the current clipping area. Returns false when it is empty.
        /// </summary>
        public bool TryGetClipArea(Rectangle bounds, out Rectangle area)
        {
            var state = CurrentState;
            area = state.TransformMatrix.HasValue ? TransformBounds(bounds, state.TransformMatrix.Value) : bounds;
            area = Rectangle.Intersect(area, state.ScissorRectangle ?? GraphicsDevice.Viewport.Bounds);
            return area.Width > 0 && area.Height > 0;
        }

        /// <summary>Begins a batch on another render target (the state stack is shared).</summary>
        public void BeginOffscreen(SpriteBatch spriteBatch, SpriteBatchState state)
        {
            states.Push(state);
            Begin(spriteBatch, state);
        }

        /// <summary>Ends a batch started with <see cref="BeginOffscreen"/>.</summary>
        public void EndOffscreen(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            if (states.Count > 0)
                states.Pop();
        }

        /// <summary>Returns the axis-aligned bounds of a rectangle transformed by a matrix.</summary>
        public static Rectangle TransformBounds(Rectangle bounds, Matrix matrix)
        {
            var topLeft = Vector2.Transform(new Vector2(bounds.Left, bounds.Top), matrix);
            var topRight = Vector2.Transform(new Vector2(bounds.Right, bounds.Top), matrix);
            var bottomLeft = Vector2.Transform(new Vector2(bounds.Left, bounds.Bottom), matrix);
            var bottomRight = Vector2.Transform(new Vector2(bounds.Right, bounds.Bottom), matrix);

            var min = Vector2.Min(Vector2.Min(topLeft, topRight), Vector2.Min(bottomLeft, bottomRight));
            var max = Vector2.Max(Vector2.Max(topLeft, topRight), Vector2.Max(bottomLeft, bottomRight));

            var left = (int)Math.Floor(min.X);
            var top = (int)Math.Floor(min.Y);
            return new Rectangle(left, top, (int)Math.Ceiling(max.X) - left, (int)Math.Ceiling(max.Y) - top);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            RootPanel.Dispose();
            SpriteBatch.Dispose();
            scissorRasterizerState.Dispose();
        }

        private void Begin(SpriteBatch spriteBatch, SpriteBatchState state)
        {
            var rasterizerState = state.RasterizerState;
            if (state.ScissorRectangle.HasValue)
            {
                GraphicsDevice.ScissorRectangle = state.ScissorRectangle.Value;
                rasterizerState = scissorRasterizerState;
            }

            spriteBatch.Begin(state.SortMode, state.BlendState, state.SamplerState, state.DepthStencilState, rasterizerState, state.Effect, state.TransformMatrix);
        }
    }
}
