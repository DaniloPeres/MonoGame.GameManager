using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A button drawn with textures (one per state) or with solid colors, with an optional text. Other controls can
    /// be added to it (icons, labels...).
    /// </summary>
    /// <remarks>
    /// The texture or the color of the current <see cref="ButtonAbstract{TButton}.VisualState"/> is chosen when
    /// drawing. A button owns the presses it receives, so the controls below it are never clicked at the same time.
    /// </remarks>
    /// <example>
    /// <code>
    /// new Button(new Vector2(20, 20), new Vector2(200, 50), Color.SteelBlue)
    ///     .SetText(font, "Play")
    ///     .AddOnClick(args => StartGame())
    ///     .AddToScreen();
    /// </code>
    /// </example>
    public class Button : ButtonAbstract<Button>
    {
        /// <summary>Creates a button drawn with textures; its size is the size of the default texture.</summary>
        public Button(Texture2D defaultTexture, Vector2 position)
            : base(defaultTexture, position) { }

        /// <summary>Creates a button drawn with solid colors.</summary>
        public Button(Vector2 position, Vector2 size, Color backgroundColor)
            : base(position, size, backgroundColor) { }
    }
}
