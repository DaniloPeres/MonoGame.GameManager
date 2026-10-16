using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Lights the whole button with a color: a highlight on hover (<see cref="ButtonEffect{TEffect}.VisibleOnHover"/>),
    /// a slow pulse, a flash...
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new LightFillEffect().SetColor(Color.White).SetIntensity(0.2f).VisibleOnHover());
    /// </code>
    /// </example>
    public class LightFillEffect : ButtonEffect<LightFillEffect>
    {
        public LightFillEffect()
        {
            Intensity = 0.25f;
        }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Inside;

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
            => spriteBatch.FillRectangle(context.Bounds, GetTint(context));
    }
}
