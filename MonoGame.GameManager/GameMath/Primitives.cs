using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// <see cref="SpriteBatch"/> extensions to draw lines, rectangles, circles and polygons.
    /// They can be used inside the <c>Draw</c> method of a custom control or of a game.
    /// </summary>
    public static class Primitives
    {
        private const int FilledCircleTextureDiameter = 256;
        private static Texture2D filledCircleTexture;

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 1f, float layerDepth = 0f)
        {
            var delta = end - start;
            var length = delta.Length();
            if (length <= 0f || thickness <= 0f)
                return;

            spriteBatch.Draw(ShapeExtension.WhitePixelTexture, start, null, color, delta.ToAngle(), new Vector2(0f, 0.5f),
                new Vector2(length, thickness), SpriteEffects.None, layerDepth);
        }

        public static void FillRectangle(this SpriteBatch spriteBatch, RectangleF rectangle, Color color, float layerDepth = 0f)
        {
            if (rectangle.IsEmpty)
                return;
            spriteBatch.Draw(ShapeExtension.WhitePixelTexture, rectangle.Position, null, color, 0f, Vector2.Zero, rectangle.Size, SpriteEffects.None, layerDepth);
        }

        /// <summary>Draws the outline of a rectangle; the border is drawn inside the rectangle.</summary>
        public static void DrawRectangle(this SpriteBatch spriteBatch, RectangleF rectangle, Color color, float thickness = 1f, float layerDepth = 0f)
        {
            if (rectangle.IsEmpty || thickness <= 0f)
                return;

            thickness = Math.Min(thickness, Math.Min(rectangle.Width, rectangle.Height) / 2f);
            spriteBatch.FillRectangle(new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, thickness), color, layerDepth);
            spriteBatch.FillRectangle(new RectangleF(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color, layerDepth);
            spriteBatch.FillRectangle(new RectangleF(rectangle.X, rectangle.Y + thickness, thickness, rectangle.Height - thickness * 2f), color, layerDepth);
            spriteBatch.FillRectangle(new RectangleF(rectangle.Right - thickness, rectangle.Y + thickness, thickness, rectangle.Height - thickness * 2f), color, layerDepth);
        }

        /// <summary>Draws the outline of a circle with line segments.</summary>
        public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color, float thickness = 1f, int segments = 32, float layerDepth = 0f)
        {
            if (radius <= 0f)
                return;

            segments = Math.Max(3, segments);
            var previous = center + new Vector2(radius, 0f);
            for (var i = 1; i <= segments; i++)
            {
                var point = center + MathUtils.AngleToVector(MathHelper.TwoPi * i / segments, radius);
                spriteBatch.DrawLine(previous, point, color, thickness, layerDepth);
                previous = point;
            }
        }

        /// <summary>Draws a filled, anti-aliased circle.</summary>
        public static void FillCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color, float layerDepth = 0f)
        {
            if (radius <= 0f)
                return;

            var texture = GetFilledCircleTexture(spriteBatch.GraphicsDevice);
            var scale = radius * 2f / FilledCircleTextureDiameter;
            spriteBatch.Draw(texture, center, null, color, 0f, new Vector2(FilledCircleTextureDiameter / 2f), scale, SpriteEffects.None, layerDepth);
        }

        /// <summary>Draws the outline of a polygon.</summary>
        public static void DrawPolygon(this SpriteBatch spriteBatch, IList<Vector2> points, Color color, float thickness = 1f, bool closed = true, float layerDepth = 0f)
        {
            if (points == null || points.Count < 2)
                return;

            for (var i = 0; i < points.Count - 1; i++)
                spriteBatch.DrawLine(points[i], points[i + 1], color, thickness, layerDepth);

            if (closed && points.Count > 2)
                spriteBatch.DrawLine(points[points.Count - 1], points[0], color, thickness, layerDepth);
        }

        /// <summary>Disposes the shared textures (they are recreated on demand).</summary>
        internal static void Reset()
        {
            filledCircleTexture?.Dispose();
            filledCircleTexture = null;
        }

        private static Texture2D GetFilledCircleTexture(GraphicsDevice graphicsDevice)
        {
            if (filledCircleTexture == null || filledCircleTexture.IsDisposed || filledCircleTexture.GraphicsDevice != graphicsDevice)
                filledCircleTexture = TextureFactory.CreateCircle(graphicsDevice, FilledCircleTextureDiameter, Color.White);
            return filledCircleTexture;
        }
    }
}
