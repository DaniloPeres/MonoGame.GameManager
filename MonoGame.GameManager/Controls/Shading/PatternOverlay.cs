using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A tiled pattern painted inside the silhouette of the control, over it: the grain of stone and metal, polka dots,
    /// comic halftone, dragon scales, sci-fi grids, scan lines. Painted (<see cref="ShadingBlend.Normal"/>) in the color
    /// of the effect over the control (<see cref="ShadingLayer.Front"/>); with <see cref="ShadingBlend.Light"/> it adds
    /// light (a white grid that glows, golden scales).
    /// </summary>
    /// <remarks>
    /// The pattern is in the local space of the control (it turns and scales with it). A scrolling pattern
    /// (<see cref="ScrollVelocity"/>) is rendered again in small steps while it moves.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Stone grain darkening a gray fill.
    /// title.AddShading(new PatternOverlay(ShadingPattern.Noise, Color.Black * 0.35f, 6));
    ///
    /// // Light scales on a dragon title.
    /// title.AddShading(new PatternOverlay(ShadingPattern.Scales, Color.White * 0.3f, 10).SetBlend(ShadingBlend.Light));
    /// </code>
    /// </example>
    public class PatternOverlay : ShadingEffect<PatternOverlay>
    {
        /// <summary>The steps of a scrolling pattern per tile.</summary>
        internal const int ScrollSteps = 32;

        private float tileSize = 8f;

        public PatternOverlay()
        {
            Color = Color.Black * 0.3f;
            Layer = ShadingLayer.Front;
            Blend = ShadingBlend.Normal;
        }

        /// <param name="pattern">The pattern.</param>
        /// <param name="color">Its color (its alpha is its strength).</param>
        /// <param name="tileSize">The size of a tile of the pattern, in local units.</param>
        public PatternOverlay(ShadingPattern pattern, Color color, float tileSize) : this()
        {
            Pattern = pattern;
            Color = color;
            TileSize = tileSize;
        }

        /// <summary>The pattern.</summary>
        public ShadingPattern Pattern { get; set; }

        /// <summary>The size of a tile of the pattern, in local units.</summary>
        public float TileSize
        {
            get => tileSize;
            set => tileSize = Math.Max(1f, value);
        }

        /// <summary>How fast the pattern moves, in local units per second (zero = still).</summary>
        public Vector2 ScrollVelocity { get; set; }

        internal override float Dilation => 0f;

        internal override float SoftEdge => 0f;

        internal override float Extent => 0f;

        internal override ShadingShape Shape => ShadingShape.Pattern;

        internal override bool IsAttached => true;

        public PatternOverlay SetPattern(ShadingPattern pattern)
        {
            Pattern = pattern;
            return this;
        }

        public PatternOverlay SetTileSize(float tileSize)
        {
            TileSize = tileSize;
            return this;
        }

        /// <summary>Makes the pattern move, in local units per second.</summary>
        public PatternOverlay SetScroll(float x, float y)
        {
            ScrollVelocity = new Vector2(x, y);
            return this;
        }

        internal override int GetShapeKey()
            => HashCode.Combine(base.GetShapeKey(), Pattern, (int)Math.Round(tileSize * 8f), ScrollVelocity != Vector2.Zero);

        internal override int GetDynamicShapeKey(float time) => GetScrollOffset(time).GetHashCode();

        /// <summary>How far the pattern moved, in tiles (from 0 to 1 on each axis), in steps.</summary>
        internal Vector2 GetScrollOffset(float time)
        {
            if (ScrollVelocity == Vector2.Zero)
                return Vector2.Zero;
            var moved = ScrollVelocity * (time + TimeOffset) / tileSize;
            return new Vector2(Step(moved.X), Step(moved.Y));
        }

        private static float Step(float value)
        {
            var fraction = value - (float)Math.Floor(value);
            return (float)Math.Floor(fraction * ScrollSteps) / ScrollSteps;
        }
    }
}
