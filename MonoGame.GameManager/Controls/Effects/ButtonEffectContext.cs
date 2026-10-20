using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// What a <see cref="ButtonEffect"/> knows about its button when it updates and draws: the shape (in the local,
    /// unscaled space of the button), the visual state, the time and the opacity. One instance is reused by each button.
    /// </summary>
    /// <remarks>
    /// Effects draw in the local space of the button with the drawing methods of the context (<c>Draw</c>,
    /// <see cref="DrawNineSlice"/>, <see cref="FillRectangle"/>). They place the sprites on the screen themselves, so the
    /// effects of many buttons are drawn in the sprite batch of the screen without starting a batch per button, and they
    /// draw the lights of <see cref="ButtonEffectBlend.Light"/> effects with an alpha of 0, which adds their color under
    /// the premultiplied alpha blend (exactly like <see cref="ButtonEffectResources.LightBlendState"/>), so lights and
    /// painted effects share one batch too.
    /// </remarks>
    public sealed class ButtonEffectContext
    {
        private float perimeterLength = -1f;
        private float requestedCornerRadius = -1f;
        private bool placesSprites;
        private Vector2 placePosition;
        private Vector2 placeOrigin;
        private float placeScale = 1f;
        private float placeRotation;
        private float placeCos = 1f;
        private float placeSin;

        /// <summary>The shape of the effects in local units: the area of the button, reduced by its effect padding.</summary>
        public RectangleF Bounds { get; private set; }

        /// <summary>The corner radius of the shape, limited to half of its smallest side.</summary>
        public float CornerRadius { get; private set; }

        /// <summary>The visual state of the button.</summary>
        public ButtonVisualState State { get; internal set; }

        /// <summary>The time of the effects of the button, in seconds (it starts at a random value for each button).</summary>
        public float Time { get; internal set; }

        /// <summary>The duration of the current frame, in seconds (0 while drawing).</summary>
        public float DeltaSeconds { get; internal set; }

        /// <summary>The opacity of the button (with the opacity of its parents).</summary>
        public float Opacity { get; internal set; } = 1f;

        /// <summary>The scale of the button on the screen (with the scale of its parents).</summary>
        public Vector2 Scale { get; internal set; } = Vector2.One;

        /// <summary>The random generator of the game.</summary>
        public IRandom Random { get; internal set; }

        /// <summary>The button.</summary>
        public IControl Host { get; internal set; }

        /// <summary>The rotation of the button, in radians.</summary>
        public float Rotation { get; internal set; }

        /// <summary>The label created by the <c>SetText</c> method of the button, or null.</summary>
        public Label TextLabel => HostInternal?.TextLabel;

        /// <summary>The background color of the button in its current state, or null when it is drawn with a texture.</summary>
        public Color? BackgroundColor => HostInternal?.EffectBackgroundColor;

        /// <summary>
        /// A darker (<paramref name="amount"/> above 0) or lighter (below 0) version of the background color of the
        /// button, or <paramref name="fallback"/> when it has no background color.
        /// </summary>
        public Color GetShadeOfBackground(float amount, Color fallback)
        {
            var background = BackgroundColor;
            if (!background.HasValue)
                return fallback;

            var color = background.Value;
            return amount >= 0f
                ? Color.Lerp(color, new Color(0, 0, 0, (int)color.A), Math.Min(1f, amount))
                : Color.Lerp(color, new Color((int)color.A, (int)color.A, (int)color.A, (int)color.A), Math.Min(1f, -amount));
        }

        internal IButtonEffectHost HostInternal { get; set; }

        /// <summary>
        /// Converts a point of the screen space of the controls (eg: the position of a child) to the local space of the
        /// button.
        /// </summary>
        public Vector2 ToLocalPosition(Vector2 point) => HostInternal != null ? HostInternal.ToLocalPosition(point) : point;

        /// <summary>
        /// The resolution of the generated textures: 1 when the button is drawn at its size, more when it is scaled up,
        /// so the lights stay sharp.
        /// </summary>
        public int TextureDensity => ButtonEffectResources.GetDensity(Scale);

        /// <summary>The length of the outline of the shape.</summary>
        public float PerimeterLength
        {
            get
            {
                if (perimeterLength < 0f)
                    perimeterLength = RoundedRectanglePath.PerimeterLength(Bounds, CornerRadius);
                return perimeterLength;
            }
        }

        /// <summary>The point of the outline of the shape at a distance from its start (see <see cref="RoundedRectanglePath"/>).</summary>
        public Vector2 PointOnBorder(float distance, out Vector2 outwardNormal)
            => RoundedRectanglePath.PointAt(Bounds, CornerRadius, distance, out outwardNormal);

        /// <summary>A remarkable point of the outline of the shape: the middle of an edge or of a corner, or the center.</summary>
        public Vector2 AnchorPoint(Anchor anchor, out Vector2 outwardNormal)
            => RoundedRectanglePath.AnchorPoint(Bounds, CornerRadius, anchor, out outwardNormal);

        /// <summary>True while the lights are drawn with an alpha of 0 (the effect drawn is a light and the batch uses the alpha blend).</summary>
        internal bool LightsWithoutAlpha { get; set; }

        /// <summary>Draws a texture in the local space of the button (like <c>SpriteBatch.Draw</c>).</summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None)
        {
            if (LightsWithoutAlpha)
                color.A = 0;
            if (placesSprites)
            {
                position = ToBatchPosition(position);
                rotation += placeRotation;
                scale *= placeScale;
            }

            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, 0f);
        }

        /// <summary>Draws a texture in the local space of the button, with the same scale on both axes.</summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color,
            float rotation, Vector2 origin, float scale)
            => Draw(spriteBatch, texture, position, sourceRectangle, color, rotation, origin, new Vector2(scale));

        /// <summary>Draws an image of the effects in the local space of the button.</summary>
        public void Draw(SpriteBatch spriteBatch, in EffectSprite sprite, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale,
            SpriteEffects effects = SpriteEffects.None)
        {
            if (sprite.IsValid)
                Draw(spriteBatch, sprite.Texture, position, sprite.Source, color, rotation, origin, scale, effects);
        }

        /// <summary>Draws an image of the effects in the local space of the button, with the same scale on both axes.</summary>
        public void Draw(SpriteBatch spriteBatch, in EffectSprite sprite, Vector2 position, Color color, float rotation, Vector2 origin, float scale)
        {
            if (sprite.IsValid)
                Draw(spriteBatch, sprite.Texture, position, sprite.Source, color, rotation, origin, new Vector2(scale));
        }

        /// <summary>Fills a rectangle of the local space of the button with a color.</summary>
        public void FillRectangle(SpriteBatch spriteBatch, RectangleF rectangle, Color color)
        {
            if (rectangle.IsEmpty)
                return;
            var pixel = ButtonEffectResources.WhitePixel;
            Draw(spriteBatch, pixel, rectangle.Position, color, 0f, Vector2.Zero, rectangle.Size);
        }

        /// <summary>Draws a texture with nine-slice scaling in the local space of the button (see <see cref="NineSlice"/>).</summary>
        public void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture, Rectangle? sourceRectangle, Thickness sourceBorder,
            RectangleF destination, Thickness destinationBorder, Color color)
        {
            if (texture == null || texture.IsDisposed || destination.Width <= 0f || destination.Height <= 0f)
                return;

            Span<int> sourceX = stackalloc int[4];
            Span<int> sourceY = stackalloc int[4];
            Span<float> destinationX = stackalloc float[4];
            Span<float> destinationY = stackalloc float[4];
            var source = sourceRectangle ?? texture.Bounds;
            NineSlice.ComputeSlices(source, sourceBorder, destination.Size, destinationBorder, sourceX, sourceY, destinationX, destinationY);

            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var sourceWidth = sourceX[column + 1] - sourceX[column];
                    var sourceHeight = sourceY[row + 1] - sourceY[row];
                    var width = destinationX[column + 1] - destinationX[column];
                    var height = destinationY[row + 1] - destinationY[row];
                    if (sourceWidth <= 0 || sourceHeight <= 0 || width <= 0f || height <= 0f)
                        continue;

                    Draw(spriteBatch, texture, new Vector2(destination.X + destinationX[column], destination.Y + destinationY[row]),
                        new Rectangle(sourceX[column], sourceY[row], sourceWidth, sourceHeight), color, 0f, Vector2.Zero,
                        new Vector2(width / sourceWidth, height / sourceHeight));
                }
            }
        }

        /// <summary>
        /// Makes the drawing methods place the sprites on the batch themselves: <paramref name="position"/> is where the
        /// <paramref name="origin"/> of the button goes, with its (uniform) scale and its rotation.
        /// </summary>
        internal void BeginPlacing(Vector2 position, Vector2 origin, float scale, float rotation)
        {
            placesSprites = true;
            placePosition = position;
            placeOrigin = origin;
            placeScale = scale;
            placeRotation = rotation;
            placeCos = MathF.Cos(rotation);
            placeSin = MathF.Sin(rotation);
        }

        /// <summary>The drawing methods draw in the coordinate space of the batch again.</summary>
        internal void EndPlacing()
        {
            placesSprites = false;
        }

        private Vector2 ToBatchPosition(Vector2 point)
        {
            var x = (point.X - placeOrigin.X) * placeScale;
            var y = (point.Y - placeOrigin.Y) * placeScale;
            return new Vector2(placePosition.X + x * placeCos - y * placeSin, placePosition.Y + x * placeSin + y * placeCos);
        }

        internal void SetShape(RectangleF bounds, float cornerRadius)
        {
            if (bounds == Bounds && cornerRadius == requestedCornerRadius)
                return;
            Bounds = bounds;
            requestedCornerRadius = cornerRadius;
            CornerRadius = RoundedRectanglePath.ClampRadius(bounds, cornerRadius);
            perimeterLength = -1f;
        }
    }
}
