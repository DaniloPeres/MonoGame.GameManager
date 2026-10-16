using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A border made of several rings (levels) of different colors and thicknesses, around the button or inside its edge
    /// (painted, not a light). Dark, bright and light rings together make metallic and jewel frames: see
    /// <see cref="Gold"/>, <see cref="Silver"/>, <see cref="Bronze"/>, <see cref="Gem"/>, <see cref="Dark"/> and
    /// <see cref="Candy"/>, or build one with <see cref="AddLayer(Color, float)"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(FrameEffect.Gold());
    /// button.AddEffect(new FrameEffect()
    ///     .AddLayer(new Color(40, 20, 0), 1.5f)     // from the edge outwards
    ///     .AddLayer(new Color(240, 180, 60), 3)
    ///     .AddLayer(new Color(255, 240, 180), 1)
    ///     .AddLayer(new Color(40, 20, 0), 1.5f));
    /// button.AddEffect(FrameEffect.Gem(Color.DeepSkyBlue).SetPosition(OutlinePosition.Inside));
    /// </code>
    /// </example>
    public class FrameEffect : ButtonEffect<FrameEffect>
    {
        /// <summary>How much each ring covers the next one, so no seam shows between them.</summary>
        private const float Overlap = 0.75f;

        public FrameEffect() : this(OutlinePosition.Outside) { }

        public FrameEffect(OutlinePosition position)
        {
            Position = position;
            Blend = ButtonEffectBlend.Normal;
            IgnoreStates();
        }

        /// <summary>Around the button, or inside its edge.</summary>
        public OutlinePosition Position { get; set; }

        /// <summary>The rings, from the edge of the button outwards (or inwards for <see cref="OutlinePosition.Inside"/>).</summary>
        public IList<FrameLayer> Layers { get; set; } = new List<FrameLayer>();

        /// <summary>The space between the edge of the button and the first ring.</summary>
        public float Offset { get; set; }

        /// <summary>Multiplies the thickness of every ring.</summary>
        public float ThicknessScale { get; set; } = 1f;

        /// <summary>The thickness of the whole frame.</summary>
        public float TotalThickness
        {
            get
            {
                var total = 0f;
                if (Layers != null)
                {
                    for (var i = 0; i < Layers.Count; i++)
                        total += Math.Max(0f, Layers[i].Thickness);
                }

                return total * Math.Max(0f, ThicknessScale);
            }
        }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => Position == OutlinePosition.Outside ? ButtonEffectLayer.Behind : ButtonEffectLayer.Inside;

        /// <summary>A golden frame: dark edges, a gold band with a pale gold line of light.</summary>
        public static FrameEffect Gold() => new FrameEffect()
            .AddLayer(new Color(70, 35, 5), 1.2f)
            .AddLayer(new Color(235, 175, 55), 2.2f)
            .AddLayer(new Color(255, 238, 165), 1f)
            .AddLayer(new Color(195, 125, 30), 1.6f)
            .AddLayer(new Color(55, 28, 5), 1.2f);

        /// <summary>A silver frame: dark edges, a silver band with a white line of light.</summary>
        public static FrameEffect Silver() => new FrameEffect()
            .AddLayer(new Color(35, 40, 52), 1.2f)
            .AddLayer(new Color(185, 196, 212), 2.2f)
            .AddLayer(new Color(248, 251, 255), 1f)
            .AddLayer(new Color(135, 146, 165), 1.6f)
            .AddLayer(new Color(30, 34, 45), 1.2f);

        /// <summary>A bronze frame: dark edges, a copper band with a warm line of light.</summary>
        public static FrameEffect Bronze() => new FrameEffect()
            .AddLayer(new Color(50, 25, 10), 1.2f)
            .AddLayer(new Color(185, 105, 50), 2.2f)
            .AddLayer(new Color(245, 185, 125), 1f)
            .AddLayer(new Color(140, 70, 30), 1.6f)
            .AddLayer(new Color(40, 20, 8), 1.2f);

        /// <summary>A jewel frame of a color: dark edges, the color, and a light line of the same color.</summary>
        public static FrameEffect Gem(Color color)
            => new FrameEffect()
                .AddLayer(Color.Lerp(color, Color.Black, 0.7f), 1.2f)
                .AddLayer(color, 2.4f)
                .AddLayer(Color.Lerp(color, Color.White, 0.65f), 1f)
                .AddLayer(Color.Lerp(color, Color.Black, 0.3f), 1.4f)
                .AddLayer(Color.Lerp(color, Color.Black, 0.75f), 1.2f);

        /// <summary>A frame made of the color of the button: a light line, the color, and a dark edge.</summary>
        public static FrameEffect ButtonColor() => new FrameEffect()
            .AddLayer(FrameLayer.FromButtonColor(-0.55f, 1.2f))
            .AddLayer(FrameLayer.FromButtonColor(0.15f, 2.4f))
            .AddLayer(FrameLayer.FromButtonColor(0.65f, 1.6f));

        /// <summary>A simple dark frame with a thin light line.</summary>
        public static FrameEffect Dark() => new FrameEffect()
            .AddLayer(new Color(110, 110, 125), 1f)
            .AddLayer(new Color(22, 22, 28), 3f);

        /// <summary>A thick white candy border with a soft dark line around it.</summary>
        public static FrameEffect Candy() => new FrameEffect()
            .AddLayer(Color.White, 3.2f)
            .AddLayer(Color.Black * 0.22f, 1.5f);

        public FrameEffect SetPosition(OutlinePosition position)
        {
            Position = position;
            return this;
        }

        public FrameEffect SetOffset(float offset)
        {
            Offset = offset;
            return this;
        }

        public FrameEffect SetThicknessScale(float thicknessScale)
        {
            ThicknessScale = thicknessScale;
            return this;
        }

        /// <summary>Adds a ring after the others (further from the edge).</summary>
        public FrameEffect AddLayer(Color color, float thickness) => AddLayer(new FrameLayer(color, thickness));

        /// <summary>Adds a ring after the others (further from the edge).</summary>
        public FrameEffect AddLayer(FrameLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));
            Layers ??= new List<FrameLayer>();
            Layers.Add(layer);
            return this;
        }

        /// <summary>Removes every ring.</summary>
        public FrameEffect ClearLayers()
        {
            Layers?.Clear();
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var layers = Layers;
            if (layers == null || layers.Count == 0)
                return;

            var scale = Math.Max(0f, ThicknessScale);
            var outside = Position == OutlinePosition.Outside;
            var bounds = context.Bounds;
            var radius = context.CornerRadius;

            // From the farthest ring to the nearest one: each ring covers the edge of the previous one a little.
            var end = Math.Max(0f, Offset);
            for (var i = 0; i < layers.Count; i++)
                end += Math.Max(0f, layers[i].Thickness) * scale;

            for (var i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];
                var thickness = Math.Max(0f, layer.Thickness) * scale;
                var start = end - thickness;
                var reach = i < layers.Count - 1 ? end + Overlap : end;
                end = start;
                if (thickness <= 0f)
                    continue;

                var color = layer.ButtonColorShade.HasValue ? context.GetShadeOfBackground(layer.ButtonColorShade.Value, layer.Color) : layer.Color;
                var tint = ApplyIntensity(color, context);
                if (outside)
                {
                    var shape = bounds.Inflate(reach, reach);
                    ButtonEffectResources.GetRoundedBorder(Math.Max(0f, radius + reach), reach - start, context.TextureDensity).Draw(spriteBatch, shape, tint);
                }
                else
                {
                    var shape = bounds.Inflate(-start, -start);
                    if (shape.Width <= 0f || shape.Height <= 0f)
                        continue;
                    ButtonEffectResources.GetRoundedBorder(Math.Max(0f, radius - start), reach - start, context.TextureDensity).Draw(spriteBatch, shape, tint);
                }
            }
        }
    }
}
