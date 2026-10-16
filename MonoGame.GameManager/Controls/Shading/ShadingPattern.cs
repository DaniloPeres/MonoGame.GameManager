namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>The tiled textures of a <see cref="PatternOverlay"/>.</summary>
    public enum ShadingPattern
    {
        /// <summary>Soft grain (stone, metal, paper, sand).</summary>
        Noise,

        /// <summary>Round dots (polka dots, comic halftone).</summary>
        Dots,

        /// <summary>A checkerboard (racing flags, pixel art).</summary>
        Checker,

        /// <summary>Diagonal lines (hatching, brushed metal).</summary>
        Diagonal,

        /// <summary>A grid of thin lines (sci-fi panels, tiles).</summary>
        Grid,

        /// <summary>Overlapping arcs (dragon and fish scales).</summary>
        Scales,

        /// <summary>Horizontal lines (scan lines of old screens).</summary>
        ScanLines,

        /// <summary>Soft blotches (marble, camouflage, clouds).</summary>
        Blotches
    }
}
