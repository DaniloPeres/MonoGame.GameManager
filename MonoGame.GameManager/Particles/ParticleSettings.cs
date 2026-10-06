using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// How the particles of a <see cref="ParticleSystem"/> are emitted and how they evolve.
    /// Values with Min and Max are chosen randomly in the range for each particle.
    /// </summary>
    public class ParticleSettings
    {
        /// <summary>The maximum number of living particles.</summary>
        public int MaxParticles { get; set; } = 500;

        /// <summary>Particles emitted per second while emitting continuously.</summary>
        public float EmissionRate { get; set; } = 50f;

        public float LifetimeMin { get; set; } = 0.5f;
        public float LifetimeMax { get; set; } = 1.5f;

        /// <summary>The initial speed, in pixels per second.</summary>
        public float SpeedMin { get; set; } = 50f;
        public float SpeedMax { get; set; } = 150f;

        /// <summary>The direction, in degrees (0 = right, 90 = down).</summary>
        public float AngleMin { get; set; }
        public float AngleMax { get; set; } = 360f;

        /// <summary>The particles are born at a random point of a circle with this radius.</summary>
        public float SpawnRadius { get; set; }

        /// <summary>A constant acceleration, eg: (0, 300) to make the particles fall.</summary>
        public Vector2 Gravity { get; set; }

        /// <summary>The fraction of the velocity lost per second, from 0 to 1.</summary>
        public float Drag { get; set; }

        public Color StartColor { get; set; } = Color.White;
        public Color EndColor { get; set; } = Color.Transparent;

        public float StartScale { get; set; } = 1f;
        public float EndScale { get; set; }

        /// <summary>The rotation speed, in degrees per second.</summary>
        public float RotationSpeedMin { get; set; }
        public float RotationSpeedMax { get; set; }

        /// <summary>The texture of the particles (null = a square of <see cref="Size"/> pixels).</summary>
        public Texture2D Texture { get; set; }

        /// <summary>The size in pixels of the square drawn when there is no <see cref="Texture"/>.</summary>
        public float Size { get; set; } = 4f;
    }
}
