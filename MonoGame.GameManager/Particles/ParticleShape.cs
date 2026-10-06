namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// The look of the particles that have no texture (<see cref="ParticleSettings.Appearance"/>). The shapes are
    /// generated at runtime (see <see cref="ParticleResources"/>) and drawn with <see cref="ParticleSettings.Size"/> pixels.
    /// </summary>
    public enum ParticleShape
    {
        /// <summary>A solid square (the default of version 2.0).</summary>
        Square,

        /// <summary>A solid anti-aliased circle.</summary>
        Circle,

        /// <summary>A soft circle, bright in the center and transparent at the edge (fire, magic, lights).</summary>
        Glow,

        /// <summary>A circle outline (bubbles).</summary>
        Ring,

        /// <summary>A five-pointed star.</summary>
        Star,

        /// <summary>A square rotated by 45 degrees.</summary>
        Diamond
    }
}
