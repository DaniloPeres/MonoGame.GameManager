using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// An effect drawn on a button (see <see cref="Abstracts.ButtonAbstract{TButton}.AddEffect"/>): glows, gloss,
    /// highlights, shine sweeps, gems, running lights, sparkles, shades, outlines, shadows... Lights are added to what is
    /// below them (additive blending), so they only make things brighter; shades, outlines, shadows and lips are painted
    /// over it (see <see cref="Blend"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// An effect draws in the local, unscaled space of its button (see <see cref="ButtonEffectContext.Bounds"/>): the
    /// position, scale and rotation of the button are applied for it. Sizes and speeds are in local units, so an effect
    /// looks the same on a small and on a big button.
    /// </para>
    /// <para>
    /// Every effect reacts to the visual state of its button (<see cref="NormalIntensity"/>, <see cref="HoverIntensity"/>...),
    /// can pulse (<see cref="PulseSpeed"/>, <see cref="PulseAmount"/>) and can cycle through colors
    /// (<see cref="ColorCycle"/>). An effect belongs to one button at a time.
    /// </para>
    /// <para>
    /// Custom effects derive from <see cref="ButtonEffect{TEffect}"/> and implement <see cref="Layer"/> and
    /// <see cref="Draw"/>, using <see cref="GetTint"/> for their color.
    /// </para>
    /// </remarks>
    public abstract class ButtonEffect
    {
        private float stateIntensity;
        private bool hasStateIntensity;
        private float pulse = 1f;

        /// <summary>When false, the effect is not updated nor drawn.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>The color of the light.</summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>The strength of the light (1 = the color as it is; above 1 saturates towards brighter colors).</summary>
        public float Intensity { get; set; } = 1f;

        /// <summary>The intensity multiplier while the button is in the normal state.</summary>
        public float NormalIntensity { get; set; } = 1f;

        /// <summary>The intensity multiplier while the pointer is over the button.</summary>
        public float HoverIntensity { get; set; } = 1.25f;

        /// <summary>The intensity multiplier while the button is pressed.</summary>
        public float PressedIntensity { get; set; } = 1.1f;

        /// <summary>The intensity multiplier while the button is disabled (0 = the light goes off).</summary>
        public float DisabledIntensity { get; set; }

        /// <summary>How long the light takes to reach the intensity of a new state, in seconds.</summary>
        public float TransitionSeconds { get; set; } = 0.15f;

        /// <summary>The number of pulses per second (0 = no pulse).</summary>
        public float PulseSpeed { get; set; }

        /// <summary>How much the light dims during a pulse, from 0 (no pulse) to 1 (it goes off).</summary>
        public float PulseAmount { get; set; }

        /// <summary>Added to the time of the button, to shift the animations of this effect.</summary>
        public float TimeOffset { get; set; }

        /// <summary>Colors the light goes through, one after the other (null or empty = <see cref="Color"/>).</summary>
        public IList<Color> ColorCycle { get; set; }

        /// <summary>The duration of a whole <see cref="ColorCycle"/>, in seconds.</summary>
        public float ColorCyclePeriod { get; set; } = 3f;

        /// <summary>
        /// When set, the effect uses the background color of its button instead of <see cref="Color"/>: darker (0 to
        /// 1) or lighter (0 to -1). <see cref="Color"/> is used for buttons drawn with textures.
        /// </summary>
        public float? ButtonColorShade { get; set; }

        /// <summary>Where the effect is drawn.</summary>
        public abstract ButtonEffectLayer Layer { get; }

        /// <summary>
        /// How the effect is mixed with what is below: <see cref="ButtonEffectBlend.Light"/> (added, the default of the
        /// lights) or <see cref="ButtonEffectBlend.Normal"/> (painted over, the default of shades, outlines and shadows).
        /// </summary>
        public ButtonEffectBlend Blend { get; set; } = ButtonEffectBlend.Light;

        /// <summary>The current state multiplier (it moves towards the multiplier of the state of the button).</summary>
        public float StateIntensity => stateIntensity;

        /// <summary>The intensity used to draw: <see cref="Intensity"/> times the state multiplier and the pulse.</summary>
        public float EffectiveIntensity => Intensity * stateIntensity * pulse;

        /// <summary>The button that owns the effect.</summary>
        internal object Owner { get; set; }

        /// <summary>The intensity multiplier of a state.</summary>
        public float GetStateIntensity(ButtonVisualState state)
        {
            switch (state)
            {
                case ButtonVisualState.Hover:
                    return HoverIntensity;
                case ButtonVisualState.Pressed:
                    return PressedIntensity;
                case ButtonVisualState.Disabled:
                    return DisabledIntensity;
                default:
                    return NormalIntensity;
            }
        }

        /// <summary>
        /// Called every frame while the button is on the screen. The base implementation moves the state multiplier
        /// towards the state of the button and computes the pulse; call it from overrides.
        /// </summary>
        public virtual void Update(ButtonEffectContext context)
        {
            var target = GetStateIntensity(context.State);
            if (!hasStateIntensity || TransitionSeconds <= 0f)
            {
                stateIntensity = target;
                hasStateIntensity = true;
            }
            else
            {
                var speed = Math.Max(Math.Abs(target - stateIntensity), 0.05f) / TransitionSeconds;
                stateIntensity = MathUtils.MoveTowards(stateIntensity, target, speed * context.DeltaSeconds);
            }

            pulse = PulseSpeed > 0f && PulseAmount > 0f
                ? 1f - MathUtils.Clamp01(PulseAmount) * (0.5f - 0.5f * (float)Math.Cos(GetLocalTime(context) * PulseSpeed * MathHelper.TwoPi))
                : 1f;
        }

        /// <summary>Draws the effect in the local space of the button (the sprite batch uses additive blending).</summary>
        public abstract void Draw(SpriteBatch spriteBatch, ButtonEffectContext context);

        /// <summary>Called when the button is pressed, with the position of the pointer in the local space of the button.</summary>
        public virtual void OnPressed(ButtonEffectContext context, Vector2 localPosition) { }

        /// <summary>Called when the button is released, with the position of the pointer in the local space of the button.</summary>
        public virtual void OnReleased(ButtonEffectContext context, Vector2 localPosition) { }

        /// <summary>Called when the visual state of the button changes.</summary>
        public virtual void OnStateChanged(ButtonEffectContext context, ButtonVisualState previousState) { }

        /// <summary>The time of the effect: the time of the button plus <see cref="TimeOffset"/>.</summary>
        protected float GetLocalTime(ButtonEffectContext context) => context.Time + TimeOffset;

        /// <summary>
        /// The current color: the shade of the background color of the button (<see cref="ButtonColorShade"/>), the
        /// current color of the <see cref="ColorCycle"/>, or <see cref="Color"/>.
        /// </summary>
        protected Color GetColor(ButtonEffectContext context)
        {
            if (ButtonColorShade.HasValue && context.BackgroundColor.HasValue)
                return context.GetShadeOfBackground(ButtonColorShade.Value, Color);

            var cycle = ColorCycle;
            if (cycle == null || cycle.Count == 0)
                return Color;
            if (cycle.Count == 1 || ColorCyclePeriod <= 0f)
                return cycle[0];

            var position = MathUtils.Wrap(GetLocalTime(context) / ColorCyclePeriod, 0f, 1f) * cycle.Count;
            var index = Math.Min(cycle.Count - 1, (int)position);
            return Color.Lerp(cycle[index], cycle[(index + 1) % cycle.Count], position - index);
        }

        /// <summary>The color to draw with: the current color times the effective intensity and the opacity of the button.</summary>
        protected Color GetTint(ButtonEffectContext context) => GetColor(context) * (EffectiveIntensity * context.Opacity);

        /// <summary>A color times the effective intensity and the opacity of the button (for secondary colors).</summary>
        protected Color ApplyIntensity(Color color, ButtonEffectContext context) => color * (EffectiveIntensity * context.Opacity);
    }

    /// <summary>
    /// Base class of the effects, with fluent setters that return the concrete effect type
    /// (eg: <c>new GlowEffect().SetColor(Color.Gold).SetRadius(16).SetPulse(1f, 0.4f)</c>).
    /// </summary>
    public abstract class ButtonEffect<TEffect> : ButtonEffect where TEffect : ButtonEffect<TEffect>
    {
        /// <summary>This instance typed as <typeparamref name="TEffect"/>, for fluent methods.</summary>
        protected TEffect ThisAsT => (TEffect)this;

        public TEffect SetIsEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            return ThisAsT;
        }

        /// <summary>Sets <see cref="ButtonEffect.Blend"/> (eg: a dark glow painted with <see cref="ButtonEffectBlend.Normal"/>).</summary>
        public TEffect SetBlend(ButtonEffectBlend blend)
        {
            Blend = blend;
            return ThisAsT;
        }

        public TEffect SetColor(Color color)
        {
            Color = color;
            return ThisAsT;
        }

        /// <summary>
        /// Uses the background color of the button: darker (0 to 1) or lighter (0 to -1); null = <see cref="ButtonEffect.Color"/>.
        /// </summary>
        public TEffect UseButtonColor(float? shade)
        {
            ButtonColorShade = shade;
            return ThisAsT;
        }

        public TEffect SetIntensity(float intensity)
        {
            Intensity = intensity;
            return ThisAsT;
        }

        /// <summary>Sets the intensity multipliers of the visual states of the button.</summary>
        public TEffect SetStateIntensities(float normal, float hover, float pressed, float disabled = 0f)
        {
            NormalIntensity = normal;
            HoverIntensity = hover;
            PressedIntensity = pressed;
            DisabledIntensity = disabled;
            return ThisAsT;
        }

        /// <summary>Shows the effect only while the pointer is over the button or pressing it.</summary>
        public TEffect VisibleOnHover() => SetStateIntensities(0f, 1f, 1f, 0f);

        /// <summary>Keeps the same intensity in every state, disabled included.</summary>
        public TEffect IgnoreStates() => SetStateIntensities(1f, 1f, 1f, 1f);

        public TEffect SetTransitionSeconds(float transitionSeconds)
        {
            TransitionSeconds = transitionSeconds;
            return ThisAsT;
        }

        /// <summary>Makes the light pulse: <paramref name="speed"/> pulses per second, dimming by <paramref name="amount"/> (0 to 1).</summary>
        public TEffect SetPulse(float speed, float amount)
        {
            PulseSpeed = speed;
            PulseAmount = amount;
            return ThisAsT;
        }

        public TEffect SetTimeOffset(float timeOffset)
        {
            TimeOffset = timeOffset;
            return ThisAsT;
        }

        /// <summary>Makes the light go through colors, one after the other, in <paramref name="period"/> seconds.</summary>
        public TEffect SetColorCycle(IList<Color> colors, float period = 3f)
        {
            ColorCycle = colors;
            ColorCyclePeriod = period;
            return ThisAsT;
        }
    }
}
