using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Particles;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A control that emits and draws particles (explosions, smoke, sparks, rain...). The particles are born at the
    /// position of the control; they keep their position when the control moves
    /// (<see cref="SimulationSpace.World"/>) or move with it (<see cref="SimulationSpace.Local"/>). The emitter can
    /// be played, paused and stopped like an animation, and can remove itself from the screen when its effect ends.
    /// </summary>
    /// <example>
    /// <code>
    /// var fire = new ParticleEmitter(ParticlePresets.Fire()).SetPosition(x, y).AddToScreen().Play();
    ///
    /// ParticleEmitter.Spawn(ParticlePresets.Explosion(), enemy.PositionAnchor); // removed when the explosion ends
    ///
    /// var sparks = new ParticleEmitter(new ParticleSettings
    /// {
    ///     SpeedMin = 80, SpeedMax = 260, LifetimeMin = 0.3f, LifetimeMax = 0.8f,
    ///     StartColor = Color.Orange, EndColor = Color.Transparent, Size = 6
    /// }).SetPosition(x, y).AddToScreen();
    /// sparks.Burst(60);
    /// </code>
    /// </example>
    public class ParticleEmitter : ScalableControlAbstract<ParticleEmitter>, IPlayable
    {
        private Action onCompleted;
        private Vector2 previousPosition;
        private bool hasPreviousPosition;

        public ParticleEmitter(ParticleSettings settings, IRandom random = null)
        {
            ParticleSystem = new ParticleSystem(settings, random ?? ServiceProvider.Random);
            ParticleSystem.Completed += OnSystemCompleted;
            AddOnUpdateEvent(UpdateParticles);
        }

        public ParticleSystem ParticleSystem { get; }

        public ParticleSettings Settings => ParticleSystem.Settings;

        /// <summary>When true, particles are emitted continuously (see <see cref="ParticleSystem.IsEmitting"/>).</summary>
        public bool IsEmitting
        {
            get => ParticleSystem.IsEmitting;
            set => ParticleSystem.IsEmitting = value;
        }

        /// <inheritdoc />
        public bool IsPlaying => ParticleSystem.IsPlaying;

        /// <inheritdoc />
        public bool IsPaused => ParticleSystem.IsPaused;

        /// <summary>True once the emission ended and every particle is dead (see <see cref="ParticleSystem.IsComplete"/>).</summary>
        public bool IsComplete => ParticleSystem.IsComplete;

        /// <summary>When true, the control disposes itself (leaving the screen) as soon as it is <see cref="IsComplete"/>.</summary>
        public bool RemoveWhenCompleted { get; set; }

        /// <summary>
        /// Creates an emitter at a position, plays it and removes it from the screen when its effect ends. Meant for
        /// finite effects: a <see cref="ParticleSettings.Duration"/> without loop, or bursts only.
        /// </summary>
        public static ParticleEmitter Spawn(ParticleSettings settings, Vector2 position, IContainer parent = null)
            => new ParticleEmitter(settings).SetPosition(position).SetRemoveWhenCompleted(true).AddToScreen(parent).Play();

        /// <summary>Creates an emitter at a position, emits a number of particles at once and removes it when they are dead.</summary>
        public static ParticleEmitter SpawnBurst(ParticleSettings settings, Vector2 position, int count, IContainer parent = null)
            => new ParticleEmitter(settings).SetPosition(position).SetRemoveWhenCompleted(true).AddToScreen(parent).Burst(count);

        /// <summary>Starts the continuous emission (same as <see cref="Play"/>).</summary>
        public ParticleEmitter Start() => Play();

        /// <summary>Starts the emission (see <see cref="ParticleSystem.Play"/>).</summary>
        public ParticleEmitter Play()
        {
            ParticleSystem.Play();
            return this;
        }

        /// <summary>Freezes the particles and the emission.</summary>
        public ParticleEmitter Pause()
        {
            ParticleSystem.Pause();
            return this;
        }

        /// <summary>Continues after <see cref="Pause"/>.</summary>
        public ParticleEmitter Resume()
        {
            ParticleSystem.Resume();
            return this;
        }

        /// <summary>Stops the emission (the living particles finish their life).</summary>
        public ParticleEmitter Stop()
        {
            ParticleSystem.Stop();
            return this;
        }

        /// <summary>Removes every particle and rewinds the emission.</summary>
        public ParticleEmitter Reset()
        {
            ParticleSystem.Reset();
            return this;
        }

        /// <summary><see cref="Reset"/> then <see cref="Play"/>.</summary>
        public ParticleEmitter Restart()
        {
            ParticleSystem.Restart();
            return this;
        }

        /// <summary>Emits a number of particles at once, at the position of the control.</summary>
        public ParticleEmitter Burst(int count) => Burst(count, GetPosition());

        /// <summary>Emits a number of particles at once, at a position given in the coordinate space of the parent (eg: a pointer position).</summary>
        public ParticleEmitter Burst(int count, Vector2 position)
        {
            ParticleSystem.Emit(count, ToSimulationSpace(position));
            return this;
        }

        /// <summary>Replaces the settings (the living particles keep living).</summary>
        public ParticleEmitter SetSettings(ParticleSettings settings)
        {
            ParticleSystem.Settings = settings;
            return this;
        }

        /// <summary>Sets <see cref="RemoveWhenCompleted"/>.</summary>
        public ParticleEmitter SetRemoveWhenCompleted(bool removeWhenCompleted)
        {
            RemoveWhenCompleted = removeWhenCompleted;
            return this;
        }

        /// <summary>Adds a callback invoked when the effect ends (see <see cref="IsComplete"/>).</summary>
        public ParticleEmitter AddOnCompleted(Action onCompleted)
        {
            this.onCompleted += onCompleted;
            return this;
        }

        public ParticleEmitter RemoveOnCompleted(Action onCompleted)
        {
            this.onCompleted -= onCompleted;
            return this;
        }

        void IPlayable.Play() => Play();
        void IPlayable.Pause() => Pause();
        void IPlayable.Resume() => Resume();
        void IPlayable.Stop() => Stop();
        void IPlayable.Reset() => Reset();

        public override void Draw(SpriteBatch spriteBatch)
        {
            var system = ParticleSystem;
            if (system.TotalActiveCount == 0)
                return;

            var tint = DrawColor;
            var local = Settings.SimulationSpace == SimulationSpace.Local;
            var origin = local ? GetPosition() : Vector2.Zero;

            if (system.TrailSystem != null)
                DrawSystem(spriteBatch, system.TrailSystem, origin, local, tint);
            DrawSystem(spriteBatch, system, origin, local, tint);
            if (system.DeathSystem != null)
                DrawSystem(spriteBatch, system.DeathSystem, origin, local, tint);
        }

        protected override Vector2 CalculateSize() => Vector2.Zero;

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            ParticleSystem.Completed -= OnSystemCompleted;
            onCompleted = null;
            base.Dispose(disposing);
        }

        private void DrawSystem(SpriteBatch spriteBatch, ParticleSystem system, Vector2 origin, bool local, Color tint)
        {
            if (system.ActiveCount == 0)
                return;

            var settings = system.Settings;
            var blendState = settings.BlendState ?? Settings.BlendState;
            var controlManager = blendState != null ? ServiceProvider.ControlManager : null;
            if (controlManager != null)
                controlManager.PushState(spriteBatch, controlManager.CurrentState.WithBlendState(blendState));

            var usesShape = settings.Texture == null && (settings.Textures == null || settings.Textures.Count == 0);
            var fallbackTexture = usesShape ? ParticleResources.GetTexture(settings.Appearance) : settings.Texture;
            var nestedScale = NestedScale;
            var rotationOffset = local ? Rotation : 0f;
            var particles = system.Particles;
            var count = system.ActiveCount;

            for (var i = 0; i < count; i++)
            {
                ref var particle = ref particles[i];
                var texture = system.GetTexture(particle) ?? fallbackTexture;
                if (texture == null || texture.IsDisposed)
                    continue;

                var source = system.GetSourceRectangle(particle);
                var width = source.HasValue ? source.Value.Width : texture.Width;
                var height = source.HasValue ? source.Value.Height : texture.Height;
                if (width <= 0 || height <= 0)
                    continue;

                var spriteOrigin = new Vector2(width / 2f, height / 2f);
                var baseScale = usesShape ? settings.Size / width : 1f;
                var scale = system.GetDrawScale(particle) * baseScale * nestedScale;
                var color = system.GetColor(particle).Multiply(tint);

                var position = particle.Position;
                if (local)
                {
                    position *= nestedScale;
                    if (rotationOffset != 0f)
                        position = position.Rotated(rotationOffset);
                    position += origin;
                }

                spriteBatch.Draw(texture, position, source, color, system.GetRotation(particle) + rotationOffset, spriteOrigin, scale,
                    system.GetEffects(particle), LayerDepthDraw);
            }

            if (controlManager != null)
                controlManager.PopState(spriteBatch);
        }

        /// <summary>Converts a position of the parent's coordinate space to the space of the particles.</summary>
        private Vector2 ToSimulationSpace(Vector2 position)
        {
            if (Settings.SimulationSpace != SimulationSpace.Local)
                return position;

            var offset = position - GetPosition();
            if (Rotation != 0f)
                offset = offset.Rotated(-Rotation);

            var scale = NestedScale;
            return new Vector2(scale.X != 0f ? offset.X / scale.X : 0f, scale.Y != 0f ? offset.Y / scale.Y : 0f);
        }

        private void UpdateParticles(GameTime gameTime)
        {
            var deltaSeconds = GetScaledDeltaSeconds(gameTime);
            var position = GetPosition();
            var velocity = Vector2.Zero;
            if (hasPreviousPosition && deltaSeconds > 0f)
                velocity = (position - previousPosition) / deltaSeconds;
            previousPosition = position;
            hasPreviousPosition = true;

            var system = ParticleSystem;
            system.EmitterPosition = Settings.SimulationSpace == SimulationSpace.Local ? Vector2.Zero : position;
            system.EmitterVelocity = velocity;
            system.Update(deltaSeconds);

            if (RemoveWhenCompleted && system.IsComplete)
                Dispose();
        }

        private void OnSystemCompleted() => onCompleted?.Invoke();
    }
}
