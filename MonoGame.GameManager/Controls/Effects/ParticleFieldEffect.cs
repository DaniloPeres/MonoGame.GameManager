using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Particles of light scattered on the button: glows, sparkles, stars and dots inside it, in its upper or lower
    /// half, above it, under it or around it. By default they stay still (like bokeh and glitter painted on the button);
    /// they can twinkle (<see cref="TwinkleSpeed"/>) or float slowly upwards (<see cref="DriftSpeed"/>).
    /// </summary>
    /// <remarks>
    /// The particles are placed from a <see cref="Seed"/>, at relative positions: the same seed gives the same pattern,
    /// and the pattern stretches with the button. Several fields can be combined (eg: big soft glows at the bottom and
    /// small sparkles everywhere).
    /// </remarks>
    /// <example>
    /// <code>
    /// button.AddEffect(new ParticleFieldEffect(ParticleFieldArea.Bottom).SetCount(18).SetSize(3, 10).UseButtonColor(-0.7f));
    /// button.AddEffect(new ParticleFieldEffect().SetShapes(LightShape.Flare, LightShape.Star).SetCount(8).SetSize(5, 11));
    /// button.AddEffect(new ParticleFieldEffect(ParticleFieldArea.Above).SetCount(10).SetTwinkle(1.2f, 0.7f));
    /// </code>
    /// </example>
    public class ParticleFieldEffect : ButtonEffect<ParticleFieldEffect>
    {
        private Particle[] particles = Array.Empty<Particle>();
        private int generatedSeed = int.MinValue;
        private int generatedCount = -1;

        public ParticleFieldEffect() : this(ParticleFieldArea.Inside) { }

        public ParticleFieldEffect(ParticleFieldArea area)
        {
            Area = area;
            Intensity = 0.45f;
            SetStateIntensities(1f, 1.2f, 1f, 0.3f);
        }

        /// <summary>Where the particles are.</summary>
        public ParticleFieldArea Area { get; set; }

        /// <summary>The number of particles.</summary>
        public int Count { get; set; } = 20;

        /// <summary>The size of the smallest particles.</summary>
        public float SizeMin { get; set; } = 3f;

        /// <summary>The size of the biggest particles.</summary>
        public float SizeMax { get; set; } = 9f;

        /// <summary>The shapes of the particles (picked at random for each one).</summary>
        public IList<LightShape> Shapes { get; set; } = new List<LightShape> { LightShape.Glow, LightShape.Glow, LightShape.Flare, LightShape.Circle };

        /// <summary>Colors picked at random for each particle (null or empty = <see cref="ButtonEffect.Color"/>).</summary>
        public IList<Color> Colors { get; set; }

        /// <summary>The value that places the particles: another seed gives another pattern.</summary>
        public int Seed { get; set; } = 7;

        /// <summary>How far from the edge the particles go, for the areas outside the button.</summary>
        public float Spread { get; set; } = 14f;

        /// <summary>The opacity of the faintest particles (the others are between it and 1).</summary>
        public float OpacityMin { get; set; } = 0.15f;

        /// <summary>The size of the white core of the glows, as a part of their size (0 = no core).</summary>
        public float CoreRate { get; set; } = 0.15f;

        /// <summary>The number of twinkles per second (0 = static).</summary>
        public float TwinkleSpeed { get; set; }

        /// <summary>How much the particles dim when they twinkle (0 to 1).</summary>
        public float TwinkleAmount { get; set; } = 0.7f;

        /// <summary>The speed at which the particles float upwards, in local units per second (0 = static).</summary>
        public float DriftSpeed { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer
            => Area == ParticleFieldArea.Inside || Area == ParticleFieldArea.Top || Area == ParticleFieldArea.Bottom
                ? ButtonEffectLayer.Inside
                : ButtonEffectLayer.Front;

        public ParticleFieldEffect SetArea(ParticleFieldArea area)
        {
            Area = area;
            return this;
        }

        public ParticleFieldEffect SetCount(int count)
        {
            Count = count;
            return this;
        }

        /// <summary>Sets the size of the smallest and of the biggest particles.</summary>
        public ParticleFieldEffect SetSize(float minimum, float maximum)
        {
            SizeMin = Math.Min(minimum, maximum);
            SizeMax = Math.Max(minimum, maximum);
            return this;
        }

        /// <summary>Sets the shapes picked for the particles.</summary>
        public ParticleFieldEffect SetShapes(params LightShape[] shapes)
        {
            Shapes = new List<LightShape>(shapes ?? Array.Empty<LightShape>());
            return this;
        }

        /// <summary>Sets the colors picked for the particles.</summary>
        public ParticleFieldEffect SetColors(params Color[] colors)
        {
            Colors = colors != null && colors.Length > 0 ? new List<Color>(colors) : null;
            return this;
        }

        public ParticleFieldEffect SetSeed(int seed)
        {
            Seed = seed;
            return this;
        }

        public ParticleFieldEffect SetSpread(float spread)
        {
            Spread = spread;
            return this;
        }

        public ParticleFieldEffect SetOpacityMin(float opacityMin)
        {
            OpacityMin = opacityMin;
            return this;
        }

        public ParticleFieldEffect SetCoreRate(float coreRate)
        {
            CoreRate = coreRate;
            return this;
        }

        /// <summary>Makes the particles twinkle (speed 0 = static).</summary>
        public ParticleFieldEffect SetTwinkle(float speed, float amount = 0.7f)
        {
            TwinkleSpeed = speed;
            TwinkleAmount = amount;
            return this;
        }

        /// <summary>Makes the particles float upwards (0 = static).</summary>
        public ParticleFieldEffect SetDrift(float speed)
        {
            DriftSpeed = speed;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var count = Math.Max(0, Math.Min(500, Count));
            if (count == 0)
                return;
            if (count != generatedCount || Seed != generatedSeed)
                Generate(count);

            var bounds = context.Bounds;
            var area = GetArea(bounds);
            if (Area != ParticleFieldArea.Around && Area != ParticleFieldArea.Edge && (area.Width <= 0f || area.Height <= 0f))
                return;

            var time = GetLocalTime(context);
            var baseColor = GetColor(context);
            var strength = EffectiveIntensity * context.Opacity;
            var shapes = Shapes;
            var colors = Colors;
            var core = ButtonEffectResources.GetShape(LightShape.Circle);
            var coreOrigin = new Vector2(core.Width / 2f, core.Height / 2f);
            var twinkleAmount = MathUtils.Clamp01(TwinkleAmount);
            var drift = DriftSpeed != 0f && area.Height > 0f ? time * DriftSpeed / area.Height : 0f;

            for (var i = 0; i < count; i++)
            {
                ref var particle = ref particles[i];
                var position = GetPosition(context, area, particle, drift);
                var size = MathUtils.Lerp(SizeMin, SizeMax, particle.Size);
                if (size <= 0f)
                    continue;

                var alpha = MathUtils.Lerp(MathUtils.Clamp01(OpacityMin), 1f, particle.Opacity);
                if (TwinkleSpeed > 0f)
                    alpha *= 1f - twinkleAmount * (0.5f + 0.5f * (float)Math.Sin((time * TwinkleSpeed + particle.Phase) * MathHelper.TwoPi));
                if (drift != 0f)
                    alpha *= EdgeFade(particle.V, drift);

                var shape = shapes != null && shapes.Count > 0 ? shapes[particle.Shape % shapes.Count] : LightShape.Glow;
                var color = colors != null && colors.Count > 0 ? colors[particle.Color % colors.Count] : baseColor;
                var texture = ButtonEffectResources.GetShape(shape);
                var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                var rotation = shape == LightShape.Flare || shape == LightShape.Star ? particle.Rotation : 0f;
                spriteBatch.Draw(texture, position, null, color * (alpha * strength), rotation, origin, size / texture.Width, SpriteEffects.None, 0f);

                if (shape == LightShape.Glow && CoreRate > 0f)
                    spriteBatch.Draw(core, position, null, Color.White * (alpha * strength * 0.5f), 0f, coreOrigin, size * CoreRate / core.Width, SpriteEffects.None, 0f);
            }
        }

        /// <summary>The rectangle of the area (unused for the areas that follow the outline).</summary>
        private RectangleF GetArea(RectangleF bounds)
        {
            var spread = Math.Max(0f, Spread);
            switch (Area)
            {
                case ParticleFieldArea.Top:
                    return new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height * 0.5f);
                case ParticleFieldArea.Bottom:
                    return new RectangleF(bounds.X, bounds.Y + bounds.Height * 0.45f, bounds.Width, bounds.Height * 0.55f);
                case ParticleFieldArea.Above:
                    return new RectangleF(bounds.X + bounds.Width * 0.05f, bounds.Y - spread, bounds.Width * 0.9f, spread);
                case ParticleFieldArea.Below:
                    return new RectangleF(bounds.X + bounds.Width * 0.05f, bounds.Bottom, bounds.Width * 0.9f, spread);
                case ParticleFieldArea.Around:
                case ParticleFieldArea.Edge:
                    return new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                default:
                    return bounds;
            }
        }

        private Vector2 GetPosition(ButtonEffectContext context, RectangleF area, Particle particle, float drift)
        {
            switch (Area)
            {
                case ParticleFieldArea.Around:
                case ParticleFieldArea.Edge:
                {
                    var point = context.PointOnBorder(particle.U * context.PerimeterLength, out var normal);
                    var spread = Math.Max(0f, Spread);
                    var distance = Area == ParticleFieldArea.Around ? 2f + particle.V * spread : (particle.V - 0.5f) * spread;
                    return point + normal * distance;
                }
                default:
                {
                    var v = drift != 0f ? MathUtils.Wrap(particle.V - drift, 0f, 1f) : particle.V;
                    return new Vector2(area.X + particle.U * area.Width, area.Y + v * area.Height);
                }
            }
        }

        /// <summary>Particles that float fade in at the bottom of the area and fade out at its top.</summary>
        private static float EdgeFade(float v, float drift)
        {
            var position = MathUtils.Wrap(v - drift, 0f, 1f);
            return MathUtils.Clamp01(position * 6f) * MathUtils.Clamp01((1f - position) * 6f);
        }

        private void Generate(int count)
        {
            var random = new Random(Seed);
            if (particles.Length != count)
                particles = new Particle[count];

            for (var i = 0; i < count; i++)
            {
                // The sizes favor small particles: a few big ones and many small ones look natural.
                var size = (float)random.NextDouble();
                particles[i] = new Particle
                {
                    U = (float)random.NextDouble(),
                    V = (float)random.NextDouble(),
                    Size = size * size,
                    Opacity = (float)random.NextDouble(),
                    Phase = (float)random.NextDouble(),
                    Rotation = (float)(random.NextDouble() * Math.PI),
                    Shape = random.Next(0, 1000),
                    Color = random.Next(0, 1000)
                };
            }

            generatedCount = count;
            generatedSeed = Seed;
        }

        private struct Particle
        {
            public float U;
            public float V;
            public float Size;
            public float Opacity;
            public float Phase;
            public float Rotation;
            public int Shape;
            public int Color;
        }
    }
}
