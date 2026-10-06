using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// The parameters of a <see cref="SpriteBatch.Begin"/> call. Containers that need a different state (a camera
    /// transform, clipping...) push a new state on the <see cref="ControlManager"/> while drawing their children.
    /// </summary>
    public struct SpriteBatchState
    {
        public SpriteSortMode SortMode;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public DepthStencilState DepthStencilState;
        public RasterizerState RasterizerState;
        public Effect Effect;

        /// <summary>The transformation applied to everything drawn (null = identity).</summary>
        public Matrix? TransformMatrix;

        /// <summary>The clipping area in render target coordinates (null = no clipping).</summary>
        public Rectangle? ScissorRectangle;

        /// <summary>The default state: deferred, alpha blend, linear clamp, no transformation.</summary>
        public static SpriteBatchState Default => new SpriteBatchState
        {
            SortMode = SpriteSortMode.Deferred,
            BlendState = BlendState.AlphaBlend,
            SamplerState = SamplerState.LinearClamp,
            DepthStencilState = DepthStencilState.None,
            RasterizerState = RasterizerState.CullCounterClockwise
        };

        public SpriteBatchState WithTransform(Matrix? transformMatrix)
        {
            var copy = this;
            copy.TransformMatrix = transformMatrix;
            return copy;
        }

        public SpriteBatchState WithScissor(Rectangle? scissorRectangle)
        {
            var copy = this;
            copy.ScissorRectangle = scissorRectangle;
            return copy;
        }

        public SpriteBatchState WithSamplerState(SamplerState samplerState)
        {
            var copy = this;
            copy.SamplerState = samplerState;
            return copy;
        }

        public SpriteBatchState WithBlendState(BlendState blendState)
        {
            var copy = this;
            copy.BlendState = blendState;
            return copy;
        }

        public SpriteBatchState WithEffect(Effect effect)
        {
            var copy = this;
            copy.Effect = effect;
            return copy;
        }
    }
}
