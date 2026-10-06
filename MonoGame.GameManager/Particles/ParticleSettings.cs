using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.GameMath;
using System.Collections.Generic;

namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// How the particles of a <see cref="ParticleSystem"/> are emitted, how they move and how they look.
    /// Values with Min and Max are chosen randomly in the range for each particle. The settings are read every
    /// frame, so they can be changed while the system runs. See <see cref="ParticlePresets"/> for ready-made effects.
    /// </summary>
    /// <example>
    /// <code>
    /// var fire = new ParticleSettings
    /// {
    ///     Shape = EmitterShape.Circle, SpawnRadius = 12,
    ///     AngleMin = -110, AngleMax = -70, SpeedMin = 40, SpeedMax = 100,
    ///     LifetimeMin = 0.6f, LifetimeMax = 1.2f,
    ///     Appearance = ParticleShape.Glow, Size = 24, BlendState = ParticleResources.AdditiveBlendState,
    ///     ColorOverLifetime = ParticleGradient.FromColors(Color.White, Color.Yellow, Color.OrangeRed, Color.Transparent),
    ///     ScaleOverLifetime = ParticleCurve.Peak(0.2f, 1.2f, start: 0.3f),
    ///     Turbulence = 60
    /// };
    /// </code>
    /// </example>
    public class ParticleSettings
    {
        // ---- Emission

        /// <summary>The maximum number of living particles.</summary>
        public int MaxParticles { get; set; } = 500;

        /// <summary>Particles emitted per second while emitting continuously.</summary>
        public float EmissionRate { get; set; } = 50f;

        /// <summary>
        /// How long the emission lasts, in seconds (0 = forever). With <see cref="Loop"/> the emission starts again
        /// when it ends, otherwise the system completes once the last particle dies.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>When <see cref="Duration"/> is set, repeat the emission (and its <see cref="Bursts"/>) forever.</summary>
        public bool Loop { get; set; } = true;

        /// <summary>The time before the emission starts, in seconds (repeated at every loop).</summary>
        public float StartDelay { get; set; }

        /// <summary>
        /// Simulates this many seconds when the system starts playing, so the effect is already in full swing
        /// (eg: snow that already fills the screen).
        /// </summary>
        public float PrewarmSeconds { get; set; }

        /// <summary>Particles emitted at once at given times of the emission.</summary>
        public List<ParticleBurst> Bursts { get; set; } = new List<ParticleBurst>();

        /// <summary>
        /// Particles emitted per pixel of movement of the emitter (eg: 0.5 = one particle every two pixels), in
        /// addition to <see cref="EmissionRate"/>. Great for trails behind a moving object or the pointer.
        /// </summary>
        public float EmissionPerDistance { get; set; }

        /// <summary>The fraction of the velocity of the emitter added to the new particles, from 0 to 1.</summary>
        public float InheritVelocity { get; set; }

        /// <summary>Whether the particles stay where they were born (<see cref="SimulationSpace.World"/>) or follow the emitter.</summary>
        public SimulationSpace SimulationSpace { get; set; } = SimulationSpace.World;

        // ---- Shape

        /// <summary>Where the particles are born around the emitter.</summary>
        public EmitterShape Shape { get; set; } = EmitterShape.Point;

        /// <summary>The radius of the <see cref="EmitterShape.Circle"/> and <see cref="EmitterShape.Ring"/> shapes (a point shape with a radius is a circle).</summary>
        public float SpawnRadius { get; set; }

        /// <summary>The inner radius of the <see cref="EmitterShape.Ring"/> shape.</summary>
        public float SpawnInnerRadius { get; set; }

        /// <summary>The size of the <see cref="EmitterShape.Rectangle"/> shape, or the length (X) of the <see cref="EmitterShape.Line"/> shape.</summary>
        public Vector2 SpawnSize { get; set; }

        /// <summary>The rotation of the <see cref="EmitterShape.Rectangle"/> and <see cref="EmitterShape.Line"/> shapes, in degrees.</summary>
        public float SpawnRotation { get; set; }

        /// <summary>When true, the particles are born on the edge of the shape instead of inside it.</summary>
        public bool EmitFromEdge { get; set; }

        /// <summary>
        /// When true, the particles move away from the center of the shape (or away from a line) instead of following
        /// <see cref="AngleMin"/> and <see cref="AngleMax"/>: explosions, rings, shock waves.
        /// </summary>
        public bool RadialVelocity { get; set; }

        // ---- Initial values

        public float LifetimeMin { get; set; } = 0.5f;
        public float LifetimeMax { get; set; } = 1.5f;

        /// <summary>The initial speed, in pixels per second.</summary>
        public float SpeedMin { get; set; } = 50f;
        public float SpeedMax { get; set; } = 150f;

        /// <summary>The direction, in degrees (0 = right, 90 = down).</summary>
        public float AngleMin { get; set; }
        public float AngleMax { get; set; } = 360f;

        /// <summary>The initial rotation of the sprite, in degrees.</summary>
        public float RotationMin { get; set; }
        public float RotationMax { get; set; } = 360f;

        /// <summary>The rotation speed, in degrees per second.</summary>
        public float RotationSpeedMin { get; set; }
        public float RotationSpeedMax { get; set; }

        /// <summary>A random variation of the scale of each particle, from 0 (none) to 1 (from 0 to twice the scale).</summary>
        public float ScaleVariation { get; set; }

        /// <summary>
        /// A palette: each particle is tinted with one of these colors chosen at random (multiplied with the color
        /// over lifetime). Empty = no tint. Great for confetti and fireworks.
        /// </summary>
        public List<Color> Colors { get; set; } = new List<Color>();

        /// <summary>When true, each particle is flipped horizontally and/or vertically at random.</summary>
        public bool RandomFlip { get; set; }

        // ---- Over lifetime

        public Color StartColor { get; set; } = Color.White;
        public Color EndColor { get; set; } = Color.Transparent;

        /// <summary>The color along the life of the particles; when set, <see cref="StartColor"/> and <see cref="EndColor"/> are ignored.</summary>
        public ParticleGradient ColorOverLifetime { get; set; }

        /// <summary>An easing applied to the life progress before evaluating the color (null = linear).</summary>
        public EasingFunction ColorEasing { get; set; }

        public float StartScale { get; set; } = 1f;
        public float EndScale { get; set; }

        /// <summary>The scale along the life of the particles; when set, <see cref="StartScale"/> and <see cref="EndScale"/> are ignored.</summary>
        public ParticleCurve ScaleOverLifetime { get; set; }

        /// <summary>An easing applied to the life progress before evaluating the scale (null = linear). Overshooting easings (Back, Elastic) are allowed.</summary>
        public EasingFunction ScaleEasing { get; set; }

        /// <summary>An opacity multiplier along the life of the particles (null = none), eg: <see cref="ParticleCurve.FadeInOut"/> or <see cref="ParticleCurve.Blink"/>.</summary>
        public ParticleCurve AlphaOverLifetime { get; set; }

        // ---- Forces

        /// <summary>A constant acceleration, eg: (0, 300) to make the particles fall.</summary>
        public Vector2 Gravity { get; set; }

        /// <summary>The fraction of the velocity lost per second, from 0 to 1.</summary>
        public float Drag { get; set; }

        /// <summary>A random force that makes the particles wander (smoke, fireflies, snow), in pixels per second squared.</summary>
        public float Turbulence { get; set; }

        /// <summary>How fast the turbulence changes direction (higher = more nervous).</summary>
        public float TurbulenceFrequency { get; set; } = 1f;

        /// <summary>The point of <see cref="AttractionStrength"/> and <see cref="VortexStrength"/> (null = the emitter position).</summary>
        public Vector2? AttractionPoint { get; set; }

        /// <summary>An acceleration toward <see cref="AttractionPoint"/>, in pixels per second squared (negative = away from it).</summary>
        public float AttractionStrength { get; set; }

        /// <summary>An acceleration around <see cref="AttractionPoint"/> (perpendicular to it), in pixels per second squared: whirlpools and portals.</summary>
        public float VortexStrength { get; set; }

        /// <summary>A horizontal line (Y, in the simulation space) the particles cannot go below (null = none). See <see cref="BoundsMode"/>.</summary>
        public float? Floor { get; set; }

        /// <summary>An area (in the simulation space) the particles cannot leave (null = none). See <see cref="BoundsMode"/>.</summary>
        public RectangleF? Bounds { get; set; }

        /// <summary>What happens at the <see cref="Floor"/> and the <see cref="Bounds"/>.</summary>
        public ParticleBoundsMode BoundsMode { get; set; } = ParticleBoundsMode.Bounce;

        /// <summary>The fraction of the speed kept when bouncing, from 0 to 1.</summary>
        public float Bounciness { get; set; } = 0.5f;

        /// <summary>The fraction of the speed along the surface (and of the spin) lost when bouncing, from 0 to 1.</summary>
        public float Friction { get; set; } = 0.2f;

        // ---- Rendering

        /// <summary>The texture of the particles (null = the <see cref="Appearance"/> shape of <see cref="Size"/> pixels).</summary>
        public Texture2D Texture { get; set; }

        /// <summary>Several textures: each particle uses one of them chosen at random (when empty, <see cref="Texture"/> is used).</summary>
        public List<Texture2D> Textures { get; set; } = new List<Texture2D>();

        /// <summary>The frames of a sprite sheet (parts of the texture); see <see cref="FrameMode"/>. Empty = the whole texture.</summary>
        public List<Rectangle> Frames { get; set; } = new List<Rectangle>();

        /// <summary>How the <see cref="Frames"/> are used.</summary>
        public ParticleFrameMode FrameMode { get; set; } = ParticleFrameMode.Random;

        /// <summary>The frames per second of <see cref="ParticleFrameMode.Loop"/>.</summary>
        public float FrameRate { get; set; } = 12f;

        /// <summary>The shape drawn when there is no texture.</summary>
        public ParticleShape Appearance { get; set; } = ParticleShape.Square;

        /// <summary>The size in pixels of the <see cref="Appearance"/> shape drawn when there is no texture.</summary>
        public float Size { get; set; } = 4f;

        /// <summary>
        /// The blend state used to draw the particles (null = the one of the screen, usually alpha blending).
        /// Use <see cref="ParticleResources.AdditiveBlendState"/> for lights, fire and magic.
        /// </summary>
        public BlendState BlendState { get; set; }

        /// <summary>When true, the sprite points in the direction of the movement (arrows, sparks, rain).</summary>
        public bool AlignToVelocity { get; set; }

        /// <summary>
        /// Stretches the sprite along its movement: the length is multiplied by 1 + speed * this value (eg: 0.01 doubles
        /// the length at 100 pixels per second). Implies <see cref="AlignToVelocity"/>.
        /// </summary>
        public float VelocityStretch { get; set; }

        // ---- Sub-emitters

        /// <summary>Particles emitted when a particle dies (explosions, splashes).</summary>
        public ParticleSubEmitter OnDeath { get; set; }

        /// <summary>Particles emitted by the living particles (smoke or sparkle trails).</summary>
        public ParticleSubEmitter Trail { get; set; }

        /// <summary>
        /// A copy of the settings (the lists, the bursts and the sub-emitters are copied; the textures, curves and
        /// gradients are shared). Use it to tweak a preset without changing the original.
        /// </summary>
        public ParticleSettings Clone()
        {
            var clone = (ParticleSettings)MemberwiseClone();
            clone.Bursts = new List<ParticleBurst>(Bursts?.Count ?? 0);
            if (Bursts != null)
                foreach (var burst in Bursts)
                    clone.Bursts.Add(burst.Clone());
            clone.Colors = Colors != null ? new List<Color>(Colors) : new List<Color>();
            clone.Textures = Textures != null ? new List<Texture2D>(Textures) : new List<Texture2D>();
            clone.Frames = Frames != null ? new List<Rectangle>(Frames) : new List<Rectangle>();
            clone.OnDeath = OnDeath?.Clone();
            clone.Trail = Trail?.Clone();
            return clone;
        }
    }
}
