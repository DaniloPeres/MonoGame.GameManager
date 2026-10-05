using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Particles;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A control that emits and draws particles (explosions, smoke, sparks, rain...). The particles are born at the
    /// position of the control and keep moving even if the control moves.
    /// </summary>
    /// <example>
    /// <code>
    /// var explosion = new ParticleEmitter(new ParticleSettings
    /// {
    ///     SpeedMin = 80, SpeedMax = 260, LifetimeMin = 0.3f, LifetimeMax = 0.8f,
    ///     StartColor = Color.Orange, EndColor = Color.Transparent, Size = 6
    /// }).SetPosition(x, y).AddToScreen();
    /// explosion.Burst(60);
    /// </code>
    /// </example>
    public class ParticleEmitter : ScalableControlAbstract<ParticleEmitter>
    {
        public ParticleEmitter(ParticleSettings settings, GameMath.IRandom random = null)
        {
            ParticleSystem = new ParticleSystem(settings, random ?? ServiceProvider.Random);
            AddOnUpdateEvent(UpdateParticles);
        }

        public ParticleSystem ParticleSystem { get; }

        public ParticleSettings Settings => ParticleSystem.Settings;

        /// <summary>When true, particles are emitted continuously.</summary>
        public bool IsEmitting
        {
            get => ParticleSystem.IsEmitting;
            set => ParticleSystem.IsEmitting = value;
        }

        /// <summary>Starts the continuous emission.</summary>
        public ParticleEmitter Start()
        {
            IsEmitting = true;
            return this;
        }

        /// <summary>Stops the continuous emission (the living particles finish their life).</summary>
        public ParticleEmitter Stop()
        {
            IsEmitting = false;
            return this;
        }

        /// <summary>Emits a number of particles at once.</summary>
        public ParticleEmitter Burst(int count)
        {
            ParticleSystem.Emit(count, GetPosition());
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var texture = Settings.Texture ?? ShapeExtension.WhitePixelTexture;
            var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            var sizeMultiplier = Settings.Texture == null ? Settings.Size : 1f;
            var scale = NestedScale * sizeMultiplier;
            var opacity = NestedOpacity;

            var particles = ParticleSystem.Particles;
            for (var i = 0; i < ParticleSystem.ActiveCount; i++)
            {
                var particle = particles[i];
                spriteBatch.Draw(texture, particle.Position, null, ParticleSystem.GetColor(particle) * opacity, particle.Rotation,
                    origin, scale * ParticleSystem.GetScale(particle), SpriteEffects.None, LayerDepthDraw);
            }
        }

        protected override Vector2 CalculateSize() => Vector2.Zero;

        private void UpdateParticles(GameTime gameTime)
        {
            ParticleSystem.EmitterPosition = GetPosition();
            ParticleSystem.Update(GetScaledDeltaSeconds(gameTime));
        }
    }
}
