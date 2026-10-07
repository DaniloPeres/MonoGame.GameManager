using System;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// Particles emitted by the particles of a <see cref="ParticleSystem"/>: when they die
    /// (<see cref="ParticleSettings.OnDeath"/>, eg: the explosion of a firework rocket or the splash of a rain drop)
    /// or while they live (<see cref="ParticleSettings.Trail"/>, eg: the smoke behind a rocket). The sub-emitter has
    /// its own settings, simulated in the space of the parent system; a sub-emitter cannot have sub-emitters itself.
    /// </summary>
    /// <example>
    /// <code>
    /// rocket.OnDeath = new ParticleSubEmitter(ParticlePresets.Explosion()) { CountMin = 80, CountMax = 120 };
    /// rocket.Trail = new ParticleSubEmitter(ParticlePresets.Smoke()) { Rate = 40 };
    /// </code>
    /// </example>
    public class ParticleSubEmitter
    {
        private ParticleSettings settings;

        public ParticleSubEmitter(ParticleSettings settings)
        {
            Settings = settings;
        }

        /// <summary>The settings of the emitted particles.</summary>
        public ParticleSettings Settings
        {
            get => settings;
            set => settings = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>The minimum number of particles emitted when a parent particle dies.</summary>
        public int CountMin { get; set; } = 8;

        /// <summary>The maximum number of particles emitted when a parent particle dies.</summary>
        public int CountMax { get; set; } = 8;

        /// <summary>The probability (from 0 to 1) of a dying parent particle to emit.</summary>
        public float Probability { get; set; } = 1f;

        /// <summary>Particles emitted per second by each living parent particle (0 = none).</summary>
        public float Rate { get; set; }

        /// <summary>The fraction of the velocity of the parent particle given to the emitted particles, from 0 to 1.</summary>
        public float InheritVelocity { get; set; }

        /// <summary>
        /// When true, the emitted particles are tinted with the color of the parent particle (its palette color, or
        /// its color over lifetime at the moment of the emission).
        /// </summary>
        public bool InheritColor { get; set; }

        /// <summary>A copy with cloned settings (the textures are shared).</summary>
        public ParticleSubEmitter Clone()
        {
            var clone = (ParticleSubEmitter)MemberwiseClone();
            clone.settings = settings.Clone();
            return clone;
        }
    }
}
