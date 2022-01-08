using Microsoft.Xna.Framework;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.GameMath
{
    public static class PositionCalculations
    {
        /// <summary>
        /// Calculate the horizontal and vertical center position on the screen give the object size
        /// </summary>
        /// <param name="size">The object size</param>
        /// <returns>The center position</returns>
        public static Vector2 CenterVerticalAndHorizontal(Vector2 size) => CenterVerticalAndHorizontal(size, ServiceProvider.ScreenManager.ScreenSize);
        public static Vector2 CenterVerticalAndHorizontal(Vector2 size, Point sizeBase)
            => CenterVerticalAndHorizontal(size, new Rectangle(Point.Zero, sizeBase));
        public static Vector2 CenterVerticalAndHorizontal(Vector2 size, Rectangle recBase)
            => new Vector2(CenterHorizontal(size.X, recBase.Width) + recBase.X, CenterVertical(size.Y, recBase.Height) + recBase.Y);

        /// <summary>
        /// Calculate the horizontal center position on the screen given the object size width
        /// </summary>
        /// <param name="sizeWidht">The object size width</param>
        /// <returns>The position X</returns>
        public static int CenterHorizontal(float sizeWidht) => CenterHorizontal(sizeWidht, ServiceProvider.ScreenManager.ScreenSize.X);

        public static int CenterHorizontal(float sizeWidht, float baseWidth)
            => (int)((baseWidth - sizeWidht) / 2f);

        /// <summary>
        /// Calculate the vertical center position on the screen given the object size height
        /// </summary>
        /// <param name="sizeHeight">The object size height</param>
        /// <returns>The position Y</returns>
        public static int CenterVertical(float sizeHeight) => CenterVertical(sizeHeight, ServiceProvider.ScreenManager.ScreenSize.Y);

        public static int CenterVertical(float sizeHeight, float baseHeight)
            => (int)((baseHeight - sizeHeight) / 2f);
    }
}
