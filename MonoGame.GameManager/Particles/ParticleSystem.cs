using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// Simulates particles (without drawing them; see <see cref="Controls.ParticleEmitter"/>): emission over time
    /// with bursts, emitter shapes, forces, floor and bounds, sub-emitters. The particles are stored in a fixed array
    /// that is reused, so emitting does not create garbage. The system is an <see cref="IPlayable"/>:
    /// <see cref="Play"/> starts the emission, <see cref="Stop"/> stops it (the living particles finish their life),
    /// <see cref="Pause"/> freezes everything, <see cref="Reset"/> removes every particle and rewinds the emission.
    /// </summary>
    /// <example>
    /// <code>
    /// var explosion = new ParticleSystem(ParticlePresets.Explosion()) { EmitterPosition = new Vector2(400, 300) };
    /// explosion.Play();
    /// Scheduler.Add(explosion);                               // updated every frame with the screen
    /// explosion.Completed += () => Scheduler.Remove(explosion);
    /// </code>
    /// </example>
    public class ParticleSystem : IUpdatable, IPlayable
    {
        private const int MaxCyclesPerUpdate = 8;
        private const float PrewarmStep = 1f / 30f;
        private const int MaxPrewarmSteps = 600;

        private readonly IRandom random;
        private readonly bool isChild;
        private ParticleSettings settings;
        private Particle[] particles;
        private int activeCount;
        private float emissionAccumulator;
        private float distanceAccumulator;
        private float time;
        private bool isPlaying;
        private bool isPaused;
        private bool hasStarted;
        private bool isCompleted;
        private bool isPrewarming;
        private ParticleSystem deathSystem;
        private ParticleSystem trailSystem;

        public ParticleSystem(ParticleSettings settings, IRandom random = null)
            : this(settings, random, false) { }

        private ParticleSystem(ParticleSettings settings, IRandom random, bool isChild)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.random = random ?? RandomGenerator.Default;
            this.isChild = isChild;
            particles = new Particle[Math.Max(1, settings.MaxParticles)];
        }

        /// <summary>The settings; they can be replaced at any time (the living particles keep living).</summary>
        public ParticleSettings Settings
        {
            get => settings;
            set
            {
                settings = value ?? throw new ArgumentNullException(nameof(value));
                EnsureCapacity();
                SyncSubSystems();
            }
        }

        /// <summary>
        /// True while particles are emitted continuously (playing, not paused and inside the emission window).
        /// Setting it calls <see cref="Play"/> or <see cref="Stop"/>.
        /// </summary>
        public bool IsEmitting
        {
            get => isPlaying && !isPaused && IsInsideEmissionWindow(time);
            set
            {
                if (value)
                    Play();
                else
                    Stop();
            }
        }

        /// <summary>Where the continuous emission happens (and the center of the attraction and the vortex).</summary>
        public Vector2 EmitterPosition { get; set; }

        /// <summary>
        /// The velocity of the emitter, in pixels per second, used by <see cref="ParticleSettings.InheritVelocity"/>
        /// and <see cref="ParticleSettings.EmissionPerDistance"/>. <see cref="Controls.ParticleEmitter"/> sets it from
        /// the movement of the control.
        /// </summary>
        public Vector2 EmitterVelocity { get; set; }

        /// <summary>The number of living particles.</summary>
        public int ActiveCount => activeCount;

        /// <summary>The number of living particles, including the ones of the sub-emitters.</summary>
        public int TotalActiveCount => activeCount + (deathSystem?.activeCount ?? 0) + (trailSystem?.activeCount ?? 0);

        /// <summary>The particle storage: only the first <see cref="ActiveCount"/> items are alive.</summary>
        public Particle[] Particles => particles;

        /// <summary>The time since the emission started, in seconds (rewound by <see cref="Reset"/>).</summary>
        public float Time => time;

        /// <inheritdoc />
        public bool IsPlaying => isPlaying;

        /// <inheritdoc />
        public bool IsPaused => isPaused;

        /// <summary>
        /// True once the emission has ended (stopped, or a finite <see cref="ParticleSettings.Duration"/> without loop
        /// elapsed) and every particle, including the ones of the sub-emitters, is dead.
        /// </summary>
        public bool IsComplete => isCompleted;

        /// <summary>Raised when the system becomes <see cref="IsComplete"/>.</summary>
        public event Action Completed;

        /// <summary>The system of the <see cref="ParticleSettings.OnDeath"/> sub-emitter, or null.</summary>
        public ParticleSystem DeathSystem => deathSystem;

        /// <summary>The system of the <see cref="ParticleSettings.Trail"/> sub-emitter, or null.</summary>
        public ParticleSystem TrailSystem => trailSystem;

        /// <summary>Emits particles at <see cref="EmitterPosition"/>.</summary>
        public void Emit(int count) => Emit(count, EmitterPosition);

        /// <summary>Emits particles at a position (as many as the maximum allows).</summary>
        public void Emit(int count, Vector2 position) => EmitFrom(count, position, Vector2.Zero, null);

        /// <summary>Removes every particle (the emission goes on).</summary>
        public void Clear()
        {
            activeCount = 0;
            emissionAccumulator = 0f;
            distanceAccumulator = 0f;
            deathSystem?.Clear();
            trailSystem?.Clear();
        }

        /// <summary>
        /// Starts the emission (a completed or finished emission starts again from the beginning) and simulates
        /// <see cref="ParticleSettings.PrewarmSeconds"/> when it starts from the beginning.
        /// </summary>
        public void Play()
        {
            if (isCompleted || HasTimelineEnded())
                Reset();

            isPlaying = true;
            isPaused = false;
            hasStarted = true;
            isCompleted = false;

            if (time <= 0f && activeCount == 0 && settings.PrewarmSeconds > 0f)
                Prewarm(settings.PrewarmSeconds);
        }

        /// <summary>Freezes the particles and the emission.</summary>
        public void Pause() => isPaused = true;

        /// <summary>Continues after <see cref="Pause"/>.</summary>
        public void Resume() => isPaused = false;

        /// <summary>Stops the emission; the living particles finish their life. <see cref="Play"/> continues the emission.</summary>
        public void Stop()
        {
            isPlaying = false;
            isPaused = false;
        }

        /// <summary>Removes every particle and rewinds the emission (the bursts will happen again).</summary>
        public void Reset()
        {
            Clear();
            time = 0f;
            isCompleted = false;
            hasStarted = isPlaying;
        }

        /// <summary><see cref="Reset"/> then <see cref="Play"/>.</summary>
        public void Restart()
        {
            Reset();
            Play();
        }

        /// <summary>Simulates the given time at once, in fixed steps (at most 20 seconds).</summary>
        public void Prewarm(float seconds)
        {
            if (seconds <= 0f || isPrewarming)
                return;

            var steps = Math.Min(MaxPrewarmSteps, (int)Math.Ceiling(seconds / PrewarmStep));
            var wasPaused = isPaused;
            isPaused = false;
            isPrewarming = true;
            for (var i = 0; i < steps; i++)
                Update(PrewarmStep);
            isPrewarming = false;
            isPaused = wasPaused;
        }

        public void Update(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || isPaused)
                return;

            SyncSubSystems();
            SimulateParticles(deltaSeconds);

            if (isPlaying)
                AdvanceTimeline(deltaSeconds);

            if (deathSystem != null)
            {
                deathSystem.EmitterPosition = EmitterPosition;
                deathSystem.Update(deltaSeconds);
            }

            if (trailSystem != null)
            {
                trailSystem.EmitterPosition = EmitterPosition;
                trailSystem.Update(deltaSeconds);
            }

            if (!isPrewarming && !isChild)
                CheckCompletion();
        }

        /// <summary>The color of a particle at its age: color over lifetime, tint and opacity curve.</summary>
        public Color GetColor(Particle particle)
        {
            var progress = MathUtils.Clamp01(particle.Progress);
            var color = GetLifetimeColor(progress);
            if (particle.Tint.PackedValue != 0 && particle.Tint.PackedValue != uint.MaxValue)
                color = color.Multiply(particle.Tint);
            if (settings.AlphaOverLifetime != null)
                color *= MathUtils.Clamp01(settings.AlphaOverLifetime.Evaluate(progress));
            return color;
        }

        /// <summary>The scale of a particle at its age (scale over lifetime and random variation).</summary>
        public float GetScale(Particle particle)
        {
            var progress = MathUtils.Clamp01(particle.Progress);
            var eased = settings.ScaleEasing != null ? settings.ScaleEasing(progress) : progress;
            var scale = settings.ScaleOverLifetime != null
                ? settings.ScaleOverLifetime.Evaluate(MathUtils.Clamp01(eased))
                : MathUtils.Lerp(settings.StartScale, settings.EndScale, eased);
            return scale * (1f + particle.ScaleVariation);
        }

        /// <summary>The scale used to draw a particle: <see cref="GetScale"/>, stretched along the movement by <see cref="ParticleSettings.VelocityStretch"/>.</summary>
        public Vector2 GetDrawScale(Particle particle)
        {
            var scale = GetScale(particle);
            if (settings.VelocityStretch > 0f)
                return new Vector2(scale * (1f + particle.Velocity.Length() * settings.VelocityStretch), scale);
            return new Vector2(scale);
        }

        /// <summary>The rotation used to draw a particle: its own rotation, or the direction of its movement when aligned to the velocity.</summary>
        public float GetRotation(Particle particle)
        {
            if ((settings.AlignToVelocity || settings.VelocityStretch > 0f) && !particle.Velocity.IsNearlyZero())
                return particle.Velocity.ToAngle();
            return particle.Rotation;
        }

        /// <summary>The texture of a particle (null when the particles are drawn with the <see cref="ParticleSettings.Appearance"/> shape).</summary>
        public Texture2D GetTexture(Particle particle)
        {
            var textures = settings.Textures;
            if (textures != null && textures.Count > 0)
                return textures[Math.Abs(particle.TextureIndex) % textures.Count];
            return settings.Texture;
        }

        /// <summary>The part of the texture shown by a particle, or null for the whole texture.</summary>
        public Rectangle? GetSourceRectangle(Particle particle)
        {
            var frames = settings.Frames;
            if (frames == null || frames.Count == 0)
                return null;

            var count = frames.Count;
            switch (settings.FrameMode)
            {
                case ParticleFrameMode.Animate:
                    return frames[Math.Min(count - 1, (int)(MathUtils.Clamp01(particle.Progress) * count))];
                case ParticleFrameMode.Loop:
                    return frames[(Math.Abs((int)(particle.Age * Math.Max(0f, settings.FrameRate))) + Math.Abs(particle.FrameIndex)) % count];
                default:
                    return frames[Math.Abs(particle.FrameIndex) % count];
            }
        }

        /// <summary>The flip of a particle.</summary>
        public SpriteEffects GetEffects(Particle particle) => particle.Effects;

        internal void EmitFrom(int count, Vector2 position, Vector2 inheritedVelocity, Color? tint)
        {
            if (count <= 0)
                return;

            EnsureCapacity();
            hasStarted = true;
            isCompleted = false;
            for (var i = 0; i < count && activeCount < particles.Length; i++)
                particles[activeCount++] = CreateParticle(position, inheritedVelocity, tint);
        }

        private void SimulateParticles(float deltaSeconds)
        {
            var dragFactor = settings.Drag > 0f ? (float)Math.Pow(MathUtils.Clamp01(1f - settings.Drag), deltaSeconds) : 1f;
            var gravityStep = settings.Gravity * deltaSeconds;
            var turbulence = settings.Turbulence;
            var turbulenceFrequency = settings.TurbulenceFrequency;
            var attractionStrength = settings.AttractionStrength;
            var vortexStrength = settings.VortexStrength;
            var hasAttraction = attractionStrength != 0f || vortexStrength != 0f;
            var center = settings.AttractionPoint ?? EmitterPosition;
            var hasBounds = settings.BoundsMode != ParticleBoundsMode.None && (settings.Floor.HasValue || settings.Bounds.HasValue);
            var trail = trailSystem != null ? settings.Trail : null;
            var trailRate = trail != null ? Math.Max(0f, trail.Rate) : 0f;

            var i = 0;
            while (i < activeCount)
            {
                ref var particle = ref particles[i];
                particle.Age += deltaSeconds;
                if (particle.Age >= particle.Lifetime)
                {
                    Kill(i);
                    continue;
                }

                var velocity = particle.Velocity + gravityStep;
                if (turbulence != 0f)
                    velocity += Noise(particle.Seed, particle.Age * turbulenceFrequency) * (turbulence * deltaSeconds);

                if (hasAttraction)
                {
                    var toCenter = center - particle.Position;
                    var distance = toCenter.Length();
                    if (distance > MathUtils.Epsilon)
                    {
                        var direction = toCenter / distance;
                        velocity += direction * (attractionStrength * deltaSeconds) + direction.Perpendicular() * (vortexStrength * deltaSeconds);
                    }
                }

                velocity *= dragFactor;
                particle.Velocity = velocity;
                particle.Position += velocity * deltaSeconds;
                particle.Rotation += particle.RotationSpeed * deltaSeconds;

                if (hasBounds && !ResolveBounds(ref particle))
                {
                    Kill(i);
                    continue;
                }

                if (trailRate > 0f)
                {
                    particle.TrailAccumulator += trailRate * deltaSeconds;
                    var trailCount = (int)particle.TrailAccumulator;
                    if (trailCount > 0)
                    {
                        particle.TrailAccumulator -= trailCount;
                        trailSystem.EmitFrom(trailCount, particle.Position, particle.Velocity * trail.InheritVelocity,
                            trail.InheritColor ? GetInheritedColor(ref particle) : (Color?)null);
                    }
                }

                i++;
            }
        }

        /// <summary>Removes a particle, emitting the <see cref="ParticleSettings.OnDeath"/> particles first.</summary>
        private void Kill(int index)
        {
            if (deathSystem != null)
            {
                var onDeath = settings.OnDeath;
                if (onDeath != null && random.Chance(onDeath.Probability))
                {
                    ref var particle = ref particles[index];
                    var count = random.Next(Math.Min(onDeath.CountMin, onDeath.CountMax), Math.Max(onDeath.CountMin, onDeath.CountMax));
                    deathSystem.EmitFrom(count, particle.Position, particle.Velocity * onDeath.InheritVelocity,
                        onDeath.InheritColor ? GetInheritedColor(ref particle) : (Color?)null);
                }
            }

            // Replace the dead particle with the last one.
            particles[index] = particles[--activeCount];
        }

        /// <summary>Keeps the particle inside the floor and the bounds; returns false when it must die.</summary>
        private bool ResolveBounds(ref Particle particle)
        {
            var kill = settings.BoundsMode == ParticleBoundsMode.Kill;
            var bounciness = MathUtils.Clamp01(settings.Bounciness);
            var keep = 1f - MathUtils.Clamp01(settings.Friction);

            if (settings.Floor.HasValue && particle.Position.Y > settings.Floor.Value)
            {
                if (kill)
                    return false;
                particle.Position.Y = settings.Floor.Value;
                if (particle.Velocity.Y > 0f)
                    particle.Velocity.Y = -particle.Velocity.Y * bounciness;
                particle.Velocity.X *= keep;
                particle.RotationSpeed *= keep;
            }

            if (settings.Bounds.HasValue)
            {
                var bounds = settings.Bounds.Value;
                if (particle.Position.X < bounds.Left || particle.Position.X > bounds.Right)
                {
                    if (kill)
                        return false;
                    particle.Position.X = MathUtils.Clamp(particle.Position.X, bounds.Left, bounds.Right);
                    particle.Velocity.X = -particle.Velocity.X * bounciness;
                    particle.Velocity.Y *= keep;
                    particle.RotationSpeed *= keep;
                }

                if (particle.Position.Y < bounds.Top || particle.Position.Y > bounds.Bottom)
                {
                    if (kill)
                        return false;
                    particle.Position.Y = MathUtils.Clamp(particle.Position.Y, bounds.Top, bounds.Bottom);
                    particle.Velocity.Y = -particle.Velocity.Y * bounciness;
                    particle.Velocity.X *= keep;
                    particle.RotationSpeed *= keep;
                }
            }

            return true;
        }

        private void AdvanceTimeline(float deltaSeconds)
        {
            var previous = time;
            var delay = Math.Max(0f, settings.StartDelay);
            var duration = Math.Max(0f, settings.Duration);
            var finite = duration > 0f;
            var loop = finite && settings.Loop;
            var cycle = delay + duration;
            var now = previous + deltaSeconds;
            var reachedEnd = false;
            if (finite && !settings.Loop && now >= cycle)
            {
                now = cycle;
                reachedEnd = true;
            }

            time = now;
            if (now <= previous)
                return;

            var active = ActiveSeconds(previous, now, delay, duration, loop);
            if (active > 0f)
            {
                if (settings.EmissionRate > 0f)
                    emissionAccumulator += settings.EmissionRate * active;
                if (settings.EmissionPerDistance > 0f)
                    distanceAccumulator += EmitterVelocity.Length() * active * settings.EmissionPerDistance;

                var continuous = (int)emissionAccumulator;
                var byDistance = (int)distanceAccumulator;
                emissionAccumulator -= continuous;
                distanceAccumulator -= byDistance;
                Emit(continuous + byDistance);
            }

            FireBursts(previous, now, delay, duration, loop, reachedEnd);
        }

        /// <summary>The time, between two moments, during which the continuous emission is active.</summary>
        private static float ActiveSeconds(float previous, float now, float delay, float duration, bool loop)
        {
            if (duration <= 0f)
                return Math.Max(0f, now - Math.Max(previous, delay));

            var cycle = delay + duration;
            if (!loop)
                return Overlap(previous, now, delay, cycle);

            var first = (int)Math.Floor(previous / cycle);
            var last = (int)Math.Floor(now / cycle);
            if (last - first > MaxCyclesPerUpdate)
                first = last - MaxCyclesPerUpdate;

            var total = 0f;
            for (var c = first; c <= last; c++)
                total += Overlap(previous, now, c * cycle + delay, (c + 1) * cycle);
            return total;
        }

        private static float Overlap(float start, float end, float otherStart, float otherEnd)
            => Math.Max(0f, Math.Min(end, otherEnd) - Math.Max(start, otherStart));

        /// <summary>Fires the bursts whose time is crossed between two moments (exactly once each, whatever the frame duration).</summary>
        private void FireBursts(float previous, float now, float delay, float duration, bool loop, bool reachedEnd)
        {
            var bursts = settings.Bursts;
            if (bursts == null || bursts.Count == 0)
                return;

            var cycle = delay + duration;
            var first = 0;
            var last = 0;
            if (loop && cycle > 0f)
            {
                first = Math.Max(0, (int)Math.Floor(previous / cycle) - 1);
                last = (int)Math.Floor(now / cycle);
                if (last - first > MaxCyclesPerUpdate)
                    first = last - MaxCyclesPerUpdate;
            }

            for (var b = 0; b < bursts.Count; b++)
            {
                var burst = bursts[b];
                if (burst == null || burst.Cycles <= 0)
                    continue;

                var burstTime = Math.Max(0f, burst.Time);
                if (duration > 0f && burstTime > duration)
                    burstTime = duration;
                var interval = Math.Max(0f, burst.Interval);

                for (var c = first; c <= last; c++)
                {
                    for (var k = 0; k < burst.Cycles; k++)
                    {
                        var fireTime = c * cycle + delay + burstTime + k * interval;
                        if (fireTime < previous || fireTime > now || (fireTime == now && !reachedEnd))
                            continue;
                        if (random.Chance(burst.Probability))
                            Emit(random.Next(Math.Min(burst.CountMin, burst.CountMax), Math.Max(burst.CountMin, burst.CountMax)));
                    }
                }
            }
        }

        private bool IsInsideEmissionWindow(float at)
        {
            var delay = Math.Max(0f, settings.StartDelay);
            var duration = Math.Max(0f, settings.Duration);
            if (duration <= 0f)
                return at >= delay;

            var cycle = delay + duration;
            if (!settings.Loop)
                return at >= delay && at < cycle;

            var inCycle = at - (float)Math.Floor(at / cycle) * cycle;
            return inCycle >= delay;
        }

        private bool HasTimelineEnded()
        {
            var duration = Math.Max(0f, settings.Duration);
            return duration > 0f && !settings.Loop && time >= Math.Max(0f, settings.StartDelay) + duration;
        }

        private void CheckCompletion()
        {
            if (!hasStarted || isCompleted)
                return;
            if (isPlaying && !HasTimelineEnded())
                return;
            if (TotalActiveCount > 0)
                return;

            isCompleted = true;
            isPlaying = false;
            Completed?.Invoke();
        }

        private Particle CreateParticle(Vector2 position, Vector2 inheritedVelocity, Color? tint)
        {
            var offset = SampleShape(out var radial);
            var angle = settings.RadialVelocity && !radial.IsNearlyZero()
                ? radial.ToAngle()
                : MathHelper.ToRadians(random.NextFloat(settings.AngleMin, settings.AngleMax));
            var speed = random.NextFloat(settings.SpeedMin, settings.SpeedMax);
            var velocity = MathUtils.AngleToVector(angle, speed) + inheritedVelocity;
            if (settings.InheritVelocity != 0f && settings.SimulationSpace == SimulationSpace.World)
                velocity += EmitterVelocity * settings.InheritVelocity;

            Color particleTint;
            if (tint.HasValue)
                particleTint = tint.Value;
            else if (settings.Colors != null && settings.Colors.Count > 0)
                particleTint = settings.Colors[random.Next(0, settings.Colors.Count - 1)];
            else
                particleTint = Color.White;

            var textures = settings.Textures;
            var frames = settings.Frames;
            return new Particle
            {
                Position = position + offset,
                Velocity = velocity,
                Age = 0f,
                Lifetime = Math.Max(0.001f, random.NextFloat(settings.LifetimeMin, settings.LifetimeMax)),
                Rotation = MathHelper.ToRadians(random.NextFloat(settings.RotationMin, settings.RotationMax)),
                RotationSpeed = MathHelper.ToRadians(random.NextFloat(settings.RotationSpeedMin, settings.RotationSpeedMax)),
                Tint = particleTint,
                ScaleVariation = settings.ScaleVariation != 0f ? random.NextFloat(-settings.ScaleVariation, settings.ScaleVariation) : 0f,
                TextureIndex = textures != null && textures.Count > 1 ? random.Next(0, textures.Count - 1) : 0,
                FrameIndex = frames != null && frames.Count > 1 ? random.Next(0, frames.Count - 1) : 0,
                Seed = random.NextFloat(),
                Effects = settings.RandomFlip ? (SpriteEffects)random.Next(0, 3) : SpriteEffects.None
            };
        }

        /// <summary>A random offset inside the emitter shape, and the direction away from its center (zero at the center).</summary>
        private Vector2 SampleShape(out Vector2 radial)
        {
            var shape = settings.Shape;
            if (shape == EmitterShape.Point && settings.SpawnRadius > 0f)
                shape = EmitterShape.Circle;

            switch (shape)
            {
                case EmitterShape.Circle:
                {
                    var radius = Math.Max(0f, settings.SpawnRadius);
                    var offset = settings.EmitFromEdge
                        ? random.NextUnitVector() * radius
                        : random.NextPointInCircle(new Circle(Vector2.Zero, radius));
                    radial = offset.NormalizedOrZero();
                    return offset;
                }
                case EmitterShape.Ring:
                {
                    var outer = Math.Max(0f, settings.SpawnRadius);
                    var inner = MathUtils.Clamp(settings.SpawnInnerRadius, 0f, outer);
                    var direction = random.NextUnitVector();
                    var radius = settings.EmitFromEdge
                        ? outer
                        : (float)Math.Sqrt(MathUtils.Lerp(inner * inner, outer * outer, random.NextFloat()));
                    radial = direction;
                    return direction * radius;
                }
                case EmitterShape.Rectangle:
                {
                    var half = new Vector2(Math.Abs(settings.SpawnSize.X), Math.Abs(settings.SpawnSize.Y)) / 2f;
                    var local = settings.EmitFromEdge && (half.X > 0f || half.Y > 0f)
                        ? RandomPointOnRectangleEdge(half)
                        : new Vector2(random.NextFloat(-half.X, half.X), random.NextFloat(-half.Y, half.Y));
                    var rotation = MathHelper.ToRadians(settings.SpawnRotation);
                    var offset = rotation != 0f ? local.Rotated(rotation) : local;
                    radial = offset.NormalizedOrZero();
                    return offset;
                }
                case EmitterShape.Line:
                {
                    var halfLength = Math.Abs(settings.SpawnSize.X) / 2f;
                    var rotation = MathHelper.ToRadians(settings.SpawnRotation);
                    var local = new Vector2(random.NextFloat(-halfLength, halfLength), 0f);
                    radial = new Vector2(0f, -1f);
                    if (rotation != 0f)
                    {
                        local = local.Rotated(rotation);
                        radial = radial.Rotated(rotation);
                    }
                    return local;
                }
                default:
                    radial = Vector2.Zero;
                    return Vector2.Zero;
            }
        }

        private Vector2 RandomPointOnRectangleEdge(Vector2 half)
        {
            var width = half.X * 2f;
            var height = half.Y * 2f;
            var at = random.NextFloat() * (width + height) * 2f;
            if (at < width)
                return new Vector2(-half.X + at, -half.Y);
            at -= width;
            if (at < height)
                return new Vector2(half.X, -half.Y + at);
            at -= height;
            if (at < width)
                return new Vector2(half.X - at, half.Y);
            at -= width;
            return new Vector2(-half.X, half.Y - at);
        }

        private Color GetLifetimeColor(float progress)
        {
            var eased = settings.ColorEasing != null ? MathUtils.Clamp01(settings.ColorEasing(progress)) : progress;
            return settings.ColorOverLifetime != null
                ? settings.ColorOverLifetime.Evaluate(eased)
                : Color.Lerp(settings.StartColor, settings.EndColor, eased);
        }

        /// <summary>The color given to the particles of a sub-emitter: the palette color of the parent, or its current color.</summary>
        private Color GetInheritedColor(ref Particle particle)
        {
            if (settings.Colors != null && settings.Colors.Count > 0 && particle.Tint.PackedValue != 0)
                return particle.Tint;
            return GetLifetimeColor(MathUtils.Clamp01(particle.Progress));
        }

        /// <summary>A smooth pseudo-random direction that changes with the time, different for each seed.</summary>
        private static Vector2 Noise(float seed, float at)
        {
            var a = at * 1.7f + seed * 61.3f;
            var b = at * 2.3f + seed * 97.7f;
            return new Vector2(
                (float)(Math.Sin(a) + 0.5 * Math.Sin(b * 1.9f)),
                (float)(Math.Cos(b) + 0.5 * Math.Sin(a * 2.1f)));
        }

        private void SyncSubSystems()
        {
            if (isChild)
                return;
            deathSystem = Sync(deathSystem, settings.OnDeath);
            trailSystem = Sync(trailSystem, settings.Trail);
        }

        private ParticleSystem Sync(ParticleSystem current, ParticleSubEmitter subEmitter)
        {
            if (subEmitter == null || subEmitter.Settings == null || ReferenceEquals(subEmitter.Settings, settings))
                return null;
            if (current != null && ReferenceEquals(current.settings, subEmitter.Settings))
                return current;
            return new ParticleSystem(subEmitter.Settings, random, true);
        }

        private void EnsureCapacity()
        {
            var capacity = Math.Max(1, settings.MaxParticles);
            if (particles.Length == capacity)
                return;

            var resized = new Particle[capacity];
            activeCount = Math.Min(activeCount, capacity);
            Array.Copy(particles, resized, activeCount);
            particles = resized;
        }
    }
}
