using Microsoft.Xna.Framework;
using MonoGame.GameManager.Enums;

namespace MonoGame.GameManager.Controls.Interfaces
{
    /// <summary>
    /// Position, size and layout members of a control.
    /// </summary>
    public interface ILayoutElement
    {
        /// <summary>The point of the parent the position is relative to.</summary>
        Anchor Anchor { get; set; }

        /// <summary>The position relative to <see cref="Anchor"/>, in the unscaled units of the parent.</summary>
        Vector2 PositionAnchor { get; set; }

        /// <summary>The size of the control (scaled for scalable controls).</summary>
        Vector2 Size { get; set; }

        /// <summary>The pivot used for rotation and drawing (scaled for scalable controls).</summary>
        Vector2 Origin { get; set; }

        /// <summary>The rotation in radians.</summary>
        float Rotation { get; }

        /// <summary>The scale of the control multiplied by the scale of all its parents.</summary>
        Vector2 NestedScale { get; }

        /// <summary>The calculated area of the control on the screen.</summary>
        Rectangle DestinationRectangle { get; }

        /// <summary>Marks the layout of the control as outdated.</summary>
        void MarkAsDirty();

        /// <summary>Returns the position of the control on the screen.</summary>
        Vector2 GetPosition();

        void CalculateSizeIfIsDirty();

        Rectangle CalculateDestinationRectangle();

        Vector2 CalculateNestedScale();
    }
}
