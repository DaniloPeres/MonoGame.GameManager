using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Particles;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Sparkles that appear, twinkle and vanish on the border of the button, around it, inside it or above it, and
    /// optionally burst where the button is pressed. It is built on a <see cref="ParticleSystem"/>: every option of
    /// <see cref="Settings"/> (gravity, turbulence, color palettes...) can be used too.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new SparkleEffect().SetColor(Color.Gold).SetRate(8).SetArea(SparkleArea.Around).SetClickBurst(12));
    /// </code>
    /// </example>
    public class SparkleEffect : ButtonEffect<SparkleEffect>
    {
        private readonly ParticleSystem system;
        private float accumulator;

        public SparkleEffect()
        {
            Settings = new ParticleSettings
            {
                MaxParticles = 80,
                EmissionRate = 0f,
                LifetimeMin = 0.5f,
                LifetimeMax = 1.1f,
                SpeedMin = 0f,
                SpeedMax = 6f,
                AngleMin = 0f,
                AngleMax = 360f,
                RotationMin = 0f,
                RotationMax = 360f,
                RotationSpeedMin = -60f,
                RotationSpeedMax = 60f,
                StartColor = Color.White,
                EndColor = Color.White,
                AlphaOverLifetime = ParticleCurve.FadeInOut(0.25f, 0.45f),
                ScaleOverLifetime = ParticleCurve.Peak(0.3f, 1f, 0.2f, 0f),
                ScaleVariation = 0.4f,
                Size = 10f
            };
            system = new ParticleSystem(Settings, ServiceProvider.Random);
            Intensity = 0.7f;
        }

        /// <summary>The settings of the particles (size, lifetime, speed, gravity...); they can be changed at any time.</summary>
        public ParticleSettings Settings { get; }

        /// <summary>The particle system (eg: to read the number of sparkles).</summary>
        public ParticleSystem ParticleSystem => system;

        /// <summary>The number of sparkles that appear per second.</summary>
        public float Rate { get; set; } = 6f;

        /// <summary>Where the sparkles appear.</summary>
        public SparkleArea Area { get; set; } = SparkleArea.Border;

        /// <summary>How far from the outline the sparkles can appear (for <see cref="SparkleArea.Border"/>, <see cref="SparkleArea.Around"/> and <see cref="SparkleArea.Top"/>).</summary>
        public float Spread { get; set; } = 6f;

        /// <summary>The shape of the sparkles.</summary>
        public LightShape Shape { get; set; } = LightShape.Flare;

        /// <summary>The number of sparkles that burst where the button is pressed.</summary>
        public int ClickBurst { get; set; }

        /// <summary>The size of the area of a burst.</summary>
        public float BurstRadius { get; set; } = 24f;

        /// <summary>The size of the sparkles (see <see cref="ParticleSettings.Size"/>).</summary>
        public float Size
        {
            get => Settings.Size;
            set => Settings.Size = value;
        }

        /// <summary>The random variation of the size, from 0 to 1 (see <see cref="ParticleSettings.ScaleVariation"/>).</summary>
        public float SizeVariation
        {
            get => Settings.ScaleVariation;
            set => Settings.ScaleVariation = value;
        }

        /// <summary>The speed of the sparkles, in local units per second (they move in random directions).</summary>
        public float Speed
        {
            get => Settings.SpeedMax;
            set
            {
                Settings.SpeedMin = 0f;
                Settings.SpeedMax = value;
            }
        }

        /// <summary>A constant acceleration, eg: <c>(0, -40)</c> makes the sparkles rise like embers.</summary>
        public Vector2 Gravity
        {
            get => Settings.Gravity;
            set => Settings.Gravity = value;
        }

        /// <summary>The maximum rotation speed of the sparkles, in degrees per second.</summary>
        public float SpinSpeed
        {
            get => Settings.RotationSpeedMax;
            set
            {
                Settings.RotationSpeedMin = -value;
                Settings.RotationSpeedMax = value;
            }
        }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Front;

        public SparkleEffect SetRate(float rate)
        {
            Rate = rate;
            return this;
        }

        public SparkleEffect SetArea(SparkleArea area, float spread = 6f)
        {
            Area = area;
            Spread = spread;
            return this;
        }

        public SparkleEffect SetShape(LightShape shape)
        {
            Shape = shape;
            return this;
        }

        /// <summary>Sets the size of the sparkles and its random variation (0 to 1).</summary>
        public SparkleEffect SetSize(float size, float variation = 0.4f)
        {
            Size = size;
            SizeVariation = variation;
            return this;
        }

        /// <summary>Sets how long a sparkle lives, in seconds.</summary>
        public SparkleEffect SetLifetime(float minimum, float maximum)
        {
            Settings.LifetimeMin = Math.Min(minimum, maximum);
            Settings.LifetimeMax = Math.Max(minimum, maximum);
            return this;
        }

        /// <summary>Sets the speed (random directions) and a constant acceleration (eg: rising sparkles).</summary>
        public SparkleEffect SetMotion(float speed, Vector2 gravity)
        {
            Speed = speed;
            Gravity = gravity;
            return this;
        }

        public SparkleEffect SetSpinSpeed(float spinSpeed)
        {
            SpinSpeed = spinSpeed;
            return this;
        }

        /// <summary>Sets the number of sparkles that burst where the button is pressed, and the size of the burst.</summary>
        public SparkleEffect SetClickBurst(int count, float radius = 24f)
        {
            ClickBurst = count;
            BurstRadius = radius;
            return this;
        }

        /// <summary>Makes sparkles appear at once, at random places of the <see cref="Area"/>.</summary>
        public void Burst(ButtonEffectContext context, int count)
        {
            for (var i = 0; i < count; i++)
                system.Emit(1, GetSpawnPoint(context));
        }

        /// <summary>Removes every sparkle.</summary>
        public void Clear() => system.Clear();

        /// <inheritdoc />
        public override void Update(ButtonEffectContext context)
        {
            base.Update(context);
            var deltaSeconds = context.DeltaSeconds;
            if (deltaSeconds <= 0f)
                return;

            if (Rate > 0f && StateIntensity > 0.01f)
            {
                accumulator = Math.Min(accumulator + Rate * deltaSeconds, Math.Max(1, Settings.MaxParticles));
                while (accumulator >= 1f)
                {
                    accumulator -= 1f;
                    system.Emit(1, GetSpawnPoint(context));
                }
            }

            system.Update(deltaSeconds);
        }

        /// <inheritdoc />
        public override void OnPressed(ButtonEffectContext context, Vector2 localPosition)
        {
            var random = context.Random ?? ServiceProvider.Random;
            for (var i = 0; i < ClickBurst; i++)
                system.Emit(1, localPosition + random.NextUnitVector() * random.NextFloat(0f, Math.Max(0f, BurstRadius)));
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var count = system.ActiveCount;
            if (count == 0)
                return;

            var texture = ButtonEffectResources.GetShape(Shape);
            var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            var baseScale = Settings.Size / texture.Width;
            var tint = GetTint(context);
            var particles = system.Particles;

            for (var i = 0; i < count; i++)
            {
                ref var particle = ref particles[i];
                var color = system.GetColor(particle).Multiply(tint);
                spriteBatch.Draw(texture, particle.Position, null, color, system.GetRotation(particle), origin,
                    system.GetDrawScale(particle) * baseScale, system.GetEffects(particle), 0f);
            }
        }

        private Vector2 GetSpawnPoint(ButtonEffectContext context)
        {
            var random = context.Random ?? ServiceProvider.Random;
            var bounds = context.Bounds;
            var spread = Math.Max(0f, Spread);
            switch (Area)
            {
                case SparkleArea.Inside:
                    var inner = bounds.Inflate(-Math.Min(spread, bounds.Width / 2f), -Math.Min(spread, bounds.Height / 2f));
                    return random.NextPointInRectangle(inner);
                case SparkleArea.Top:
                    return new Vector2(random.NextFloat(bounds.X, bounds.Right), bounds.Y - random.NextFloat(0f, spread));
                case SparkleArea.Around:
                {
                    var point = context.PointOnBorder(random.NextFloat(0f, context.PerimeterLength), out var normal);
                    return point + normal * random.NextFloat(spread * 0.25f, spread + 2f);
                }
                default:
                {
                    var point = context.PointOnBorder(random.NextFloat(0f, context.PerimeterLength), out var normal);
                    return point + normal * random.NextFloat(-spread, spread);
                }
            }
        }
    }
}
