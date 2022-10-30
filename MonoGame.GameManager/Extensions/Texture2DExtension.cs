using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Extensions
{
    public static class Texture2DExtension
    {
        public static Point Size(this Texture2D texture) => new Point(texture.Width, texture.Height);
        
        public static Color GetPixelColor(this Texture2D texture, int x, int y)
        {
            Color[] data = new Color[1];
            texture.GetData(0, new Rectangle(x, y, 1, 1), data, 0, 1);
            return data[0];
        }
    }
}
