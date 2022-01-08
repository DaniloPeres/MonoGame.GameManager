using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Extensions
{
    public static class Texture2DExtension
    {
        public static Point Size(this Texture2D texture) => new Point(texture.Width, texture.Height);
    }
}
