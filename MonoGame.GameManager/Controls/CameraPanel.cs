using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Cameras;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Controls.Shading;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A panel that shows its children through a <see cref="Camera2D"/>: the children are the world (positioned in
    /// world coordinates) and the panel is the viewport. Pointer input is converted to world coordinates, so the
    /// children can be clicked normally, and children outside the view are not drawn.
    /// </summary>
    /// <example>
    /// <code>
    /// var world = new CameraPanel(Vector2.Zero, ScreenManager.ScreenSize.ToVector2()).AddToScreen();
    /// world.AddChild(tileMap);
    /// world.AddChild(player);
    /// world.Camera.Bounds = new RectangleF(0, 0, 4000, 2000);
    /// world.Camera.Follow(() => player.PositionAnchor, lerpSpeed: 6f);
    /// </code>
    /// </example>
    public class CameraPanel : ContainerAbstract<CameraPanel>
    {
        private float pendingDeltaSeconds;

        public CameraPanel(Rectangle destinationRectangle)
            : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public CameraPanel(Vector2 position, Vector2 size)
            : base(position, size)
        {
            Camera = new Camera2D(size);
            HideOverflow = true;
            AddOnUpdateEvent(AccumulateTime);
        }

        /// <summary>The camera of the panel.</summary>
        public Camera2D Camera { get; }

        /// <summary>When true (default), the children outside of the view are not drawn.</summary>
        public bool CullChildren { get; set; } = true;

        /// <summary>Converts a point of the parent's coordinate space (eg: the screen) to world coordinates.</summary>
        public Vector2 ScreenToWorld(Vector2 point) => Camera.ScreenToWorld(point - DestinationRectangle.Location.ToVector2());

        /// <summary>Converts world coordinates to the parent's coordinate space (eg: the screen).</summary>
        public Vector2 WorldToScreen(Vector2 worldPosition) => Camera.WorldToScreen(worldPosition) + DestinationRectangle.Location.ToVector2();

        /// <inheritdoc />
        public override Point TransformPointToLocal(Point point)
        {
            var offset = DestinationRectangle.Location.ToVector2();
            var world = Camera.ScreenToWorld(point.ToVector2() - offset) + offset;
            return new Point((int)System.Math.Floor(world.X), (int)System.Math.Floor(world.Y));
        }

        /// <summary>
        /// Updates the camera after the game logic of the frame (follow and shake), so it never draws a target one
        /// frame behind.
        /// </summary>
        public override void OnBeforeDraw()
        {
            Camera.ViewportSize = Size;
            Camera.Update(pendingDeltaSeconds);
            pendingDeltaSeconds = 0f;
            base.OnBeforeDraw();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var controlManager = ServiceProvider.ControlManager;
            if (controlManager == null)
            {
                DrawChildren(spriteBatch);
                return;
            }

            Camera.ViewportSize = Size;
            var state = controlManager.CurrentState;
            if (HideOverflow)
            {
                if (!controlManager.TryGetClipArea(GetClipBounds(), out var clipArea))
                    return;
                state = state.WithScissor(clipArea);
            }

            var offset = DestinationRectangle.Location.ToVector2();
            var view = Matrix.CreateTranslation(-offset.X, -offset.Y, 0f)
                * Camera.GetViewMatrix()
                * Matrix.CreateTranslation(offset.X, offset.Y, 0f);
            state = state.WithTransform(state.TransformMatrix.HasValue ? view * state.TransformMatrix.Value : view);

            controlManager.PushState(spriteBatch, state);
            DrawVisibleChildren(spriteBatch, offset);
            controlManager.PopState(spriteBatch);
        }

        /// <inheritdoc />
        protected override bool UsesRenderTargetClipping => false;

        private void AccumulateTime(GameTime gameTime) => pendingDeltaSeconds += GetScaledDeltaSeconds(gameTime);

        private void DrawVisibleChildren(SpriteBatch spriteBatch, Vector2 offset)
        {
            var visibleArea = Camera.GetVisibleArea().Offset(offset);
            foreach (var child in Children)
            {
                if (!child.IsVisible || !ReferenceEquals(child.Parent, this))
                    continue;
                if (CullChildren && child.Rotation == 0f && !(child is IContainer) && !IsUnbounded(child) && !visibleArea.Intersects(GetBounds(child)))
                    continue;
                ShadingRenderer.DrawControl(child, spriteBatch);
            }
        }

        /// <summary>Controls without a size (eg: particle emitters) draw outside of their bounds, so they are never culled.</summary>
        private static bool IsUnbounded(IControl control)
        {
            var bounds = control.DestinationRectangle;
            return bounds.Width <= 0 || bounds.Height <= 0;
        }

        private static RectangleF GetBounds(IControl control)
        {
            var bounds = control.DestinationRectangle;
            var origin = control.Origin;
            var area = new RectangleF(bounds.X - origin.X, bounds.Y - origin.Y, bounds.Width, bounds.Height);

            // Shadows and glows reach outside the control: it is drawn while they can be seen.
            var margin = control is IShadingHost host ? host.ShadingMargin : 0f;
            return margin > 0f ? area.Inflate(margin, margin) : area;
        }
    }
}
