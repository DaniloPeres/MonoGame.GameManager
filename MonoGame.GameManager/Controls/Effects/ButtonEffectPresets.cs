using Microsoft.Xna.Framework;
using MonoGame.GameManager.Enums;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Ready-made combinations of effects. Each method returns new effects, so a preset can be given to many buttons;
    /// the effects can be changed after they are created. The static presets (<see cref="StaticNames"/>: Ornate,
    /// Starlight, Treasure, Candy, Jelly, Glossy...) do not move nor blink and use the background color of the button for their borders and lips, so they
    /// fit buttons of any color; the others are animated lights.
    /// </summary>
    /// <example>
    /// <code>
    /// new Button(new Vector2(100, 100), new Vector2(220, 64), new Color(120, 70, 10))
    ///     .SetCornerRadius(14)
    ///     .SetText(font, "PLAY")
    ///     .AddEffects(ButtonEffectPresets.Gold())
    ///     .AddToScreen();
    ///
    /// button.SetEffects(ButtonEffectPresets.Create("Magic"));
    /// </code>
    /// </example>
    public static class ButtonEffectPresets
    {
        private static readonly Dictionary<string, Func<List<ButtonEffect>>> factories = new Dictionary<string, Func<List<ButtonEffect>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Gold", Gold },
            { "Royal", Royal },
            { "Magic", Magic },
            { "Ice", Ice },
            { "Fire", Fire },
            { "Emerald", Emerald },
            { "Neon", Neon },
            { "Rainbow", Rainbow },
            { "Legendary", Legendary },
            { "Glass", Glass },
            { "Alert", Alert },
            { "Subtle", Subtle },
            { "Candy", Candy },
            { "Jelly", Jelly },
            { "Glossy", Glossy },
            { "Cartoon", Cartoon },
            { "Bubble", Bubble },
            { "Pearl", Pearl },
            { "Soft Glow", SoftGlow },
            { "Inner Light", InnerLight },
            { "Framed", Framed },
            { "Minimal", Minimal },
            { "Ornate", Ornate },
            { "Starlight", Starlight },
            { "Treasure", Treasure },
            { "Crystal", Crystal },
            { "Galaxy", Galaxy },
            { "Fairy", Fairy },
            { "Sunburst", Sunburst }
        };

        /// <summary>The names of the presets, usable with <see cref="Create"/>.</summary>
        public static IReadOnlyList<string> Names { get; } = new[]
        {
            "Ornate", "Starlight", "Treasure", "Crystal", "Galaxy", "Fairy", "Sunburst",
            "Candy", "Jelly", "Glossy", "Cartoon", "Bubble", "Pearl", "Soft Glow", "Inner Light", "Framed", "Minimal",
            "Gold", "Royal", "Magic", "Ice", "Fire", "Emerald", "Neon", "Rainbow", "Legendary", "Glass", "Alert", "Subtle"
        };

        /// <summary>The presets that do not move nor blink (only static lights, shades and borders).</summary>
        public static IReadOnlyList<string> StaticNames { get; } = new[]
        {
            "Ornate", "Starlight", "Treasure", "Crystal", "Galaxy", "Fairy", "Sunburst",
            "Candy", "Jelly", "Glossy", "Cartoon", "Bubble", "Pearl", "Soft Glow", "Inner Light", "Framed", "Minimal"
        };

        /// <summary>The colors of the <see cref="Rainbow"/> preset.</summary>
        public static readonly IList<Color> RainbowColors = new[]
        {
            new Color(255, 70, 70), new Color(255, 170, 40), new Color(250, 240, 70),
            new Color(70, 230, 110), new Color(60, 170, 255), new Color(190, 90, 255)
        };

        /// <summary>The colors of the <see cref="Neon"/> preset.</summary>
        public static readonly IList<Color> NeonColors = new[] { new Color(40, 240, 255), new Color(255, 60, 220) };

        /// <summary>Creates the effects of a preset by name (see <see cref="Names"/>).</summary>
        public static List<ButtonEffect> Create(string name)
        {
            if (name == null || !factories.TryGetValue(name, out var factory))
                throw new ArgumentException($"There is no button effect preset named '{name}'. Use one of: {string.Join(", ", Names)}.", nameof(name));
            return factory();
        }

        /// <summary>Warm gold: a pulsing halo, a pale gold rim, gloss, a gem with a streak on the top, sweeps and sparkles.</summary>
        public static List<ButtonEffect> Gold()
        {
            var gold = new Color(255, 185, 60);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(gold).SetRadius(16).SetPulse(0.8f, 0.35f),
                new GlossEffect().SetHeightRate(0.5f).SetIntensity(0.35f),
                new ShineSweepEffect().SetColor(new Color(255, 240, 200)).SetTiming(0.7f, 3f),
                new GlowEffect(GlowPlacement.Rim).SetColor(new Color(255, 230, 160)).SetThickness(2).SetSoftness(2).SetOffset(-3).SetIntensity(0.75f),
                new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Diamond).SetColor(gold).SetSize(14, 44).SetStreak(110, 4),
                new CornerLightEffect().SetAnchors(Anchor.TopLeft, Anchor.TopRight).SetColor(gold).SetSize(12, 24).SetTwinkle(1.6f, 0.6f).SetTimeOffset(0.4f),
                new SparkleEffect().SetColor(new Color(255, 220, 120)).SetRate(3)
            };
        }

        /// <summary>Deep blue with a gold rim and five gold gems, gloss and sweeps.</summary>
        public static List<ButtonEffect> Royal()
        {
            var blue = new Color(60, 140, 255);
            var gold = new Color(255, 200, 90);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(blue).SetRadius(14).SetPulse(0.6f, 0.25f),
                new GlowEffect(GlowPlacement.Inner).SetColor(new Color(110, 180, 255)).SetRadius(10).SetIntensity(0.5f),
                new GlossEffect().SetHeightRate(0.45f).SetIntensity(0.3f),
                new ShineSweepEffect().SetColor(new Color(200, 225, 255)).SetTiming(0.8f, 3.5f).SetAngle(25),
                new GlowEffect(GlowPlacement.Rim).SetColor(gold).SetThickness(2.5f).SetSoftness(1.5f).SetOffset(-1),
                new CornerLightEffect().SetAnchors(Anchor.TopCenter, Anchor.TopLeft, Anchor.TopRight, Anchor.BottomLeft, Anchor.BottomRight)
                    .SetShape(LightShape.Diamond).SetColor(gold).SetSize(10, 26).SetTwinkle(0.8f, 0.3f)
            };
        }

        /// <summary>Arcane purple: halo, inner light, two running lights, stars around and a ripple on click.</summary>
        public static List<ButtonEffect> Magic()
        {
            var purple = new Color(170, 80, 255);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(purple).SetRadius(18).SetPulse(1.2f, 0.4f),
                new GlowEffect(GlowPlacement.Inner).SetColor(new Color(220, 140, 255)).SetRadius(12).SetIntensity(0.6f),
                new ClickRippleEffect().SetColor(new Color(240, 200, 255)),
                new RunningLightEffect().SetColor(new Color(255, 110, 255)).SetCount(2).SetSpeed(200).SetSize(12),
                new SparkleEffect().SetColor(new Color(230, 170, 255)).SetShape(LightShape.Star).SetArea(SparkleArea.Around, 10).SetRate(8).SetSpinSpeed(120).SetClickBurst(14)
            };
        }

        /// <summary>Frozen cyan: a tight halo, strong gloss, diamonds on the corners, slow sweeps and tiny inner sparkles.</summary>
        public static List<ButtonEffect> Ice()
        {
            var ice = new Color(90, 200, 255);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(ice).SetRadius(14).SetFalloff(3),
                new GlossEffect().SetHeightRate(0.55f).SetIntensity(0.5f).SetColor(new Color(220, 245, 255)),
                new ShineSweepEffect().SetTiming(1f, 4f).SetAngle(30).SetWidth(28),
                new SparkleEffect().SetColor(new Color(200, 240, 255)).SetArea(SparkleArea.Inside, 4).SetRate(4).SetSize(8),
                new CornerLightEffect().SetAnchors(CornerLightEffect.Corners).SetShape(LightShape.Diamond).SetColor(ice).SetSize(10, 26).SetTwinkle(0.7f, 0.5f)
            };
        }

        /// <summary>Burning orange: a fast pulsing halo, a red inner glow, a flickering fill and rising embers.</summary>
        public static List<ButtonEffect> Fire()
        {
            var orange = new Color(255, 110, 30);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(orange).SetRadius(18).SetPulse(2f, 0.5f),
                new GlowEffect(GlowPlacement.Inner).SetColor(new Color(255, 70, 20)).SetRadius(14).SetIntensity(0.7f),
                new LightFillEffect().SetColor(new Color(255, 140, 40)).SetIntensity(0.15f).SetPulse(2.6f, 0.8f),
                new SparkleEffect().SetColor(new Color(255, 170, 60)).SetShape(LightShape.Glow).SetArea(SparkleArea.Top, 4)
                    .SetRate(16).SetSize(9, 0.5f).SetLifetime(0.6f, 1.3f).SetMotion(18, new Vector2(0, -70)).SetClickBurst(16)
            };
        }

        /// <summary>Nature green: a calm halo, a thin rim, gloss, a slow running light and leafy sparkles.</summary>
        public static List<ButtonEffect> Emerald()
        {
            var green = new Color(40, 220, 120);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(green).SetRadius(12).SetPulse(0.7f, 0.3f),
                new GlossEffect().SetHeightRate(0.45f).SetIntensity(0.3f),
                new GlowEffect(GlowPlacement.Rim).SetColor(new Color(160, 255, 190)).SetThickness(1.5f).SetSoftness(1.5f).SetIntensity(0.8f),
                new RunningLightEffect().SetColor(new Color(120, 255, 170)).SetSpeed(120).SetSize(10).SetTail(90),
                new SparkleEffect().SetColor(new Color(150, 255, 180)).SetRate(4)
            };
        }

        /// <summary>Neon sign: a thick rim and a halo cycling between cyan and magenta, and three fast white lights.</summary>
        public static List<ButtonEffect> Neon()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColorCycle(NeonColors, 4f).SetRadius(16).SetFalloff(1.6f).SetPulse(3f, 0.15f),
                new GlowEffect(GlowPlacement.Rim).SetColorCycle(NeonColors, 4f).SetThickness(3).SetSoftness(3),
                new RunningLightEffect().SetCount(3).SetSpeed(320).SetSize(8).SetTail(50, 8)
            };
        }

        /// <summary>Every color: a halo, a rim, two running lights and sparkles cycling through the rainbow.</summary>
        public static List<ButtonEffect> Rainbow()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColorCycle(RainbowColors, 4f).SetRadius(14).SetIntensity(0.6f),
                new OutlineEffect().SetColor(Color.White * 0.8f).SetThickness(2),
                new HighlightEffect().SetInset(8, 3).SetHeightRate(0.42f).SetIntensity(0.3f),
                new GlowEffect(GlowPlacement.Rim).SetColorCycle(RainbowColors, 4f).SetThickness(1.5f).SetSoftness(1.5f).SetOffset(-2).SetIntensity(0.7f),
                new RunningLightEffect().SetColorCycle(RainbowColors, 2f).SetCount(2).SetSpeed(200).SetSize(10).SetIntensity(0.8f),
                new SparkleEffect().SetColorCycle(RainbowColors, 1.5f).SetArea(SparkleArea.Around, 10).SetRate(5).SetClickBurst(12)
            };
        }

        /// <summary>A legendary reward: turning rays, a strong golden halo, a rim, a gem with a long streak, sweeps and sparkles.</summary>
        public static List<ButtonEffect> Legendary()
        {
            var gold = new Color(255, 190, 70);
            return new List<ButtonEffect>
            {
                new LightRaysEffect().SetColor(new Color(255, 200, 90)).SetRays(12, 0, 16).SetRotationSpeed(14),
                new GlowEffect().SetColor(new Color(255, 150, 40)).SetRadius(22).SetPulse(1f, 0.35f),
                new GlossEffect().SetIntensity(0.35f),
                new ShineSweepEffect().SetColor(new Color(255, 245, 210)).SetTiming(0.6f, 2f).SetWidth(44),
                new GlowEffect(GlowPlacement.Rim).SetColor(new Color(255, 225, 140)).SetThickness(2.5f).SetSoftness(2.5f),
                new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Diamond).SetColor(gold).SetSize(16, 54).SetStreak(160, 5),
                new SparkleEffect().SetColor(new Color(255, 215, 110)).SetArea(SparkleArea.Around, 12).SetRate(10).SetClickBurst(20)
            };
        }

        /// <summary>Polished glass: strong gloss, a soft reflection at the bottom, a faint rim, sweeps, inner light on hover and a ripple.</summary>
        public static List<ButtonEffect> Glass()
        {
            return new List<ButtonEffect>
            {
                new DropShadowEffect().SetOffset(new Vector2(0, 3)).SetBlur(6).SetIntensity(0.3f),
                new OutlineEffect().UseButtonColor(0.55f).SetThickness(1.5f),
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.18f),
                new HighlightEffect().SetInset(3, 2).SetHeightRate(0.48f).SetSoftness(0.03f).SetFade(0.3f).SetIntensity(0.32f),
                new GlossEffect().SetFromBottom(true).SetHeightRate(0.3f).SetIntensity(0.15f),
                new GlowEffect(GlowPlacement.Inner).SetRadius(10).SetIntensity(0.45f).VisibleOnHover(),
                new ShineSweepEffect().SetTiming(0.55f, 2.5f).SetWidth(24).SetIntensity(0.45f),
                new ClickRippleEffect().SetIntensity(0.6f),
                new GlowEffect(GlowPlacement.Rim).SetThickness(1.2f).SetSoftness(1).SetOffset(-1).SetIntensity(0.4f)
            };
        }

        /// <summary>Warning red: a strong pulsing halo and rim, and a red light on hover.</summary>
        public static List<ButtonEffect> Alert()
        {
            var red = new Color(255, 40, 40);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(red).SetRadius(16).SetPulse(1.6f, 0.85f),
                new LightFillEffect().SetColor(red).SetIntensity(0.3f).VisibleOnHover(),
                new GlowEffect(GlowPlacement.Rim).SetColor(new Color(255, 130, 130)).SetThickness(2).SetPulse(1.6f, 0.6f)
            };
        }

        /// <summary>Discreet: a faint gloss, a sweep and an inner light when the pointer enters, and a ripple on click.</summary>
        public static List<ButtonEffect> Subtle()
        {
            return new List<ButtonEffect>
            {
                new OutlineEffect().UseButtonColor(-0.35f).SetThickness(1),
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.15f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(6, 2).SetThickness(2).SetIntensity(0.22f),
                new GlowEffect(GlowPlacement.Inner).SetRadius(10).SetIntensity(0.4f).VisibleOnHover(),
                new ShineSweepEffect().SetTiming(0.5f, -1f).SetSweepOnHover(true).SetIntensity(0.5f),
                new ClickRippleEffect().SetIntensity(0.5f)
            };
        }

        // ---- Static presets: no movement, no blinking

        /// <summary>A candy button: a shadow, a thick lip, a white border, a glossy reflection, a shaded bottom, glints and outlined text.</summary>
        public static List<ButtonEffect> Candy()
        {
            return new List<ButtonEffect>
            {
                new DropShadowEffect().SetOffset(new Vector2(0, 7)).SetBlur(8).SetIntensity(0.3f),
                new DepthEffect().SetDepth(5).UseButtonColor(0.4f),
                FrameEffect.Candy(),
                new InnerShadeEffect().SetHeightRate(0.55f).SetIntensity(0.2f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Bottom).UseButtonColor(-0.4f).SetRadius(14).SetIntensity(0.3f),
                new HighlightEffect().SetInset(9, 4).SetHeightRate(0.42f).SetIntensity(0.42f),
                new TextOutlineEffect().UseButtonColor(0.62f).SetOutline(Color.Black, 2).SetShadow(new Vector2(0, 2), Color.Black * 0.25f),
                new GlintEffect().SetCorners(Anchor.TopLeft).SetDots(2, 6)
            };
        }

        /// <summary>A jelly button: a soft oval reflection, a deep shaded bottom lit from below, a glint and a faint halo of its own color.</summary>
        public static List<ButtonEffect> Jelly()
        {
            return new List<ButtonEffect>
            {
                new DropShadowEffect().SetOffset(new Vector2(0, 5)).SetBlur(10).SetIntensity(0.28f),
                new GlowEffect().UseButtonColor(-0.35f).SetRadius(10).SetFalloff(2.5f).SetIntensity(0.25f),
                new OutlineEffect().UseButtonColor(0.35f).SetThickness(1.5f),
                new InnerShadeEffect().SetHeightRate(0.6f).SetIntensity(0.28f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Bottom).UseButtonColor(-0.55f).SetRadius(18).SetIntensity(0.5f),
                new HighlightEffect().SetInset(12, 4).SetHeightRate(0.4f).SetFade(0.75f).SetIntensity(0.45f),
                new GlintEffect().SetCorners(Anchor.TopRight).SetDots(1, 6)
            };
        }

        /// <summary>A glossy button: a crisp top reflection, a thin light rim, a dark border and a shaded bottom.</summary>
        public static List<ButtonEffect> Glossy()
        {
            return new List<ButtonEffect>
            {
                new OutlineEffect().UseButtonColor(0.55f).SetThickness(2),
                new InnerShadeEffect().SetHeightRate(0.45f).SetIntensity(0.2f),
                new HighlightEffect().SetInset(4, 3).SetHeightRate(0.46f).SetSoftness(0.04f).SetFade(0.35f).SetIntensity(0.35f),
                new GlowEffect(GlowPlacement.Rim).UseButtonColor(-0.7f).SetThickness(1.5f).SetSoftness(1).SetOffset(-1).SetIntensity(0.45f).IgnoreStates()
            };
        }

        /// <summary>A cartoon button: a thick dark border, a 3D lip, a light strip at the top and bold outlined text.</summary>
        public static List<ButtonEffect> Cartoon()
        {
            return new List<ButtonEffect>
            {
                new DepthEffect().SetDepth(6).UseButtonColor(0.55f),
                new OutlineEffect().UseButtonColor(0.72f).SetThickness(3),
                new InnerShadeEffect().SetHeightRate(0.4f).SetIntensity(0.18f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(10, 5).SetThickness(4).SetIntensity(0.45f),
                new TextOutlineEffect().UseButtonColor(0.72f).SetOutline(Color.Black, 2.5f).SetShadow(new Vector2(0, 3), Color.Black * 0.35f)
            };
        }

        /// <summary>A bubble: light inside the whole edge, an oval reflection, three glints and a thin white border.</summary>
        public static List<ButtonEffect> Bubble()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().UseButtonColor(-0.5f).SetRadius(9).SetIntensity(0.25f),
                new OutlineEffect().SetColor(Color.White * 0.55f).SetThickness(1.5f),
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(12).SetIntensity(0.18f),
                new GlowEffect(GlowPlacement.Inner).UseButtonColor(-0.7f).SetRadius(10).SetIntensity(0.45f),
                new HighlightEffect(HighlightStyle.Ellipse).SetInset(12, 3).SetHeightRate(0.38f).SetIntensity(0.4f),
                new GlintEffect().SetCorners(Anchor.TopLeft).SetDots(3, 6)
            };
        }

        /// <summary>A pearl: a soft white light inside, a faint halo, a gentle reflection and a light shade.</summary>
        public static List<ButtonEffect> Pearl()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().SetRadius(12).SetIntensity(0.2f),
                new OutlineEffect().UseButtonColor(-0.5f).SetThickness(1.5f),
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.12f),
                new GlowEffect(GlowPlacement.Inner).SetRadius(14).SetFalloff(1.5f).SetIntensity(0.3f),
                new HighlightEffect().SetInset(10, 4).SetHeightRate(0.42f).SetSoftness(0.3f).SetFade(0.7f).SetIntensity(0.3f)
            };
        }

        /// <summary>Only a soft halo of the color of the button and a faint strip of light, without movement.</summary>
        public static List<ButtonEffect> SoftGlow()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().UseButtonColor(-0.4f).SetRadius(16).SetIntensity(0.4f),
                new GlowEffect(GlowPlacement.Inner).UseButtonColor(-0.5f).SetRadius(8).SetIntensity(0.3f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(14, 4).SetThickness(3).SetIntensity(0.28f)
            };
        }

        /// <summary>Light that falls inside the button from the top, a thin inner rim and a light shade at the bottom.</summary>
        public static List<ButtonEffect> InnerLight()
        {
            return new List<ButtonEffect>
            {
                new OutlineEffect().UseButtonColor(0.5f).SetThickness(1.5f),
                new InnerShadeEffect().SetHeightRate(0.45f).SetIntensity(0.2f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Top).UseButtonColor(-0.75f).SetRadius(28).SetFalloff(1.6f).SetIntensity(0.55f),
                new GlowEffect(GlowPlacement.Rim).UseButtonColor(-0.8f).SetThickness(1.5f).SetSoftness(1).SetOffset(-2).SetIntensity(0.5f).IgnoreStates()
            };
        }

        /// <summary>A frame of three rings made of the color of the button, a light line inside it, a strip and a shadow.</summary>
        public static List<ButtonEffect> Framed()
        {
            return new List<ButtonEffect>
            {
                new DropShadowEffect().SetOffset(new Vector2(0, 5)).SetBlur(7).SetIntensity(0.3f),
                FrameEffect.ButtonColor(),
                new InnerShadeEffect().SetHeightRate(0.45f).SetIntensity(0.15f),
                new OutlineEffect(OutlinePosition.Inside).SetColor(Color.White * 0.45f).SetOffset(3).SetThickness(1.5f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(10, 7).SetThickness(3).SetIntensity(0.3f)
            };
        }

        /// <summary>Barely there: a thin border, a thin strip of light and a light shade.</summary>
        public static List<ButtonEffect> Minimal()
        {
            return new List<ButtonEffect>
            {
                new OutlineEffect().UseButtonColor(0.4f).SetThickness(1),
                new InnerShadeEffect().SetHeightRate(0.4f).SetIntensity(0.12f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(8, 3).SetThickness(2).SetIntensity(0.28f)
            };
        }

        // ---- Rich static presets: a lot of glow and particles, without movement

        /// <summary>
        /// An ornate fantasy button: a halo of its color, a golden frame, a lit inner edge, a dark vignette, a soft
        /// reflection, glowing particles gathered at the bottom, sparkles on the top corners and a gem with a streak.
        /// </summary>
        public static List<ButtonEffect> Ornate()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().UseButtonColor(-0.35f).SetRadius(16).SetFalloff(2.2f).SetIntensity(0.55f).IgnoreStates(),
                FrameEffect.Gold(),
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(16).SetFalloff(1.6f).SetIntensity(0.4f),
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.12f),
                new GlowEffect(GlowPlacement.Inner).UseButtonColor(-0.6f).SetRadius(9).SetFalloff(2.4f).SetIntensity(0.7f),
                new HighlightEffect().SetInset(12, 4).SetHeightRate(0.38f).SetFade(0.75f).SetIntensity(0.22f),
                new ParticleFieldEffect(ParticleFieldArea.Bottom).UseButtonColor(-0.7f).SetCount(16).SetSize(3, 10).SetSeed(11),
                new ParticleFieldEffect().SetShapes(LightShape.Flare, LightShape.Circle).SetCount(6).SetSize(3, 6).SetIntensity(0.35f).SetSeed(3),
                new GlowEffect(GlowPlacement.Rim).UseButtonColor(-0.8f).SetThickness(1.2f).SetSoftness(1.5f).SetOffset(-2).SetIntensity(0.55f).IgnoreStates(),
                new CornerLightEffect().SetAnchors(Anchor.TopLeft, Anchor.TopRight).UseButtonColor(-0.8f).SetSize(14, 26).SetTwinkle(0f, 0f).SetOffset(-5),
                new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Diamond).SetColor(new Color(255, 215, 120)).SetSize(11, 30).SetStreak(90, 3).SetTwinkle(0f, 0f)
            };
        }

        /// <summary>A night sky: a silver frame, a cool halo, a field of small stars and big soft lights at the bottom.</summary>
        public static List<ButtonEffect> Starlight()
        {
            var cyan = new Color(90, 200, 255);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(cyan).SetRadius(14).SetIntensity(0.45f).IgnoreStates(),
                FrameEffect.Silver(),
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(14).SetIntensity(0.35f),
                new GlowEffect(GlowPlacement.Inner).SetColor(new Color(80, 160, 255)).SetRadius(10).SetIntensity(0.5f),
                new ParticleFieldEffect(ParticleFieldArea.Bottom).SetColor(new Color(60, 140, 255)).SetShapes(LightShape.Glow).SetCount(8).SetSize(10, 22)
                    .SetOpacityMin(0.1f).SetCoreRate(0f).SetIntensity(0.35f).SetSeed(5),
                new ParticleFieldEffect().SetShapes(LightShape.Circle, LightShape.Circle, LightShape.Flare, LightShape.Star)
                    .SetColors(Color.White, new Color(200, 235, 255), new Color(255, 240, 200)).SetCount(24).SetSize(1.5f, 7).SetIntensity(0.4f).SetSeed(21),
                new HighlightEffect(HighlightStyle.Strip).SetInset(14, 5).SetThickness(3).SetIntensity(0.22f),
                new CornerLightEffect().SetAnchors(Anchor.TopRight).SetShape(LightShape.Star).SetColor(cyan).SetSize(14, 30).SetTwinkle(0f, 0f).SetOffset(-7)
            };
        }

        /// <summary>A treasure: still golden rays behind it, a warm halo, a golden frame, golden bokeh, glints and a reflection.</summary>
        public static List<ButtonEffect> Treasure()
        {
            var gold = new Color(255, 200, 70);
            return new List<ButtonEffect>
            {
                new LightRaysEffect().SetColor(gold).SetRays(10, 0, 14).SetRotationSpeed(0).SetFlicker(0).SetIntensity(0.35f).IgnoreStates(),
                new GlowEffect().SetColor(new Color(255, 160, 40)).SetRadius(18).SetIntensity(0.5f).IgnoreStates(),
                FrameEffect.Gold(),
                new InnerShadeEffect().SetHeightRate(0.55f).SetIntensity(0.2f),
                new ParticleFieldEffect().SetColors(gold, new Color(255, 240, 180), new Color(255, 150, 40)).SetCount(16).SetSize(3, 11).SetSeed(8),
                new HighlightEffect().SetInset(10, 4).SetHeightRate(0.4f).SetIntensity(0.35f),
                new GlintEffect().SetCorners(Anchor.TopLeft).SetDots(2, 6),
                new ParticleFieldEffect(ParticleFieldArea.Above).SetColor(gold).SetShapes(LightShape.Flare, LightShape.Glow).SetCount(5).SetSize(5, 10).SetSpread(16).SetIntensity(0.35f).SetSeed(4)
            };
        }

        /// <summary>A crystal: a silver frame, a clear reflection, light inside the edges, diamonds at the top and a cold halo.</summary>
        public static List<ButtonEffect> Crystal()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(new Color(150, 230, 255)).SetRadius(12).SetIntensity(0.35f).IgnoreStates(),
                FrameEffect.Silver().SetThicknessScale(0.8f),
                new GlowEffect(GlowPlacement.Inner).SetRadius(12).SetIntensity(0.45f),
                new HighlightEffect().SetInset(6, 3).SetHeightRate(0.48f).SetSoftness(0.03f).SetFade(0.4f).SetIntensity(0.32f),
                new ParticleFieldEffect(ParticleFieldArea.Top).SetShapes(LightShape.Diamond, LightShape.Flare, LightShape.Circle).SetCount(10).SetSize(3, 8).SetSeed(13),
                new GlintEffect().SetCorners(Anchor.TopLeft, Anchor.TopRight).SetDots(2, 5),
                new ParticleFieldEffect(ParticleFieldArea.Edge).SetShapes(LightShape.Flare).SetCount(4).SetSize(7, 12).SetSpread(6).SetIntensity(0.35f).SetSeed(2)
            };
        }

        /// <summary>A galaxy: a magenta halo, a jewel frame, a dark vignette, a dense field of colored stars and big soft nebulas.</summary>
        public static List<ButtonEffect> Galaxy()
        {
            var pink = new Color(255, 90, 220);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(pink).SetRadius(18).SetIntensity(0.45f).IgnoreStates(),
                FrameEffect.Gem(new Color(180, 90, 255)),
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(18).SetIntensity(0.45f),
                new ParticleFieldEffect().SetColors(pink, new Color(90, 200, 255), new Color(170, 110, 255)).SetShapes(LightShape.Glow)
                    .SetCount(8).SetSize(14, 28).SetOpacityMin(0.1f).SetCoreRate(0f).SetIntensity(0.35f).SetSeed(17),
                new ParticleFieldEffect().SetColors(Color.White, new Color(255, 200, 250), new Color(190, 230, 255))
                    .SetShapes(LightShape.Circle, LightShape.Circle, LightShape.Flare).SetCount(30).SetSize(1.5f, 5).SetIntensity(0.4f).SetSeed(29),
                new HighlightEffect(HighlightStyle.Strip).SetInset(14, 5).SetThickness(2.5f).SetIntensity(0.25f)
            };
        }

        /// <summary>A fairy button: a green halo, a frame of its own color, sparkles floating above it and around it, and a reflection.</summary>
        public static List<ButtonEffect> Fairy()
        {
            var light = new Color(200, 255, 140);
            return new List<ButtonEffect>
            {
                new GlowEffect().UseButtonColor(-0.5f).SetRadius(16).SetIntensity(0.45f).IgnoreStates(),
                FrameEffect.ButtonColor(),
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.18f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Bottom).UseButtonColor(-0.6f).SetRadius(16).SetIntensity(0.45f),
                new HighlightEffect().SetInset(10, 4).SetHeightRate(0.4f).SetIntensity(0.35f),
                new ParticleFieldEffect(ParticleFieldArea.Bottom).SetColor(light).SetShapes(LightShape.Glow, LightShape.Flare).SetCount(8).SetSize(3, 8).SetSeed(6),
                new ParticleFieldEffect(ParticleFieldArea.Above).SetColors(light, new Color(255, 250, 180), Color.White)
                    .SetShapes(LightShape.Flare, LightShape.Glow, LightShape.Star).SetCount(10).SetSize(4, 9).SetSpread(22).SetSeed(9),
                new ParticleFieldEffect(ParticleFieldArea.Around).SetColor(light).SetShapes(LightShape.Glow, LightShape.Circle).SetCount(8).SetSize(2, 5).SetSpread(12).SetIntensity(0.3f).SetSeed(15)
            };
        }

        /// <summary>A sunburst: still rays and a strong warm halo behind it, a golden frame, a big flare with a streak on top and warm sparkles.</summary>
        public static List<ButtonEffect> Sunburst()
        {
            var warm = new Color(255, 190, 70);
            return new List<ButtonEffect>
            {
                new LightRaysEffect().SetColor(warm).SetRays(14, 0, 18).SetShortRayRate(0.55f).SetRotationSpeed(0).SetFlicker(0).SetIntensity(0.45f).IgnoreStates(),
                new GlowEffect().SetColor(new Color(255, 140, 30)).SetRadius(22).SetIntensity(0.6f).IgnoreStates(),
                FrameEffect.Gold(),
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.18f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Top).SetColor(new Color(255, 230, 150)).SetRadius(22).SetIntensity(0.4f),
                new ParticleFieldEffect(ParticleFieldArea.Bottom).SetColors(warm, new Color(255, 240, 200)).SetCount(10).SetSize(3, 9).SetSeed(19),
                new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Flare).SetColor(warm).SetSize(22, 60).SetStreak(150, 4).SetTwinkle(0f, 0f)
            };
        }
    }
}
