using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Layout
{
    /// <summary>
    /// The geometry of a grid of cells of the same size (used by <see cref="Controls.GridPanel"/>).
    /// </summary>
    public static class GridLayout
    {
        /// <summary>The top-left position of a cell.</summary>
        public static Vector2 CellPosition(int column, int row, Vector2 cellSize, Vector2 spacing, Thickness padding)
            => new Vector2(padding.Left + column * (cellSize.X + spacing.X), padding.Top + row * (cellSize.Y + spacing.Y));

        /// <summary>The size of a grid, with the padding.</summary>
        public static Vector2 TotalSize(int columns, int rows, Vector2 cellSize, Vector2 spacing, Thickness padding)
            => new Vector2(
                padding.Horizontal + Math.Max(0, columns) * cellSize.X + Math.Max(0, columns - 1) * spacing.X,
                padding.Vertical + Math.Max(0, rows) * cellSize.Y + Math.Max(0, rows - 1) * spacing.Y);

        /// <summary>
        /// Finds the cell at a position. Returns false outside of the grid and between cells (in the spacing).
        /// </summary>
        public static bool TryGetCell(Vector2 position, int columns, int rows, Vector2 cellSize, Vector2 spacing, Thickness padding, out Point cell)
        {
            cell = Point.Zero;
            if (cellSize.X <= 0f || cellSize.Y <= 0f)
                return false;

            var x = position.X - padding.Left;
            var y = position.Y - padding.Top;
            if (x < 0f || y < 0f)
                return false;

            var stepX = cellSize.X + spacing.X;
            var stepY = cellSize.Y + spacing.Y;
            var column = (int)(x / stepX);
            var row = (int)(y / stepY);
            if (column >= columns || row >= rows || x - column * stepX >= cellSize.X || y - row * stepY >= cellSize.Y)
                return false;

            cell = new Point(column, row);
            return true;
        }
    }
}
