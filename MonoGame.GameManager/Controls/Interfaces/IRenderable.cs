using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Controls.Interfaces
{
    /// <summary>
    /// Drawing members of a control.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>The tint of the control.</summary>
        Color Color { get; set; }

        /// <summary>The opacity of the control and its children, from 0 (invisible) to 1 (opaque).</summary>
        float Opacity { get; set; }

        /// <summary>The opacity multiplied by the opacity of all its parents.</summary>
        float NestedOpacity { get; }

        /// <summary>When false, the control and its children are not drawn and do not receive input.</summary>
        bool IsVisible { get; set; }

        /// <summary>The drawing and input order among siblings (higher values are drawn on top).</summary>
        float ZIndex { get; }

        SpriteEffects SpriteEffects { get; set; }

        /// <summary>Called once per frame before <see cref="Draw"/> to update the layout.</summary>
        void OnBeforeDraw();

        void Draw(SpriteBatch spriteBatch);
    }
}
