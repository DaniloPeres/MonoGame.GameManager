using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Stretches a texture to any size without deforming its corners (nine-slice scaling): the corners keep their
    /// size, the edges stretch in one direction and the center in both. Used for panels, buttons and dialogs.
    /// </summary>
    /// <example>
    /// <code>
    /// new NineSliceImage(panelTexture, new Thickness(16), new Vector2(100, 100), new Vector2(400, 250))
    ///     .AddToScreen();
    /// </code>
    /// </example>
    public class NineSliceImage : ScalableControlAbstract<NineSliceImage>
    {
        /// <param name="texture">The texture (or atlas, see <see cref="SourceRectangle"/>).</param>
        /// <param name="border">The size of the corners in pixels of the texture.</param>
        /// <param name="position">The position.</param>
        /// <param name="size">The size of the control.</param>
        public NineSliceImage(Texture2D texture, Thickness border, Vector2 position, Vector2 size)
        {
            Texture = texture;
            Border = border;
            PositionAnchor = position;
            Size = size;
        }

        public Texture2D Texture { get; set; }

        /// <summary>The part of the texture used (null = the whole texture).</summary>
        public Rectangle? SourceRectangle { get; set; }

        /// <summary>The size of the corners, in pixels of the texture.</summary>
        public Thickness Border { get; set; }

        /// <summary>When false, the center is not drawn (a frame).</summary>
        public bool DrawCenter { get; set; } = true;

        public NineSliceImage SetTexture(Texture2D texture, Rectangle? sourceRectangle = null)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
            return this;
        }

        public NineSliceImage SetBorder(Thickness border)
        {
            Border = border;
            return this;
        }

        public NineSliceImage SetSize(Vector2 size)
        {
            Size = size;
            return this;
        }

        public NineSliceImage SetDrawCenter(bool drawCenter)
        {
            DrawCenter = drawCenter;
            return this;
        }

        /// <inheritdoc />
        protected override int? GetContentSignature() => HashCode.Combine(Texture, SourceRectangle, Border, DrawCenter);

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null || Texture.IsDisposed)
                return;

            var source = SourceRectangle ?? Texture.Bounds;

            // Source slices in texture pixels, destination slices in local units (the corners shrink when the control
            // is smaller than them).
            Span<int> sourceX = stackalloc int[4];
            Span<int> sourceY = stackalloc int[4];
            Span<float> destinationX = stackalloc float[4];
            Span<float> destinationY = stackalloc float[4];
            NineSlice.ComputeSlices(source, Border, SizeWithoutScale, sourceX, sourceY, destinationX, destinationY);

            var color = DrawColor;
            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    if (row == 1 && column == 1 && !DrawCenter)
                        continue;

                    var sourceRectangle = new Rectangle(sourceX[column], sourceY[row], sourceX[column + 1] - sourceX[column], sourceY[row + 1] - sourceY[row]);
                    var area = new RectangleF(destinationX[column], destinationY[row],
                        destinationX[column + 1] - destinationX[column], destinationY[row + 1] - destinationY[row]);
                    DrawLocalTexture(spriteBatch, Texture, sourceRectangle, area, color);
                }
            }
        }
    }
}
