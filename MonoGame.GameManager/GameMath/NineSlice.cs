using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Layout;
using System;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Nine-slice scaling: stretches a texture to any size without deforming its corners. The corners keep their size,
    /// the edges stretch in one direction and the center in both. When the destination is smaller than the corners,
    /// they shrink proportionally.
    /// </summary>
    /// <remarks>
    /// The border of the texture (in texture pixels) and the border of the destination can be different: a texture
    /// generated at twice the resolution can be drawn with half of its border, so it stays sharp when scaled up.
    /// </remarks>
    public static class NineSlice
    {
        /// <summary>
        /// Computes the slices: the X and Y coordinates (4 values each) of the columns and rows in the texture and in
        /// the destination (relative to its top-left corner).
        /// </summary>
        /// <param name="source">The part of the texture used.</param>
        /// <param name="border">The size of the corners in texture pixels; the destination uses the same size.</param>
        /// <param name="size">The size of the destination.</param>
        /// <param name="sourceX">Receives the 4 column coordinates in the texture.</param>
        /// <param name="sourceY">Receives the 4 row coordinates in the texture.</param>
        /// <param name="destinationX">Receives the 4 column coordinates in the destination.</param>
        /// <param name="destinationY">Receives the 4 row coordinates in the destination.</param>
        public static void ComputeSlices(Rectangle source, Thickness border, Vector2 size,
            Span<int> sourceX, Span<int> sourceY, Span<float> destinationX, Span<float> destinationY)
        {
            var clamped = ClampBorder(source, border);
            ComputeSlices(source, clamped, size, clamped, sourceX, sourceY, destinationX, destinationY);
        }

        /// <summary>
        /// Computes the slices with a border in the texture and another one in the destination (see
        /// <see cref="ComputeSlices(Rectangle, Thickness, Vector2, Span{int}, Span{int}, Span{float}, Span{float})"/>).
        /// </summary>
        public static void ComputeSlices(Rectangle source, Thickness sourceBorder, Vector2 size, Thickness destinationBorder,
            Span<int> sourceX, Span<int> sourceY, Span<float> destinationX, Span<float> destinationY)
        {
            var clamped = ClampBorder(source, sourceBorder);
            var left = (int)clamped.Left;
            var right = (int)clamped.Right;
            var top = (int)clamped.Top;
            var bottom = (int)clamped.Bottom;
            sourceX[0] = source.X;
            sourceX[1] = source.X + left;
            sourceX[2] = source.Right - right;
            sourceX[3] = source.Right;
            sourceY[0] = source.Y;
            sourceY[1] = source.Y + top;
            sourceY[2] = source.Bottom - bottom;
            sourceY[3] = source.Bottom;

            var destinationLeft = Math.Max(0f, destinationBorder.Left);
            var destinationRight = Math.Max(0f, destinationBorder.Right);
            var destinationTop = Math.Max(0f, destinationBorder.Top);
            var destinationBottom = Math.Max(0f, destinationBorder.Bottom);
            var width = Math.Max(0f, size.X);
            var height = Math.Max(0f, size.Y);

            // The corners shrink when the destination is smaller than them.
            var horizontalFactor = destinationLeft + destinationRight > width && destinationLeft + destinationRight > 0f ? width / (destinationLeft + destinationRight) : 1f;
            var verticalFactor = destinationTop + destinationBottom > height && destinationTop + destinationBottom > 0f ? height / (destinationTop + destinationBottom) : 1f;
            destinationX[0] = 0f;
            destinationX[1] = destinationLeft * horizontalFactor;
            destinationX[2] = width - destinationRight * horizontalFactor;
            destinationX[3] = width;
            destinationY[0] = 0f;
            destinationY[1] = destinationTop * verticalFactor;
            destinationY[2] = height - destinationBottom * verticalFactor;
            destinationY[3] = height;
        }

        /// <summary>Draws a texture with nine-slice scaling; the destination uses the border of the texture.</summary>
        public static void DrawNineSlice(this SpriteBatch spriteBatch, Texture2D texture, Rectangle? sourceRectangle, Thickness border,
            RectangleF destination, Color color, bool drawCenter = true, float layerDepth = 0f)
        {
            if (texture == null || texture.IsDisposed)
                return;
            var clamped = ClampBorder(sourceRectangle ?? texture.Bounds, border);
            DrawNineSlice(spriteBatch, texture, sourceRectangle, clamped, destination, clamped, color, drawCenter, layerDepth);
        }

        /// <summary>
        /// Draws a texture with nine-slice scaling, with a border in the texture (pixels) and another one in the
        /// destination (units of the destination).
        /// </summary>
        public static void DrawNineSlice(this SpriteBatch spriteBatch, Texture2D texture, Rectangle? sourceRectangle, Thickness sourceBorder,
            RectangleF destination, Thickness destinationBorder, Color color, bool drawCenter = true, float layerDepth = 0f)
        {
            if (texture == null || texture.IsDisposed || destination.Width <= 0f || destination.Height <= 0f)
                return;

            Span<int> sourceX = stackalloc int[4];
            Span<int> sourceY = stackalloc int[4];
            Span<float> destinationX = stackalloc float[4];
            Span<float> destinationY = stackalloc float[4];
            var source = sourceRectangle ?? texture.Bounds;
            ComputeSlices(source, sourceBorder, destination.Size, destinationBorder, sourceX, sourceY, destinationX, destinationY);

            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    if (row == 1 && column == 1 && !drawCenter)
                        continue;

                    var sourceWidth = sourceX[column + 1] - sourceX[column];
                    var sourceHeight = sourceY[row + 1] - sourceY[row];
                    var width = destinationX[column + 1] - destinationX[column];
                    var height = destinationY[row + 1] - destinationY[row];
                    if (sourceWidth <= 0 || sourceHeight <= 0 || width <= 0f || height <= 0f)
                        continue;

                    spriteBatch.Draw(texture,
                        new Vector2(destination.X + destinationX[column], destination.Y + destinationY[row]),
                        new Rectangle(sourceX[column], sourceY[row], sourceWidth, sourceHeight),
                        color, 0f, Vector2.Zero, new Vector2(width / sourceWidth, height / sourceHeight), SpriteEffects.None, layerDepth);
                }
            }
        }

        /// <summary>The border limited to the size of the source, in whole pixels.</summary>
        private static Thickness ClampBorder(Rectangle source, Thickness border)
        {
            var left = (int)MathHelper.Clamp(border.Left, 0, source.Width);
            var right = (int)MathHelper.Clamp(border.Right, 0, source.Width - left);
            var top = (int)MathHelper.Clamp(border.Top, 0, source.Height);
            var bottom = (int)MathHelper.Clamp(border.Bottom, 0, source.Height - top);
            return new Thickness(left, top, right, bottom);
        }
    }
}
