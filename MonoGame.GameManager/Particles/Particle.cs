using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// The state of one particle of a <see cref="ParticleSystem"/>.
    /// </summary>
    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Age;
        public float Lifetime;

        /// <summary>The rotation in radians.</summary>
        public float Rotation;

        /// <summary>The rotation speed in radians per second.</summary>
        public float RotationSpeed;

        /// <summary>
        /// The color chosen in <see cref="ParticleSettings.Colors"/> when the particle was born, multiplied with the
        /// color over lifetime (white, or the default value, means no tint).
        /// </summary>
        public Color Tint;

        /// <summary>A random variation of the scale: the particle is drawn with its scale multiplied by 1 + this value.</summary>
        public float ScaleVariation;

        /// <summary>The index in <see cref="ParticleSettings.Textures"/> chosen when the particle was born.</summary>
        public int TextureIndex;

        /// <summary>The frame of <see cref="ParticleSettings.Frames"/> chosen when the particle was born (the first frame when looping).</summary>
        public int FrameIndex;

        /// <summary>A random value from 0 to 1, so each particle follows a different turbulence path.</summary>
        public float Seed;

        /// <summary>The fraction of a trail particle not emitted yet (see <see cref="ParticleSettings.Trail"/>).</summary>
        public float TrailAccumulator;

        /// <summary>The flip chosen when the particle was born (see <see cref="ParticleSettings.RandomFlip"/>).</summary>
        public SpriteEffects Effects;

        /// <summary>The life progress, from 0 (born) to 1 (dead).</summary>
        public float Progress => Lifetime <= 0f ? 1f : Age / Lifetime;
    }
}
