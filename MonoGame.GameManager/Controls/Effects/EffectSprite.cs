using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A generated image of the button effects: a texture and the part of it that holds the image. The images share a few
    /// atlas textures (see <see cref="ButtonEffectResources"/>), so the effects of many buttons are drawn together without
    /// switching textures. Do not keep it between frames: get it again when drawing.
    /// </summary>
    public readonly struct EffectSprite
    {
        public EffectSprite(Texture2D texture, Rectangle source)
        {
            Texture = texture;
            Source = source;
        }

        /// <summary>The texture (an atlas page or a texture of its own).</summary>
        public Texture2D Texture { get; }

        /// <summary>The part of the texture that holds the image, in pixels.</summary>
        public Rectangle Source { get; }

        /// <summary>The width of the image in pixels.</summary>
        public int Width => Source.Width;

        /// <summary>The height of the image in pixels.</summary>
        public int Height => Source.Height;

        /// <summary>The center of the image, relative to its top-left corner (the origin to draw it centered).</summary>
        public Vector2 Center => new Vector2(Source.Width / 2f, Source.Height / 2f);

        /// <summary>True when the texture exists and is not disposed.</summary>
        public bool IsValid => Texture != null && !Texture.IsDisposed;
    }
}
