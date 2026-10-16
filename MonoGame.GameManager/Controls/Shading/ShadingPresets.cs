using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// Ready-made combinations of shading effects for texts and images. Each method returns new effects, so a preset can
    /// be given to many controls; the effects can be changed after they are created. The animated presets (<see
    /// cref="AnimatedNames"/>) flicker, pulse, breathe, orbit, cycle colors or shine; the others stay still.
    /// </summary>
    /// <example>
    /// <code>
    /// new Label(font, "GAME OVER", new Vector2(100, 100), Color.White)
    ///     .AddShadings(ShadingPresets.Neon(Color.Magenta))
    ///     .AddToScreen();
    ///
    /// icon.SetShadings(ShadingPresets.Create("Sticker"));
    /// </code>
    /// </example>
    public static class ShadingPresets
    {
        private static readonly Dictionary<string, Func<List<ShadingEffect>>> factories = new Dictionary<string, Func<List<ShadingEffect>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Soft Shadow", SoftShadow },
            { "Hard Shadow", HardShadow },
            { "Long Shadow", LongShadow },
            { "Sticker", Sticker },
            { "Comic", Comic },
            { "Candy", Candy },
            { "Emboss", Emboss },
            { "Retro", Retro },
            { "Double Outline", () => DoubleOutline(Color.White, new Color(30, 30, 60)) },
            { "Neon", () => Neon(NeonPink) },
            { "Gold Title", GoldTitle },
            { "Fire", Fire },
            { "Ice", Ice },
            { "Magic", Magic },
            { "Rainbow", Rainbow },
            { "Toxic", Toxic },
            { "Plasma", Plasma },
            { "Heartbeat", Heartbeat },
            { "Ghost", Ghost },
            { "Hologram", Hologram },
            { "Selected", Selected },
            { "Shiny", Shiny },
            { "Glitch", Glitch },
            { "Gold Map", GoldMap },
            { "Chrome", Chrome },
            { "Ice Crystal", IceCrystal },
            { "Candy Fill", CandyFill },
            { "Emerald", Emerald },
            { "Sunset Fill", SunsetFill },
            { "Lava", Lava },
            { "Holographic", Holographic },
            { "Fantasy Logo", FantasyLogo },
            { "Arena Title", ArenaTitle },
            { "Hero Title", HeroTitle }
        };

        /// <summary>The names of the presets, usable with <see cref="Create"/>.</summary>
        public static IReadOnlyList<string> Names { get; } = new[]
        {
            "Gold Title", "Fire", "Neon", "Ice", "Magic", "Rainbow", "Toxic", "Plasma", "Heartbeat", "Ghost", "Hologram",
            "Selected", "Shiny", "Glitch", "Lava", "Holographic", "Fantasy Logo", "Gold Map", "Arena Title", "Hero Title", "Chrome", "Ice Crystal", "Candy Fill", "Emerald",
            "Sunset Fill", "Soft Shadow", "Hard Shadow", "Long Shadow", "Sticker", "Comic", "Candy", "Emboss", "Retro",
            "Double Outline"
        };

        /// <summary>The presets that move: they flicker, pulse, breathe, orbit, cycle colors or shine.</summary>
        public static IReadOnlyList<string> AnimatedNames { get; } = new[]
        {
            "Gold Title", "Fire", "Neon", "Ice", "Magic", "Rainbow", "Toxic", "Plasma", "Heartbeat", "Ghost", "Hologram",
            "Selected", "Shiny", "Glitch", "Lava", "Holographic", "Fantasy Logo"
        };

        /// <summary>
        /// The presets that paint the glyphs with a <see cref="GradientFill"/> (the control is not drawn: its own color
        /// does not show).
        /// </summary>
        public static IReadOnlyList<string> FillNames { get; } = new[]
        {
            "Gold Map", "Chrome", "Ice Crystal", "Candy Fill", "Emerald", "Sunset Fill", "Lava", "Holographic", "Fantasy Logo", "Arena Title",
            "Hero Title"
        };

        /// <summary>The pink of the <see cref="Neon(Color)"/> preset created by name.</summary>
        public static readonly Color NeonPink = new Color(255, 60, 200);

        /// <summary>The colors of the <see cref="Magic"/> preset.</summary>
        public static readonly IList<Color> MagicColors = new[] { new Color(190, 80, 255), new Color(255, 90, 210), new Color(90, 150, 255) };

        /// <summary>The colors of the <see cref="Rainbow"/> preset.</summary>
        public static readonly IList<Color> RainbowColors = new[]
        {
            new Color(255, 70, 70), new Color(255, 170, 40), new Color(250, 240, 70),
            new Color(70, 230, 110), new Color(60, 170, 255), new Color(190, 90, 255)
        };

        /// <summary>The colors of the <see cref="Fire"/> preset, from the outer glow to the core.</summary>
        public static readonly IList<Color> FireColors = new[] { new Color(255, 40, 10), new Color(255, 120, 20), new Color(255, 210, 90) };

        /// <summary>Creates the effects of a preset by name (see <see cref="Names"/>).</summary>
        public static List<ShadingEffect> Create(string name)
        {
            if (name == null || !factories.TryGetValue(name, out var factory))
                throw new ArgumentException($"There is no shading preset named '{name}'. Use one of: {string.Join(", ", Names)}.", nameof(name));
            return factory();
        }

        /// <summary>
        /// A bevel: a light edge at the top of the silhouette and a dark edge at its bottom, as if it were raised (two
        /// <see cref="InnerShadow"/> effects drawn over the control). Add it after a <see cref="GradientFill"/>.
        /// </summary>
        /// <param name="highlight">The color of the top edge (its alpha is its strength), added as light.</param>
        /// <param name="shade">The color of the bottom edge (its alpha is its strength).</param>
        /// <param name="depth">How wide the edges are, in local units.</param>
        /// <param name="softness">How soft the edges are, in local units.</param>
        public static List<ShadingEffect> Bevel(Color highlight, Color shade, float depth = 2f, float softness = 2f) => new List<ShadingEffect>
        {
            new InnerShadow(new Vector2(0f, depth), softness, highlight).SetBlend(ShadingBlend.Light),
            new InnerShadow(new Vector2(0f, -depth), softness, shade)
        };

        /// <summary>
        /// A glossy band over the top part of the glyphs (a light that fades from <paramref name="strength"/> to a third of it,
        /// with a hard bottom edge), added as light: the shine of candy, plastic and casual game titles.
        /// </summary>
        /// <param name="strength">The strength of the gloss at the top, from 0 to 1.</param>
        /// <param name="bottom">Where the gloss ends, as a fraction of the glyphs (0.5 = their middle).</param>
        public static GradientOverlay Gloss(float strength = 0.6f, float bottom = 0.48f)
            => GradientOverlay.Band(Color.White, strength, strength / 3f, 0f, bottom).SetBlend(ShadingBlend.Light);

        // ---- Layered presets (many effects inside the glyphs)

        /// <summary>
        /// A fantasy game logo: polished gold letters with metal reflections, a light rim, a bevel and twinkling star
        /// glints, a dark outline and a blue magic glow behind.
        /// </summary>
        public static List<ShadingEffect> FantasyLogo()
        {
            var effects = new List<ShadingEffect>
            {
                new Shadow(new Vector2(0, 5), 5, Color.Black * 0.6f).SetSpread(4),
                new Glow(new Color(60, 120, 255), 18).SetIntensity(0.9f),
                new Outline(new Color(12, 16, 50), 5),
                new Outline(new Color(120, 70, 20), 2.5f),
                new GradientFill(new Color(255, 252, 215), new Color(245, 190, 70), new Color(150, 80, 20), new Color(255, 225, 130), new Color(200, 120, 30))
                    .SetPositions(0f, 0.42f, 0.5f, 0.62f, 1f).SetHardness(0.3f),
                new PatternOverlay(ShadingPattern.Noise, new Color(120, 60, 0) * 0.25f, 6)
            };
            effects.AddRange(Bevel(Color.White * 0.9f, new Color(90, 40, 0) * 0.6f, 1.5f, 1f));
            effects.Add(new InnerGlow(new Color(255, 250, 220), 1.5f).SetIntensity(0.7f));
            effects.Add(new Sparkles(Color.White, 12, 20).SetTwinkle(0.6f));
            effects.Add(new Shine(new Color(255, 250, 225)).SetTiming(1f, 3.5f).SetWidth(30));
            return effects;
        }

        /// <summary>
        /// An arena title: chunky orange and gold letters with a glossy top band, a dark lower edge, a thick dark outline and
        /// a deep shadow (the titles of mobile strategy games).
        /// </summary>
        public static List<ShadingEffect> ArenaTitle()
        {
            var effects = new List<ShadingEffect>
            {
                new Shadow(new Vector2(0, 6), 2, new Color(20, 10, 5) * 0.8f).SetSpread(5),
                new Outline(new Color(30, 18, 10), 5),
                new GradientFill(new Color(255, 225, 100), new Color(255, 160, 25), new Color(225, 95, 0)),
                Gloss(0.55f, 0.45f),
                new InnerShadow(new Vector2(0, -2.5f), 1.5f, new Color(120, 40, 0) * 0.7f)
            };
            effects.AddRange(Bevel(Color.White * 0.6f, new Color(80, 25, 0) * 0.3f, 1.2f, 1f));
            return effects;
        }

        /// <summary>
        /// A hero title: warm orange letters with a gloss band, a dark brown outline inside a thick golden one, a soft
        /// bevel and a warm glow.
        /// </summary>
        public static List<ShadingEffect> HeroTitle()
        {
            var effects = new List<ShadingEffect>
            {
                new Shadow(new Vector2(0, 5), 4, Color.Black * 0.5f).SetSpread(7),
                new Glow(new Color(255, 170, 40), 14).SetIntensity(0.6f),
                new Outline(new Color(255, 205, 90), 7.5f),
                new Outline(new Color(95, 30, 0), 4.5f),
                new GradientFill(new Color(255, 245, 200), new Color(255, 175, 45), new Color(240, 110, 10)).SetPositions(0f, 0.5f, 1f),
                Gloss(0.7f, 0.42f)
            };
            effects.AddRange(Bevel(Color.White * 0.7f, new Color(140, 40, 0) * 0.5f, 1.5f, 1.2f));
            return effects;
        }

        // ---- Fill presets (gradient letters)

        /// <summary>
        /// The golden title of casual game maps: a cream to gold to orange gradient, a light bevel, a thick dark brown
        /// outline, a dark shadow below and a warm glow around.
        /// </summary>
        public static List<ShadingEffect> GoldMap()
        {
            var effects = new List<ShadingEffect>
            {
                new Shadow(new Vector2(0, 5), 3, Color.Black * 0.55f).SetSpread(3.5f),
                new Glow(new Color(255, 170, 40), 14).SetIntensity(0.8f),
                new Outline(new Color(75, 32, 6), 3.5f),
                new GradientFill(new Color(255, 252, 220), new Color(255, 222, 100), new Color(242, 150, 22)).SetPositions(0f, 0.45f, 1f)
            };
            effects.AddRange(Bevel(new Color(255, 255, 240) * 0.85f, new Color(150, 60, 0) * 0.55f, 1.5f, 1.2f));
            return effects;
        }

        /// <summary>Polished chrome: a hard-edged gray gradient (the horizon of a metal), a white top edge and a dark outline.</summary>
        public static List<ShadingEffect> Chrome()
        {
            var effects = new List<ShadingEffect>
            {
                new Shadow(new Vector2(0, 4), 4, Color.Black * 0.6f).SetSpread(2.5f),
                new Outline(new Color(30, 35, 45), 2.5f),
                new GradientFill(new Color(250, 252, 255), new Color(170, 180, 195), new Color(70, 78, 92), new Color(200, 210, 225))
                    .SetPositions(0f, 0.48f, 0.52f, 1f)
            };
            effects.AddRange(Bevel(Color.White * 0.9f, Color.Black * 0.4f, 1.2f, 1f));
            return effects;
        }

        /// <summary>Ice: a white to cyan to blue gradient, a cold inner glow, a pale outline and a blue halo.</summary>
        public static List<ShadingEffect> IceCrystal() => new List<ShadingEffect>
        {
            new Glow(new Color(60, 160, 255), 16).SetIntensity(0.9f),
            new Outline(new Color(20, 60, 130), 3f),
            new Outline(new Color(210, 245, 255), 1.5f),
            new GradientFill(Color.White, new Color(150, 230, 255), new Color(40, 130, 230)),
            new InnerGlow(new Color(200, 245, 255), 3f).SetIntensity(0.8f),
            new InnerShadow(new Vector2(0, -2), 1.5f, new Color(0, 40, 120) * 0.5f)
        };

        /// <summary>Candy: a pink to magenta gradient with a glossy top edge, a white outline and a deep pink lip.</summary>
        public static List<ShadingEffect> CandyFill()
        {
            var effects = new List<ShadingEffect>
            {
                new Shadow(new Vector2(0, 7), 5, Color.Black * 0.35f).SetSpread(4),
                new Shadow(new Vector2(0, 4), 0, new Color(150, 20, 90)).SetSpread(4),
                new Outline(Color.White, 4),
                new GradientFill(new Color(255, 200, 235), new Color(255, 90, 180), new Color(220, 30, 140))
            };
            effects.AddRange(Bevel(Color.White * 0.9f, new Color(120, 0, 60) * 0.4f, 2f, 1.5f));
            return effects;
        }

        /// <summary>An emerald: a light to deep green gradient with a hard middle, a bevel and a dark green outline.</summary>
        public static List<ShadingEffect> Emerald()
        {
            var effects = new List<ShadingEffect>
            {
                new Shadow(new Vector2(0, 4), 4, Color.Black * 0.5f).SetSpread(3),
                new Glow(new Color(40, 255, 140), 12).SetIntensity(0.6f),
                new Outline(new Color(5, 50, 25), 3f),
                new GradientFill(new Color(190, 255, 210), new Color(40, 200, 110), new Color(0, 110, 60)).SetHardness(0.35f)
            };
            effects.AddRange(Bevel(Color.White * 0.75f, Color.Black * 0.45f, 1.5f, 1f));
            return effects;
        }

        /// <summary>A sunset: a yellow to orange to purple gradient, a dark purple outline and a soft shadow.</summary>
        public static List<ShadingEffect> SunsetFill() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 5), 6, new Color(40, 0, 50) * 0.6f).SetSpread(2),
            new Outline(new Color(50, 10, 60), 3f),
            new GradientFill(new Color(255, 240, 120), new Color(255, 120, 60), new Color(200, 40, 120), new Color(110, 30, 160)),
            new InnerShadow(new Vector2(0, 2), 1.5f, Color.White * 0.6f).SetBlend(ShadingBlend.Light)
        };

        /// <summary>Lava: a molten gradient flowing slowly upward, a glowing rim and a flickering red glow.</summary>
        public static List<ShadingEffect> Lava() => new List<ShadingEffect>
        {
            new Glow(new Color(255, 60, 0), 20).SetIntensity(1.2f).SetFlicker(0.35f, 4f),
            new Outline(new Color(50, 8, 0), 3f),
            new GradientFill(new Color(255, 230, 90), new Color(255, 100, 0), new Color(170, 20, 0), new Color(255, 100, 0), new Color(255, 230, 90))
                .SetAngle(-90).SetScroll(0.25f),
            new InnerGlow(new Color(255, 210, 80), 3f).SetIntensity(0.9f).SetPulse(0.8f, 0.4f)
        };

        /// <summary>A holographic foil: a rainbow gradient sliding diagonally across the glyphs, with a shine.</summary>
        public static List<ShadingEffect> Holographic() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 4), 5, Color.Black * 0.5f).SetSpread(2),
            new Outline(new Color(20, 20, 40), 2.5f),
            new GradientFill(new Color(255, 120, 200), new Color(120, 200, 255), new Color(160, 255, 170), new Color(255, 240, 140), new Color(255, 120, 200))
                .SetAngle(30).SetScroll(0.3f),
            new InnerShadow(new Vector2(0, 2), 1.5f, Color.White * 0.7f).SetBlend(ShadingBlend.Light),
            new Shine(Color.White).SetTiming(1f, 3f).SetWidth(30)
        };

        // ---- Still presets

        /// <summary>A soft dark shadow below.</summary>
        public static List<ShadingEffect> SoftShadow() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 4), 8, Color.Black * 0.6f)
        };

        /// <summary>A sharp dark shadow, down and to the right.</summary>
        public static List<ShadingEffect> HardShadow() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(3, 4), 0, Color.Black * 0.75f)
        };

        /// <summary>A long diagonal shadow made of four sharp shadows that fade.</summary>
        public static List<ShadingEffect> LongShadow() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(8, 8), 0, Color.Black * 0.15f),
            new Shadow(new Vector2(6, 6), 0, Color.Black * 0.2f),
            new Shadow(new Vector2(4, 4), 0, Color.Black * 0.25f),
            new Shadow(new Vector2(2, 2), 0, Color.Black * 0.3f)
        };

        /// <summary>A thick white border and a soft shadow, like a sticker.</summary>
        public static List<ShadingEffect> Sticker() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 4), 6, Color.Black * 0.5f).SetSpread(4),
            new Outline(Color.White, 4)
        };

        /// <summary>A comic title: a thick black outline and a bold sharp shadow.</summary>
        public static List<ShadingEffect> Comic() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(4, 5), 0, Color.Black).SetSpread(3),
            new Outline(Color.Black, 3)
        };

        /// <summary>A candy title: a white outline over a deep pink lip and a soft shadow.</summary>
        public static List<ShadingEffect> Candy() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 8), 6, Color.Black * 0.35f).SetSpread(4),
            new Shadow(new Vector2(0, 4), 0, new Color(160, 20, 90)).SetSpread(4),
            new Outline(Color.White, 4)
        };

        /// <summary>Embossed: a light edge on the top left and a dark edge on the bottom right.</summary>
        public static List<ShadingEffect> Emboss() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(-1.5f, -1.5f), 1, Color.White * 0.55f),
            new Shadow(new Vector2(1.5f, 1.5f), 1.5f, Color.Black * 0.65f)
        };

        /// <summary>Synthwave: two sharp colored shadows, pink and cyan.</summary>
        public static List<ShadingEffect> Retro() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(4, 4), 0, new Color(60, 220, 255)),
            new Shadow(new Vector2(2, 2), 0, new Color(255, 60, 170))
        };

        /// <summary>Two borders: <paramref name="inner"/> around the control and <paramref name="outer"/> around it.</summary>
        public static List<ShadingEffect> DoubleOutline(Color inner, Color outer) => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 4), 4, Color.Black * 0.5f).SetSpread(6),
            new Outline(outer, 6),
            new Outline(inner, 3)
        };

        // ---- Animated presets

        /// <summary>
        /// A neon sign: three glows of a color (a wide haze, a halo and a bright tube) that flicker like an old neon
        /// tube.
        /// </summary>
        public static List<ShadingEffect> Neon(Color color) => new List<ShadingEffect>
        {
            new Glow(color, 34).SetIntensity(0.9f).SetFlicker(0.35f, 5f).SetBreathe(0.4f, 0.03f),
            new Glow(color, 12).SetIntensity(1.3f).SetFlicker(0.3f, 5f),
            new Glow(Color.Lerp(color, Color.White, 0.3f), 3).SetIntensity(1.1f).SetFlicker(0.2f, 5f)
        };

        /// <summary>A golden title: a soft shadow, a warm pulsing glow, a dark brown outline and a shine that sweeps across it.</summary>
        public static List<ShadingEffect> GoldTitle() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 5), 6, Color.Black * 0.6f).SetSpread(3),
            new Glow(new Color(255, 160, 30), 22).SetIntensity(1.1f).SetPulse(0.6f, 0.35f),
            new Glow(new Color(255, 215, 110), 7).SetIntensity(1.4f),
            new Outline(new Color(90, 45, 0), 3),
            new Shine(new Color(255, 250, 220)).SetTiming(0.9f, 3f).SetWidth(34)
        };

        /// <summary>
        /// Burning: three glows from red to yellow, each flickering at its own pace, rising a little and breathing like
        /// flames, around a dark red outline.
        /// </summary>
        public static List<ShadingEffect> Fire() => new List<ShadingEffect>
        {
            new Glow(FireColors[0], 30).SetOffset(0, -6).SetIntensity(1.8f).SetFlicker(0.5f, 6f).SetBreathe(1.3f, 0.06f),
            new Glow(FireColors[1], 15).SetOffset(0, -3).SetIntensity(1.6f).SetFlicker(0.4f, 9f).SetTimeOffset(1.3f),
            new Glow(FireColors[2], 4).SetIntensity(1.6f).SetFlicker(0.3f, 13f).SetTimeOffset(2.1f),
            new Outline(new Color(110, 20, 0), 2)
        };

        /// <summary>Frozen: a cold blue shadow, two cyan glows that breathe slowly, a pale outline and a cold shine.</summary>
        public static List<ShadingEffect> Ice() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 3), 4, new Color(0, 30, 80) * 0.7f),
            new Glow(new Color(40, 140, 255), 26).SetIntensity(1f).SetBreathe(0.35f, 0.05f),
            new Glow(new Color(120, 220, 255), 8).SetIntensity(1.1f),
            new Outline(new Color(225, 250, 255), 2),
            new Shine(new Color(220, 245, 255)).SetTiming(1.2f, 4f).SetWidth(24).SetAngle(35)
        };

        /// <summary>Arcane: glows cycling through purple, pink and blue, one of them turning around the control, and a dark outline.</summary>
        public static List<ShadingEffect> Magic() => new List<ShadingEffect>
        {
            new Glow(MagicColors[0], 26).SetIntensity(1.5f).SetColorCycle(MagicColors, 4f).SetBreathe(0.5f, 0.05f),
            new Glow(MagicColors[1], 12).SetIntensity(1.6f).SetColorCycle(MagicColors, 4f).SetTimeOffset(1.3f).SetOrbit(5, 0.7f),
            new Outline(new Color(40, 0, 70), 2)
        };

        /// <summary>Party: three glows cycling through the rainbow, one step apart, around a thin dark outline.</summary>
        public static List<ShadingEffect> Rainbow() => new List<ShadingEffect>
        {
            new Glow(RainbowColors[0], 30).SetIntensity(1f).SetColorCycle(RainbowColors, 3f),
            new Glow(RainbowColors[2], 13).SetIntensity(1.2f).SetColorCycle(RainbowColors, 3f).SetTimeOffset(0.5f),
            new Glow(RainbowColors[4], 4).SetIntensity(1.1f).SetColorCycle(RainbowColors, 3f).SetTimeOffset(1f),
            new Outline(new Color(25, 10, 45), 1.5f)
        };

        /// <summary>Toxic: a black shadow, a radioactive green glow that throbs and a black outline.</summary>
        public static List<ShadingEffect> Toxic() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 4), 3, Color.Black * 0.8f).SetSpread(2),
            new Glow(new Color(90, 255, 40), 24).SetIntensity(1.4f).SetPulse(1.6f, 0.6f).SetBreathe(1.6f, 0.05f),
            new Glow(new Color(200, 255, 120), 5).SetIntensity(1.2f).SetFlicker(0.3f, 10f),
            new Outline(new Color(10, 30, 0), 2.5f)
        };

        /// <summary>Plasma: a cyan glow and a magenta glow turning around the control in opposite directions, with a pulsing white core and a thin dark outline.</summary>
        public static List<ShadingEffect> Plasma() => new List<ShadingEffect>
        {
            new Glow(new Color(40, 220, 255), 18).SetIntensity(1.2f).SetOrbit(6, 0.8f),
            new Glow(new Color(255, 40, 220), 18).SetIntensity(1.2f).SetOrbit(6, -0.8f),
            new Glow(Color.White, 3).SetIntensity(1f).SetPulse(2.4f, 0.4f),
            new Outline(new Color(25, 0, 45), 1.5f)
        };

        /// <summary>A heartbeat: a red glow that beats twice, grows with each beat and fades.</summary>
        public static List<ShadingEffect> Heartbeat() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 4), 6, new Color(60, 0, 10) * 0.7f),
            new Glow(new Color(255, 30, 70), 26).SetIntensity(1.6f).SetPulse(1.2f, 0.85f).SetBreathe(1.2f, 0.08f),
            new Glow(new Color(255, 120, 150), 7).SetIntensity(1.2f).SetPulse(2.4f, 0.5f),
            new Outline(new Color(90, 0, 20), 2)
        };

        /// <summary>A pale breathing light around the control.</summary>
        public static List<ShadingEffect> Ghost() => new List<ShadingEffect>
        {
            new Glow(new Color(90, 140, 255), 30).SetIntensity(0.9f).SetBreathe(0.4f, 0.08f).SetPulse(0.4f, 0.5f),
            new Glow(new Color(170, 210, 255), 9).SetIntensity(0.7f).SetFlicker(0.4f, 3f)
        };

        /// <summary>A hologram: a cyan glow, a thin outline and a blue light over the control that flickers, with a scan line.</summary>
        public static List<ShadingEffect> Hologram() => new List<ShadingEffect>
        {
            new Glow(new Color(60, 200, 255), 14).SetIntensity(1.4f).SetFlicker(0.35f, 11f),
            new Outline(new Color(120, 230, 255), 1.5f),
            new Glow(new Color(60, 160, 255), 6).SetLayer(ShadingLayer.Front).SetIntensity(0.6f).SetFlicker(0.7f, 14f),
            new Shine(new Color(150, 230, 255)).SetAngle(0).SetWidth(12).SetTiming(1.4f, 1.8f)
        };

        /// <summary>A selected item: a white border and a pulsing, breathing golden glow.</summary>
        public static List<ShadingEffect> Selected() => new List<ShadingEffect>
        {
            new Glow(new Color(255, 210, 60), 18).SetIntensity(2f).SetPulse(1.2f, 0.5f).SetBreathe(1.2f, 0.05f),
            new Outline(Color.White, 3)
        };

        /// <summary>A soft shadow and a white shine that sweeps across the control.</summary>
        public static List<ShadingEffect> Shiny() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 4), 6, Color.Black * 0.45f),
            new Shine(Color.White).SetTiming(0.7f, 2.2f).SetWidth(30).SetIntensity(0.9f)
        };

        /// <summary>A glitch: red and cyan copies that jitter around the control.</summary>
        public static List<ShadingEffect> Glitch() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(-3, 0), 0, new Color(255, 40, 80) * 0.9f).SetOrbit(2, 3.3f).SetFlicker(0.6f, 15f),
            new Shadow(new Vector2(3, 0), 0, new Color(40, 230, 255) * 0.9f).SetOrbit(2, -2.7f).SetFlicker(0.6f, 13f),
            new Glow(new Color(120, 200, 255), 10).SetIntensity(0.8f).SetFlicker(0.8f, 9f)
        };
    }
}
