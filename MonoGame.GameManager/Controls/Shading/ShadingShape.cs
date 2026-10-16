namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>How the shape of a <see cref="ShadingEffect"/> is rendered from the silhouette of its control.</summary>
    internal enum ShadingShape
    {
        /// <summary>Around the silhouette: grown and blurred (shadows, glows, outlines, shines).</summary>
        Outer,

        /// <summary>Inside the silhouette, along its edges: the silhouette minus a blurred, moved copy of it (inner shadows and glows).</summary>
        Inner,

        /// <summary>The silhouette itself, painted with colors (gradient fills and overlays).</summary>
        Fill,

        /// <summary>The silhouette multiplied by a tiled pattern.</summary>
        Pattern,

        /// <summary>Stars stamped over the content, cut by the silhouette.</summary>
        Sparkles
    }
}
