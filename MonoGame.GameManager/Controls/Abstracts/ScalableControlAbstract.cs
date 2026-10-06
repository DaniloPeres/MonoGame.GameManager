using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls.Abstracts
{
    /// <summary>
    /// Base class of the controls that have a scale. The size and the origin are stored without the scale
    /// (<see cref="SizeWithoutScale"/>, <see cref="OriginWithoutScale"/>) and returned scaled by the nested scale.
    /// </summary>
    /// <remarks>
    /// The "local space" of a control is its unscaled space, where (0, 0) is the top-left corner of the control and
    /// <see cref="SizeWithoutScale"/> its bottom-right corner. The helpers <see cref="DrawLocalTexture"/>,
    /// <see cref="DrawLocalRectangle"/> and <see cref="ToLocalPosition(Vector2)"/> work in that space and follow the
    /// position, origin, scale and rotation of the control.
    /// </remarks>
    public abstract class ScalableControlAbstract<TControl> : Control<TControl>, IScalableControl where TControl : IScalableControl
    {
        Vector2 IScalableControl.Scale {
            get => Scale;
            set => Scale = value;
        }
        public virtual Vector2 Scale
        {
            get => scale;
            set
            {
                scale = value;
                MarkAsDirty();
            }
        }
        private Vector2 scale = Vector2.One;

        public override Vector2 Size
        {
            get => base.Size * NestedScale;
            set => base.Size = value;
        }

        public Vector2 SizeWithoutScale => base.Size;

        public override Vector2 Origin
        {
            get => base.Origin * NestedScale;
            set => base.Origin = value;
        }

        Vector2 IScalableControl.OriginWithoutScale {
            get => OriginWithoutScale;
            set => OriginWithoutScale = value;
        }
        public virtual Vector2 OriginWithoutScale
        {
            get => base.Origin;
            set => base.Origin = value;
        }

        protected override Vector2 CalculateSize() => SizeWithoutScale;
        public override Vector2 CalculateNestedScale() => base.CalculateNestedScale() * Scale;
        IScalableControl IScalableControl.SetScale(float scale) => SetScale(scale);
        public TControl SetScale(float scale) => SetScale(new Vector2(scale));
        IScalableControl IScalableControl.SetScale(Vector2 scale) => SetScale(scale);
        public TControl SetScale(Vector2 scale)
        {
            Scale = scale;
            return (TControl)(object)this;
        }

        public override TControl SetOriginRate(Vector2 originRate) => SetOriginRate(originRate, SizeWithoutScale);

        /// <summary>
        /// Converts a point of the parent's coordinate space (eg: a pointer position) to the local space of the
        /// control. The rotation is considered.
        /// </summary>
        public Vector2 ToLocalPosition(Point point) => ToLocalPosition(point.ToVector2());

        /// <inheritdoc cref="ToLocalPosition(Point)"/>
        public Vector2 ToLocalPosition(Vector2 point)
        {
            var offset = point - DestinationRectangle.Location.ToVector2();
            if (Rotation != 0f)
                offset = offset.Rotated(-Rotation);

            var scale = NestedScale;
            var local = new Vector2(scale.X != 0f ? offset.X / scale.X : 0f, scale.Y != 0f ? offset.Y / scale.Y : 0f);
            return local + OriginWithoutScale;
        }

        /// <summary>Converts a point of the local space to the parent's coordinate space (inverse of <see cref="ToLocalPosition(Vector2)"/>).</summary>
        public Vector2 LocalToParentPosition(Vector2 localPoint)
        {
            var offset = (localPoint - OriginWithoutScale) * NestedScale;
            if (Rotation != 0f)
                offset = offset.Rotated(Rotation);
            return DestinationRectangle.Location.ToVector2() + offset;
        }

        /// <summary>
        /// Draws a texture (or a part of it) stretched over an area given in local space, following the position,
        /// origin, scale and rotation of the control.
        /// </summary>
        protected void DrawLocalTexture(SpriteBatch spriteBatch, Texture2D texture, Rectangle? sourceRectangle, RectangleF localArea, Color color)
        {
            if (texture == null || texture.IsDisposed || localArea.Width <= 0f || localArea.Height <= 0f)
                return;

            var source = sourceRectangle ?? texture.Bounds;
            if (source.Width <= 0 || source.Height <= 0)
                return;

            var sourceSize = new Vector2(source.Width, source.Height);
            var areaSize = new Vector2(localArea.Width, localArea.Height);
            var origin = (OriginWithoutScale - new Vector2(localArea.X, localArea.Y)) / areaSize * sourceSize;
            var scale = areaSize / sourceSize * NestedScale;
            spriteBatch.Draw(texture, GetPosition(), source, color, Rotation, origin, scale, SpriteEffects.None, LayerDepthDraw);
        }

        /// <summary>Draws a solid rectangle given in local space (see <see cref="DrawLocalTexture"/>).</summary>
        protected void DrawLocalRectangle(SpriteBatch spriteBatch, RectangleF localArea, Color color)
        {
            if (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0)
                return; // fully transparent
            DrawLocalTexture(spriteBatch, ShapeExtension.WhitePixelTexture, null, localArea, color);
        }

        /// <summary>Draws the outline of a rectangle given in local space, inside the rectangle.</summary>
        protected void DrawLocalBorder(SpriteBatch spriteBatch, RectangleF localArea, Color color, float thickness)
        {
            if (thickness <= 0f)
                return;

            var horizontal = Math.Min(thickness, localArea.Height / 2f);
            var vertical = Math.Min(thickness, localArea.Width / 2f);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.X, localArea.Y, localArea.Width, horizontal), color);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.X, localArea.Bottom - horizontal, localArea.Width, horizontal), color);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.X, localArea.Y + horizontal, vertical, localArea.Height - horizontal * 2f), color);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.Right - vertical, localArea.Y + horizontal, vertical, localArea.Height - horizontal * 2f), color);
        }

        /// <summary>The whole area of the control in local space.</summary>
        protected RectangleF LocalBounds => new RectangleF(Vector2.Zero, SizeWithoutScale);
    }
}
