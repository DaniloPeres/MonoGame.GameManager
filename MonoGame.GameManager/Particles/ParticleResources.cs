using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// Shared resources of the particle systems, created on demand for the current graphics device: the textures of
    /// the <see cref="ParticleShape"/>s and the additive blend state. They are released with the screen manager.
    /// </summary>
    public static class ParticleResources
    {
        /// <summary>The size in pixels of the generated shape textures (they are scaled to <see cref="ParticleSettings.Size"/>).</summary>
        public const int ShapeTextureSize = 64;

        private static readonly Texture2D[] shapeTextures = new Texture2D[Enum.GetValues(typeof(ParticleShape)).Length];
        private static BlendState additiveBlendState;
        private static GraphicsDevice graphicsDevice;

        /// <summary>
        /// Adds the particles to the background instead of blending them over it (source One, destination One):
        /// lights, fire, sparks and magic look much better with it, and overlapping particles get brighter. Unlike
        /// <see cref="BlendState.Additive"/> it works with the premultiplied alpha of the textures created by the
        /// content pipeline and by <see cref="TextureFactory"/>.
        /// </summary>
        public static BlendState AdditiveBlendState
        {
            get
            {
                if (additiveBlendState == null || additiveBlendState.IsDisposed)
                {
                    additiveBlendState = new BlendState
                    {
                        Name = "ParticleResources.Additive",
                        ColorSourceBlend = Blend.One,
                        AlphaSourceBlend = Blend.One,
                        ColorDestinationBlend = Blend.One,
                        AlphaDestinationBlend = Blend.One
                    };
                }

                return additiveBlendState;
            }
        }

        /// <summary>The texture of a shape, white, <see cref="ShapeTextureSize"/> pixels wide (1 pixel for <see cref="ParticleShape.Square"/>).</summary>
        public static Texture2D GetTexture(ParticleShape shape)
        {
            if (shape == ParticleShape.Square)
                return ShapeExtension.WhitePixelTexture;

            var device = EnsureGraphicsDevice();
            var index = (int)shape;
            if (index < 0 || index >= shapeTextures.Length)
                return ShapeExtension.WhitePixelTexture;

            var texture = shapeTextures[index];
            if (texture == null || texture.IsDisposed)
                shapeTextures[index] = texture = CreateTexture(device, shape);
            return texture;
        }

        /// <summary>Disposes the shared resources (they are recreated on demand).</summary>
        internal static void Reset()
        {
            ResetTextures();
            additiveBlendState?.Dispose();
            additiveBlendState = null;
        }

        private static void ResetTextures()
        {
            for (var i = 0; i < shapeTextures.Length; i++)
            {
                shapeTextures[i]?.Dispose();
                shapeTextures[i] = null;
            }

            graphicsDevice = null;
        }

        /// <summary>
        /// The textures belong to a graphics device: when it changes they are released (the blend state is not,
        /// it may be in use by the sprite batch that draws the particles).
        /// </summary>
        private static GraphicsDevice EnsureGraphicsDevice()
        {
            var device = ServiceProvider.GraphicsDevice
                ?? throw new InvalidOperationException("The graphics device is not available yet.");

            if (graphicsDevice != device)
            {
                ResetTextures();
                graphicsDevice = device;
            }

            return device;
        }

        private static Texture2D CreateTexture(GraphicsDevice device, ParticleShape shape)
        {
            switch (shape)
            {
                case ParticleShape.Circle:
                    return TextureFactory.CreateCircle(device, ShapeTextureSize, Color.White);
                case ParticleShape.Glow:
                    return TextureFactory.CreateGlow(device, ShapeTextureSize, Color.White, 2f);
                case ParticleShape.Ring:
                    return TextureFactory.CreateRing(device, ShapeTextureSize, ShapeTextureSize * 0.12f, Color.White);
                case ParticleShape.Star:
                    return TextureFactory.CreateStar(device, ShapeTextureSize, Color.White);
                case ParticleShape.Diamond:
                    return TextureFactory.CreateDiamond(device, ShapeTextureSize, Color.White);
                default:
                    return ShapeExtension.WhitePixelTexture;
            }
        }
    }
}
