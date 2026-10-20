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

        /// <summary>True when a batch begun with this state draws like one begun with <paramref name="other"/>.</summary>
        public readonly bool IsSameAs(SpriteBatchState other)
            => SortMode == other.SortMode
            && ReferenceEquals(BlendState, other.BlendState)
            && ReferenceEquals(SamplerState, other.SamplerState)
            && ReferenceEquals(DepthStencilState, other.DepthStencilState)
            && ReferenceEquals(RasterizerState, other.RasterizerState)
            && ReferenceEquals(Effect, other.Effect)
            && TransformMatrix == other.TransformMatrix
            && ScissorRectangle == other.ScissorRectangle;

        public readonly SpriteBatchState WithTransform(Matrix? transformMatrix)
        {
            var copy = this;
            copy.TransformMatrix = transformMatrix;
            return copy;
        }

        public readonly SpriteBatchState WithScissor(Rectangle? scissorRectangle)
        {
            var copy = this;
            copy.ScissorRectangle = scissorRectangle;
            return copy;
        }

        public readonly SpriteBatchState WithSamplerState(SamplerState samplerState)
        {
            var copy = this;
            copy.SamplerState = samplerState;
            return copy;
        }

        public readonly SpriteBatchState WithBlendState(BlendState blendState)
        {
            var copy = this;
            copy.BlendState = blendState;
            return copy;
        }

        public readonly SpriteBatchState WithEffect(Effect effect)
        {
            var copy = this;
            copy.Effect = effect;
            return copy;
        }
    }
}
