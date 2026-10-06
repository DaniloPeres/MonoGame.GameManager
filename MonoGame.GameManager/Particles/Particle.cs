using Microsoft.Xna.Framework;

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

        /// <summary>The life progress, from 0 (born) to 1 (dead).</summary>
        public float Progress => Lifetime <= 0f ? 1f : Age / Lifetime;
    }
}
