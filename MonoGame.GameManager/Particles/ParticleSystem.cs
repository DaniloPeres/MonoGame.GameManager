using Microsoft.Xna.Framework;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// Simulates particles (without drawing them; see <see cref="Controls.ParticleEmitter"/>). The particles are
    /// stored in a fixed array that is reused, so emitting does not create garbage.
    /// </summary>
    public class ParticleSystem : IUpdatable
    {
        private readonly IRandom random;
        private Particle[] particles;
        private int activeCount;
        private float emissionAccumulator;

        public ParticleSystem(ParticleSettings settings, IRandom random = null)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.random = random ?? RandomGenerator.Default;
            particles = new Particle[Math.Max(1, settings.MaxParticles)];
        }

        public ParticleSettings Settings { get; }

        /// <summary>When true, particles are emitted continuously at <see cref="ParticleSettings.EmissionRate"/>.</summary>
        public bool IsEmitting { get; set; }

        /// <summary>Where the continuous emission happens.</summary>
        public Vector2 EmitterPosition { get; set; }

        /// <summary>The number of living particles.</summary>
        public int ActiveCount => activeCount;

        /// <summary>The particle storage: only the first <see cref="ActiveCount"/> items are alive.</summary>
        public Particle[] Particles => particles;

        /// <summary>Emits particles at <see cref="EmitterPosition"/>.</summary>
        public void Emit(int count) => Emit(count, EmitterPosition);

        /// <summary>Emits particles at a position (as many as the maximum allows).</summary>
        public void Emit(int count, Vector2 position)
        {
            EnsureCapacity();
            for (var i = 0; i < count && activeCount < particles.Length; i++)
                particles[activeCount++] = CreateParticle(position);
        }

        public void Clear()
        {
            activeCount = 0;
            emissionAccumulator = 0f;
        }

        public void Update(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
                return;

            var dragFactor = Settings.Drag > 0f ? (float)Math.Pow(MathUtils.Clamp01(1f - Settings.Drag), deltaSeconds) : 1f;
            var i = 0;
            while (i < activeCount)
            {
                ref var particle = ref particles[i];
                particle.Age += deltaSeconds;
                if (particle.Age >= particle.Lifetime)
                {
                    // Replace the dead particle with the last one.
                    particles[i] = particles[--activeCount];
                    continue;
                }

                particle.Velocity = (particle.Velocity + Settings.Gravity * deltaSeconds) * dragFactor;
                particle.Position += particle.Velocity * deltaSeconds;
                particle.Rotation += particle.RotationSpeed * deltaSeconds;
                i++;
            }

            // Emit after moving the living particles, so new particles start at age 0 where they are born.
            if (IsEmitting && Settings.EmissionRate > 0f)
            {
                emissionAccumulator += Settings.EmissionRate * deltaSeconds;
                var toEmit = (int)emissionAccumulator;
                emissionAccumulator -= toEmit;
                Emit(toEmit);
            }
        }

        /// <summary>The color of a particle at its age.</summary>
        public Color GetColor(Particle particle) => Color.Lerp(Settings.StartColor, Settings.EndColor, MathUtils.Clamp01(particle.Progress));

        /// <summary>The scale of a particle at its age.</summary>
        public float GetScale(Particle particle) => MathUtils.Lerp(Settings.StartScale, Settings.EndScale, MathUtils.Clamp01(particle.Progress));

        private Particle CreateParticle(Vector2 position)
        {
            var angle = MathHelper.ToRadians(random.NextFloat(Settings.AngleMin, Settings.AngleMax));
            var speed = random.NextFloat(Settings.SpeedMin, Settings.SpeedMax);
            var spawn = Settings.SpawnRadius > 0f ? random.NextPointInCircle(new Circle(position, Settings.SpawnRadius)) : position;

            return new Particle
            {
                Position = spawn,
                Velocity = MathUtils.AngleToVector(angle, speed),
                Age = 0f,
                Lifetime = Math.Max(0.001f, random.NextFloat(Settings.LifetimeMin, Settings.LifetimeMax)),
                Rotation = random.NextAngle(),
                RotationSpeed = MathHelper.ToRadians(random.NextFloat(Settings.RotationSpeedMin, Settings.RotationSpeedMax))
            };
        }

        private void EnsureCapacity()
        {
            var capacity = Math.Max(1, Settings.MaxParticles);
            if (particles.Length == capacity)
                return;

            var resized = new Particle[capacity];
            activeCount = Math.Min(activeCount, capacity);
            Array.Copy(particles, resized, activeCount);
            particles = resized;
        }
    }
}
