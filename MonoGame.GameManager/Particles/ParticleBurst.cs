namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// A number of particles emitted at once at a given time of the emission (see <see cref="ParticleSettings.Bursts"/>).
    /// Bursts are repeated with every loop of the emission.
    /// </summary>
    /// <example>
    /// <code>
    /// settings.Bursts.Add(new ParticleBurst(0f, 100));                        // 100 particles when the emission starts
    /// settings.Bursts.Add(new ParticleBurst(0.5f, 10, 20) { Cycles = 3, Interval = 0.1f }); // 3 bursts of 10 to 20
    /// </code>
    /// </example>
    public class ParticleBurst
    {
        public ParticleBurst() { }

        public ParticleBurst(float time, int count) : this(time, count, count) { }

        public ParticleBurst(float time, int countMin, int countMax)
        {
            Time = time;
            CountMin = countMin;
            CountMax = countMax;
        }

        /// <summary>When the burst happens, in seconds after the emission starts (after <see cref="ParticleSettings.StartDelay"/>).</summary>
        public float Time { get; set; }

        /// <summary>The minimum number of particles.</summary>
        public int CountMin { get; set; } = 10;

        /// <summary>The maximum number of particles.</summary>
        public int CountMax { get; set; } = 10;

        /// <summary>How many times the burst is repeated, every <see cref="Interval"/> seconds.</summary>
        public int Cycles { get; set; } = 1;

        /// <summary>The time between the repetitions, in seconds.</summary>
        public float Interval { get; set; } = 0.1f;

        /// <summary>The probability of each repetition to happen, from 0 to 1.</summary>
        public float Probability { get; set; } = 1f;

        public ParticleBurst Clone() => (ParticleBurst)MemberwiseClone();
    }
}
