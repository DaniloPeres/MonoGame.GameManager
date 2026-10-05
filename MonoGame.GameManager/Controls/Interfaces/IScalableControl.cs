using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Controls.Interfaces
{
    /// <summary>
    /// A control with its own scale. The scale is applied to the control and its children.
    /// </summary>
    public interface IScalableControl : IControl
    {
        Vector2 Scale { get; set; }
        Vector2 OriginWithoutScale { get; set; }

        /// <summary>The size of the control without any scale applied.</summary>
        Vector2 SizeWithoutScale { get; }

        IScalableControl SetScale(float scale);
        IScalableControl SetScale(Vector2 scale);
    }
}
