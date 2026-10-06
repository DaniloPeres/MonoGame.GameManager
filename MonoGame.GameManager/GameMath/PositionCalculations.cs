using Microsoft.Xna.Framework;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Helpers to center elements. Prefer anchors (eg: <see cref="Enums.Anchor.Center"/>) for controls.
    /// </summary>
    public static class PositionCalculations
    {
        /// <summary>
        /// Calculate the horizontal and vertical center position on the screen give the object size.
        /// </summary>
        /// <param name="size">The object size</param>
        /// <returns>The center position</returns>
        public static Vector2 CenterVerticalAndHorizontal(Vector2 size) => CenterVerticalAndHorizontal(size, ScreenSize);

        public static Vector2 CenterVerticalAndHorizontal(Vector2 size, Point sizeBase)
            => CenterVerticalAndHorizontal(size, new Rectangle(Point.Zero, sizeBase));

        public static Vector2 CenterVerticalAndHorizontal(Vector2 size, Rectangle recBase)
            => new Vector2(CenterHorizontal(size.X, recBase.Width) + recBase.X, CenterVertical(size.Y, recBase.Height) + recBase.Y);

        /// <summary>
        /// Calculate the horizontal center position on the screen given the object size width.
        /// </summary>
        /// <param name="sizeWidth">The object size width</param>
        /// <returns>The position X</returns>
        public static int CenterHorizontal(float sizeWidth) => CenterHorizontal(sizeWidth, ScreenSize.X);

        public static int CenterHorizontal(float sizeWidth, float baseWidth)
            => (int)((baseWidth - sizeWidth) / 2f);

        /// <summary>
        /// Calculate the vertical center position on the screen given the object size height.
        /// </summary>
        /// <param name="sizeHeight">The object size height</param>
        /// <returns>The position Y</returns>
        public static int CenterVertical(float sizeHeight) => CenterVertical(sizeHeight, ScreenSize.Y);

        public static int CenterVertical(float sizeHeight, float baseHeight)
            => (int)((baseHeight - sizeHeight) / 2f);

        /// <summary>The position that centers an element of the given size in an area, without rounding.</summary>
        public static Vector2 Center(Vector2 size, RectangleF area)
            => new Vector2(area.X + (area.Width - size.X) / 2f, area.Y + (area.Height - size.Y) / 2f);

        private static Point ScreenSize => ServiceProvider.ScreenManager?.ScreenSize ?? Point.Zero;
    }
}
