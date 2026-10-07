using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// Ready-made particle effects. Each method returns new settings, sized for a screen of about 1280x720 pixels,
    /// that can be changed freely before or after creating the emitter. Effects that fall (rain, sparks, confetti,
    /// fountain) look best with a <see cref="ParticleSettings.Floor"/> at the ground level of the scene.
    /// </summary>
    /// <example>
    /// <code>
    /// var campfire = new ParticleEmitter(ParticlePresets.Fire()).SetPosition(400, 500).AddToScreen().Play();
    ///
    /// var snow = ParticlePresets.Snow();
    /// snow.SpawnSize = new Vector2(ScreenManager.ScreenSize.X, 0);   // as wide as the screen
    /// new ParticleEmitter(snow).SetPosition(ScreenManager.ScreenSize.X / 2f, -10).AddToScreen().Play();
    ///
    /// ParticleEmitter.Spawn(ParticlePresets.Explosion(), enemy.PositionAnchor);
    /// </code>
    /// </example>
    public static class ParticlePresets
    {
        /// <summary>The names of the presets, usable with <see cref="Create"/>.</summary>
        public static readonly IReadOnlyList<string> Names = new[]
        {
            "Fire", "Smoke", "Explosion", "Sparks", "Rain", "Snow", "Confetti", "Fireworks",
            "Magic", "Fountain", "Bubbles", "Fireflies", "Vortex", "Stars"
        };

        /// <summary>Creates a preset by name (case insensitive), eg: for effects described in data files.</summary>
        /// <exception cref="ArgumentException">The name is not one of <see cref="Names"/>.</exception>
        public static ParticleSettings Create(string name)
        {
            switch (name?.Trim().ToLowerInvariant())
            {
                case "fire": return Fire();
                case "smoke": return Smoke();
                case "explosion": return Explosion();
                case "sparks": return Sparks();
                case "rain": return Rain();
                case "snow": return Snow();
                case "confetti": return Confetti();
                case "fireworks": return Fireworks();
                case "magic": return Magic();
                case "fountain": return Fountain();
                case "bubbles": return Bubbles();
                case "fireflies": return Fireflies();
                case "vortex": return Vortex();
                case "stars": return Stars();
                default:
                    throw new ArgumentException($"Unknown particle preset '{name}'. The presets are: {string.Join(", ", Names)}.", nameof(name));
            }
        }

        /// <summary>A flame: additive glows rising with turbulence, from white-yellow to red.</summary>
        public static ParticleSettings Fire() => new ParticleSettings
        {
            MaxParticles = 300,
            EmissionRate = 90,
            Shape = EmitterShape.Circle,
            SpawnRadius = 16,
            AngleMin = -110,
            AngleMax = -70,
            SpeedMin = 40,
            SpeedMax = 120,
            LifetimeMin = 0.6f,
            LifetimeMax = 1.1f,
            Appearance = ParticleShape.Glow,
            Size = 30,
            BlendState = ParticleResources.AdditiveBlendState,
            ColorOverLifetime = ParticleGradient.FromColors(new Color(255, 240, 180), new Color(255, 160, 40), new Color(200, 40, 10), Color.Transparent),
            ScaleOverLifetime = ParticleCurve.Peak(0.2f, 1.1f, start: 0.5f, end: 0.15f),
            Drag = 0.6f,
            Turbulence = 60,
            TurbulenceFrequency = 2f,
            RotationMax = 0
        };

        /// <summary>Gray puffs that grow, rotate slowly and fade while they rise.</summary>
        public static ParticleSettings Smoke() => new ParticleSettings
        {
            MaxParticles = 200,
            EmissionRate = 30,
            Shape = EmitterShape.Circle,
            SpawnRadius = 10,
            AngleMin = -100,
            AngleMax = -80,
            SpeedMin = 20,
            SpeedMax = 45,
            LifetimeMin = 1.8f,
            LifetimeMax = 3f,
            Appearance = ParticleShape.Glow,
            Size = 40,
            ColorOverLifetime = new ParticleGradient().AddStop(0f, Color.Transparent).AddStop(0.15f, new Color(120, 120, 120) * 0.55f).AddStop(1f, Color.Transparent),
            ScaleOverLifetime = ParticleCurve.FromEasing(Easing.CubicOut, 0.4f, 1.8f),
            Turbulence = 25,
            TurbulenceFrequency = 0.6f,
            RotationSpeedMin = -20,
            RotationSpeedMax = 20,
            Drag = 0.3f,
            Gravity = new Vector2(0, -15)
        };

        /// <summary>A one-shot explosion: a burst of fast additive glows that slow down, with a few smoke puffs; completes by itself.</summary>
        public static ParticleSettings Explosion() => new ParticleSettings
        {
            MaxParticles = 300,
            EmissionRate = 0,
            Duration = 0.2f,
            Loop = false,
            Bursts = { new ParticleBurst(0f, 80, 120) },
            Shape = EmitterShape.Circle,
            SpawnRadius = 6,
            SpeedMin = 120,
            SpeedMax = 420,
            Drag = 0.92f,
            LifetimeMin = 0.4f,
            LifetimeMax = 0.9f,
            Appearance = ParticleShape.Glow,
            Size = 18,
            BlendState = ParticleResources.AdditiveBlendState,
            ColorOverLifetime = ParticleGradient.FromColors(Color.White, new Color(255, 200, 60), new Color(255, 90, 20), new Color(120, 20, 10), Color.Transparent),
            ScaleOverLifetime = ParticleCurve.Linear(1.2f, 0.2f),
            VelocityStretch = 0.004f,
            OnDeath = new ParticleSubEmitter(ExplosionSmoke()) { CountMin = 0, CountMax = 1, Probability = 0.35f }
        };

        /// <summary>Hot sparks thrown up that fall with gravity and bounce on the <see cref="ParticleSettings.Floor"/>.</summary>
        public static ParticleSettings Sparks() => new ParticleSettings
        {
            MaxParticles = 400,
            EmissionRate = 120,
            AngleMin = -150,
            AngleMax = -30,
            SpeedMin = 150,
            SpeedMax = 420,
            Gravity = new Vector2(0, 500),
            Drag = 0.2f,
            LifetimeMin = 0.4f,
            LifetimeMax = 1f,
            Appearance = ParticleShape.Square,
            Size = 3,
            VelocityStretch = 0.012f,
            BlendState = ParticleResources.AdditiveBlendState,
            ColorOverLifetime = ParticleGradient.FromColors(Color.White, new Color(255, 220, 90), new Color(255, 120, 30), Color.Transparent),
            Bounciness = 0.4f,
            Friction = 0.3f
        };

        /// <summary>
        /// Rain drops falling from a wide line, stretched by their speed. With a <see cref="ParticleSettings.Floor"/>
        /// (and the default <see cref="ParticleBoundsMode.Kill"/>) they splash on the ground.
        /// </summary>
        public static ParticleSettings Rain() => new ParticleSettings
        {
            MaxParticles = 800,
            EmissionRate = 400,
            Shape = EmitterShape.Line,
            SpawnSize = new Vector2(1400, 0),
            AngleMin = 95,
            AngleMax = 100,
            SpeedMin = 700,
            SpeedMax = 900,
            LifetimeMin = 1.2f,
            LifetimeMax = 1.4f,
            Appearance = ParticleShape.Square,
            Size = 2,
            VelocityStretch = 0.015f,
            StartColor = new Color(170, 200, 255) * 0.7f,
            EndColor = new Color(170, 200, 255) * 0.7f,
            BoundsMode = ParticleBoundsMode.Kill,
            OnDeath = new ParticleSubEmitter(Splash()) { CountMin = 4, CountMax = 7 },
            PrewarmSeconds = 1.5f
        };

        /// <summary>Snowflakes of various sizes drifting down slowly; the scene is already snowing when it starts.</summary>
        public static ParticleSettings Snow() => new ParticleSettings
        {
            MaxParticles = 600,
            EmissionRate = 60,
            Shape = EmitterShape.Line,
            SpawnSize = new Vector2(1400, 0),
            AngleMin = 80,
            AngleMax = 100,
            SpeedMin = 25,
            SpeedMax = 60,
            Gravity = new Vector2(0, 10),
            LifetimeMin = 7f,
            LifetimeMax = 11f,
            Appearance = ParticleShape.Circle,
            Size = 7,
            ScaleVariation = 0.5f,
            StartColor = Color.White,
            EndColor = Color.White,
            AlphaOverLifetime = ParticleCurve.FadeInOut(0.05f, 0.15f),
            Turbulence = 35,
            TurbulenceFrequency = 0.4f,
            PrewarmSeconds = 8f,
            Bounciness = 0.1f,
            Friction = 0.6f
        };

        /// <summary>A one-shot burst of colored, spinning paper pieces that fall and bounce; completes by itself.</summary>
        public static ParticleSettings Confetti() => new ParticleSettings
        {
            MaxParticles = 400,
            EmissionRate = 0,
            Duration = 0.1f,
            Loop = false,
            Bursts = { new ParticleBurst(0f, 140, 180) },
            AngleMin = -130,
            AngleMax = -50,
            SpeedMin = 350,
            SpeedMax = 650,
            Gravity = new Vector2(0, 700),
            Drag = 0.85f,
            LifetimeMin = 2f,
            LifetimeMax = 3f,
            Appearance = ParticleShape.Square,
            Size = 8,
            ScaleVariation = 0.3f,
            Colors = { new Color(255, 80, 80), new Color(255, 200, 40), new Color(80, 220, 120), new Color(80, 160, 255), new Color(220, 90, 255), Color.White },
            StartColor = Color.White,
            EndColor = Color.White,
            ScaleOverLifetime = ParticleCurve.Linear(1f, 0.7f),
            AlphaOverLifetime = ParticleCurve.FadeInOut(0f, 0.25f),
            RotationSpeedMin = -400,
            RotationSpeedMax = 400,
            RandomFlip = true,
            Bounciness = 0.3f,
            Friction = 0.5f
        };

        /// <summary>
        /// Rockets launched every second from a wide line, each with a trail, exploding into a burst of its own color.
        /// Place the emitter at the bottom of the scene.
        /// </summary>
        public static ParticleSettings Fireworks() => new ParticleSettings
        {
            MaxParticles = 20,
            EmissionRate = 0,
            Duration = 1.1f,
            Loop = true,
            Bursts = { new ParticleBurst(0f, 1) { Probability = 0.9f }, new ParticleBurst(0.55f, 0, 1) },
            Shape = EmitterShape.Line,
            SpawnSize = new Vector2(600, 0),
            AngleMin = -97,
            AngleMax = -83,
            SpeedMin = 420,
            SpeedMax = 500,
            Gravity = new Vector2(0, 230),
            LifetimeMin = 1.1f,
            LifetimeMax = 1.4f,
            Appearance = ParticleShape.Glow,
            Size = 10,
            BlendState = ParticleResources.AdditiveBlendState,
            Colors = { new Color(255, 90, 90), new Color(255, 215, 90), new Color(110, 255, 140), new Color(110, 170, 255), new Color(230, 120, 255), Color.White },
            StartColor = Color.White,
            EndColor = Color.White,
            RotationMax = 0,
            Trail = new ParticleSubEmitter(RocketTrail()) { Rate = 60, InheritVelocity = 0.1f },
            OnDeath = new ParticleSubEmitter(FireworkBurst()) { CountMin = 120, CountMax = 180, InheritColor = true }
        };

        /// <summary>Twinkling colored stars; also emits along the movement of the emitter, for a trail behind the pointer or a wand.</summary>
        public static ParticleSettings Magic() => new ParticleSettings
        {
            MaxParticles = 300,
            EmissionRate = 40,
            EmissionPerDistance = 0.4f,
            Shape = EmitterShape.Ring,
            SpawnInnerRadius = 4,
            SpawnRadius = 18,
            RadialVelocity = true,
            SpeedMin = 10,
            SpeedMax = 40,
            LifetimeMin = 0.8f,
            LifetimeMax = 1.6f,
            Appearance = ParticleShape.Star,
            Size = 12,
            ScaleVariation = 0.4f,
            BlendState = ParticleResources.AdditiveBlendState,
            Colors = { new Color(200, 120, 255), new Color(120, 220, 255), Color.White, new Color(255, 150, 220) },
            StartColor = Color.White,
            EndColor = Color.White,
            ScaleOverLifetime = ParticleCurve.FadeInOut(0.2f, 0.5f),
            AlphaOverLifetime = ParticleCurve.FadeInOut(0.1f, 0.4f),
            RotationSpeedMin = -180,
            RotationSpeedMax = 180,
            Turbulence = 40,
            TurbulenceFrequency = 1.5f,
            Gravity = new Vector2(0, -20)
        };

        /// <summary>A jet of water drops thrown up that fall back and bounce on the <see cref="ParticleSettings.Floor"/>.</summary>
        public static ParticleSettings Fountain() => new ParticleSettings
        {
            MaxParticles = 800,
            EmissionRate = 220,
            Shape = EmitterShape.Circle,
            SpawnRadius = 4,
            AngleMin = -100,
            AngleMax = -80,
            SpeedMin = 560,
            SpeedMax = 680,
            Gravity = new Vector2(0, 900),
            LifetimeMin = 2f,
            LifetimeMax = 2.6f,
            Appearance = ParticleShape.Circle,
            Size = 5,
            ScaleVariation = 0.3f,
            ColorOverLifetime = ParticleGradient.FromColors(new Color(120, 180, 255), new Color(170, 215, 255), Color.Transparent),
            VelocityStretch = 0.002f,
            Bounciness = 0.35f,
            Friction = 0.3f
        };

        /// <summary>Soap bubbles of various sizes wandering up.</summary>
        public static ParticleSettings Bubbles() => new ParticleSettings
        {
            MaxParticles = 150,
            EmissionRate = 12,
            Shape = EmitterShape.Circle,
            SpawnRadius = 30,
            AngleMin = -110,
            AngleMax = -70,
            SpeedMin = 20,
            SpeedMax = 50,
            Gravity = new Vector2(0, -25),
            LifetimeMin = 2.5f,
            LifetimeMax = 4f,
            Appearance = ParticleShape.Ring,
            Size = 16,
            ScaleVariation = 0.6f,
            StartColor = new Color(190, 240, 255) * 0.9f,
            EndColor = new Color(190, 240, 255) * 0.9f,
            ScaleOverLifetime = ParticleCurve.Linear(0.6f, 1.2f),
            AlphaOverLifetime = ParticleCurve.FadeInOut(0.1f, 0.3f),
            Turbulence = 50,
            TurbulenceFrequency = 0.8f,
            RotationMax = 0
        };

        /// <summary>Green lights blinking while they wander in an area; the area is already populated when it starts.</summary>
        public static ParticleSettings Fireflies() => new ParticleSettings
        {
            MaxParticles = 120,
            EmissionRate = 8,
            Shape = EmitterShape.Rectangle,
            SpawnSize = new Vector2(500, 300),
            SpeedMin = 5,
            SpeedMax = 25,
            LifetimeMin = 3f,
            LifetimeMax = 6f,
            Appearance = ParticleShape.Glow,
            Size = 18,
            ScaleVariation = 0.3f,
            BlendState = ParticleResources.AdditiveBlendState,
            StartColor = new Color(190, 255, 90),
            EndColor = new Color(190, 255, 90),
            AlphaOverLifetime = ParticleCurve.Blink(3, 0.7f),
            Turbulence = 80,
            TurbulenceFrequency = 0.3f,
            Drag = 0.5f,
            PrewarmSeconds = 4f
        };

        /// <summary>Purple lights born on a ring, pulled to the center while they spin around it.</summary>
        public static ParticleSettings Vortex() => new ParticleSettings
        {
            MaxParticles = 500,
            EmissionRate = 150,
            Shape = EmitterShape.Ring,
            SpawnInnerRadius = 120,
            SpawnRadius = 160,
            SpeedMin = 0,
            SpeedMax = 10,
            AttractionStrength = 150,
            VortexStrength = 90,
            Drag = 0.7f,
            LifetimeMin = 2f,
            LifetimeMax = 3f,
            Appearance = ParticleShape.Glow,
            Size = 9,
            ScaleVariation = 0.4f,
            BlendState = ParticleResources.AdditiveBlendState,
            Colors = { new Color(150, 90, 255), new Color(90, 160, 255), new Color(220, 150, 255) },
            StartColor = Color.White,
            EndColor = Color.White,
            AlphaOverLifetime = ParticleCurve.FadeInOut(0.2f, 0.3f),
            ScaleOverLifetime = ParticleCurve.Linear(1f, 0.3f)
        };

        /// <summary>Gold and white stars that appear, twinkle and spin slowly around the emitter.</summary>
        public static ParticleSettings Stars() => new ParticleSettings
        {
            MaxParticles = 200,
            EmissionRate = 25,
            Shape = EmitterShape.Circle,
            SpawnRadius = 90,
            SpeedMin = 5,
            SpeedMax = 25,
            LifetimeMin = 1.5f,
            LifetimeMax = 3f,
            Appearance = ParticleShape.Star,
            Size = 14,
            ScaleVariation = 0.5f,
            Colors = { new Color(255, 220, 120), Color.White, new Color(255, 250, 200) },
            StartColor = Color.White,
            EndColor = Color.White,
            ScaleOverLifetime = ParticleCurve.FadeInOut(0.2f, 0.3f),
            AlphaOverLifetime = ParticleCurve.Blink(2, 0.9f),
            RotationSpeedMin = -90,
            RotationSpeedMax = 90,
            Gravity = new Vector2(0, -10)
        };

        private static ParticleSettings ExplosionSmoke() => new ParticleSettings
        {
            MaxParticles = 150,
            SpeedMin = 10,
            SpeedMax = 40,
            LifetimeMin = 0.6f,
            LifetimeMax = 1.2f,
            Appearance = ParticleShape.Glow,
            Size = 30,
            ColorOverLifetime = new ParticleGradient().AddStop(0f, new Color(90, 90, 90) * 0.5f).AddStop(1f, Color.Transparent),
            ScaleOverLifetime = ParticleCurve.Linear(0.6f, 1.6f),
            Gravity = new Vector2(0, -40),
            Drag = 0.5f
        };

        private static ParticleSettings Splash() => new ParticleSettings
        {
            MaxParticles = 400,
            AngleMin = -150,
            AngleMax = -30,
            SpeedMin = 60,
            SpeedMax = 160,
            Gravity = new Vector2(0, 600),
            LifetimeMin = 0.15f,
            LifetimeMax = 0.3f,
            Appearance = ParticleShape.Circle,
            Size = 3,
            StartColor = new Color(190, 215, 255) * 0.8f,
            EndColor = Color.Transparent,
            RotationMax = 0
        };

        private static ParticleSettings RocketTrail() => new ParticleSettings
        {
            MaxParticles = 600,
            SpeedMin = 5,
            SpeedMax = 30,
            LifetimeMin = 0.3f,
            LifetimeMax = 0.5f,
            Appearance = ParticleShape.Glow,
            Size = 8,
            BlendState = ParticleResources.AdditiveBlendState,
            ColorOverLifetime = ParticleGradient.FromColors(new Color(255, 200, 120), new Color(255, 100, 30), Color.Transparent),
            ScaleOverLifetime = ParticleCurve.Linear(1f, 0.2f),
            Drag = 0.5f,
            RotationMax = 0
        };

        private static ParticleSettings FireworkBurst() => new ParticleSettings
        {
            MaxParticles = 1500,
            SpeedMin = 80,
            SpeedMax = 260,
            Drag = 0.9f,
            Gravity = new Vector2(0, 60),
            LifetimeMin = 1f,
            LifetimeMax = 1.6f,
            Appearance = ParticleShape.Glow,
            Size = 7,
            BlendState = ParticleResources.AdditiveBlendState,
            StartColor = Color.White,
            EndColor = Color.White,
            AlphaOverLifetime = new ParticleCurve().AddKey(0f, 1f).AddKey(0.5f, 1f).AddKey(1f, 0f),
            ScaleOverLifetime = ParticleCurve.Linear(1.2f, 0.4f),
            RotationMax = 0
        };
    }
}
