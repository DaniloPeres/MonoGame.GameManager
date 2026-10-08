using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A solid lip under the button that makes it look thick, like a 3D cartoon button (painted, not a light). By
    /// default it uses a darker version of the background color of the button (<see cref="ButtonEffect.ButtonColorShade"/>), and it gets thinner while the button is
    /// pressed.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new DepthEffect().SetDepth(6));
    /// button.AddEffect(new DepthEffect().SetDepth(5).SetColor(new Color(120, 60, 0)).UseButtonColor(null));
    /// </code>
    /// </example>
    public class DepthEffect : ButtonEffect<DepthEffect>
    {
        public DepthEffect()
        {
            Blend = ButtonEffectBlend.Normal;
            Color = new Color(40, 40, 40);
            ButtonColorShade = 0.45f;
            IgnoreStates();
        }

        /// <summary>The thickness of the lip.</summary>
        public float Depth { get; set; } = 6f;

        /// <summary>The thickness of the lip while the button is pressed, as a part of <see cref="Depth"/>.</summary>
        public float PressedDepthRate { get; set; } = 0.5f;

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Behind;

        public DepthEffect SetDepth(float depth)
        {
            Depth = depth;
            return this;
        }

        public DepthEffect SetPressedDepthRate(float pressedDepthRate)
        {
            PressedDepthRate = pressedDepthRate;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var depth = Math.Max(0f, Depth) * (context.State == ButtonVisualState.Pressed ? Math.Max(0f, PressedDepthRate) : 1f);
            if (depth <= 0f)
                return;

            var shape = context.Bounds.Offset(new Vector2(0f, depth));
            ButtonEffectResources.GetRoundedMask(context.CornerRadius, context.TextureDensity).Draw(spriteBatch, shape, GetTint(context));
        }
    }
}
