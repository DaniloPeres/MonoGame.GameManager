using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Controls.Sprites
{
    public class SpriteAnimationFrame
    {
        public Texture2D Texture { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public Vector2 Margin { get; set; }
        public float Duration { get; set; } = 0.0166f;
    }
}
