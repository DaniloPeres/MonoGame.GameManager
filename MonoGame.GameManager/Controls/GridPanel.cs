using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Layout;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A container that places its children in a grid of cells of the same size (inventories, level selection,
    /// board games...). Its size follows the grid.
    /// </summary>
    /// <remarks>
    /// <see cref="SetChild"/> puts a control in a given cell; <see cref="ContainerAbstract{TControl}.AddChild"/>
    /// uses the first free cell (row by row). The grid sets the position and the anchor of its children.
    /// </remarks>
    /// <example>
    /// <code>
    /// var inventory = new GridPanel(new Vector2(20, 20), columns: 5, rows: 4, cellSize: new Vector2(64))
    ///     .SetSpacing(new Vector2(4))
    ///     .AddToScreen();
    /// inventory.SetChild(0, 0, new Image(swordTexture));
    /// var cell = inventory.PositionToCell(inventory.ToLocalPosition(args.Position));
    /// </code>
    /// </example>
    public class GridPanel : ContainerAbstract<GridPanel>
    {
        private readonly Dictionary<IControl, Point> cells = new Dictionary<IControl, Point>();
        private int columns;
        private int rows;
        private Vector2 cellSize;
        private Vector2 spacing;
        private Thickness padding;

        public GridPanel(Vector2 position, int columns, int rows, Vector2 cellSize)
            : base(position, Vector2.Zero)
        {
            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns), "A grid needs at least one column.");
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), "A grid needs at least one row.");

            this.columns = columns;
            this.rows = rows;
            this.cellSize = cellSize;
        }

        public int Columns
        {
            get => columns;
            set
            {
                columns = Math.Max(1, value);
                MarkSizeAsDirty();
            }
        }

        public int Rows
        {
            get => rows;
            set
            {
                rows = Math.Max(1, value);
                MarkSizeAsDirty();
            }
        }

        public Vector2 CellSize
        {
            get => cellSize;
            set
            {
                cellSize = value;
                MarkSizeAsDirty();
            }
        }

        /// <summary>The space between two cells.</summary>
        public Vector2 Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                MarkSizeAsDirty();
            }
        }

        /// <summary>The space between the cells and the edges of the grid.</summary>
        public Thickness Padding
        {
            get => padding;
            set
            {
                padding = value;
                MarkSizeAsDirty();
            }
        }

        /// <summary>How a child smaller than its cell is aligned (in both directions).</summary>
        public ChildAlignment CellAlignment { get; set; } = ChildAlignment.Center;

        public GridPanel SetSpacing(Vector2 spacing)
        {
            Spacing = spacing;
            return this;
        }

        public GridPanel SetPadding(Thickness padding)
        {
            Padding = padding;
            return this;
        }

        public GridPanel SetCellAlignment(ChildAlignment cellAlignment)
        {
            CellAlignment = cellAlignment;
            return this;
        }

        /// <summary>Adds a child in the first free cell.</summary>
        /// <exception cref="InvalidOperationException">Every cell is used.</exception>
        public override GridPanel AddChild(IControl child)
        {
            if (child != null && cells.ContainsKey(child) && ReferenceEquals(child.Parent, this))
                return this;
            if (!TryGetFreeCell(out var cell))
                throw new InvalidOperationException("The grid is full: use SetChild to replace a cell, or add rows.");
            return SetChild(cell.X, cell.Y, child);
        }

        /// <summary>Puts a control in a cell. A control already in that cell is removed from the grid.</summary>
        public GridPanel SetChild(int column, int row, IControl child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (column < 0 || column >= columns || row < 0 || row >= rows)
                throw new ArgumentOutOfRangeException(nameof(column), $"The cell ({column}, {row}) is outside of the grid.");

            var previous = GetChildAt(column, row);
            if (previous != null && !ReferenceEquals(previous, child))
                RemoveChild(previous);

            base.AddChild(child);
            cells[child] = new Point(column, row);
            return this;
        }

        public override void RemoveChild(IControl child)
        {
            base.RemoveChild(child);
            if (child != null && !ContainsChild(child))
                cells.Remove(child);
        }

        /// <summary>The control in a cell, or null.</summary>
        public IControl GetChildAt(int column, int row)
        {
            foreach (var pair in cells)
            {
                if (pair.Value.X == column && pair.Value.Y == row && ReferenceEquals(pair.Key.Parent, this))
                    return pair.Key;
            }
            return null;
        }

        /// <summary>The cell of a child, or null.</summary>
        public Point? GetCellOf(IControl child)
            => child != null && cells.TryGetValue(child, out var cell) ? cell : (Point?)null;

        /// <summary>The top-left position of a cell, in the local space of the grid.</summary>
        public Vector2 CellToPosition(int column, int row) => GridLayout.CellPosition(column, row, cellSize, spacing, padding);

        /// <summary>The cell at a position of the local space of the grid (see <see cref="ScalableControlAbstract{TControl}.ToLocalPosition(Vector2)"/>), or null.</summary>
        public Point? PositionToCell(Vector2 localPosition)
            => GridLayout.TryGetCell(localPosition, columns, rows, cellSize, spacing, padding, out var cell) ? cell : (Point?)null;

        protected override void ArrangeChildren()
        {
            var scale = NestedScale;
            var safeScale = new Vector2(Math.Abs(scale.X) < 0.0001f ? 1f : scale.X, Math.Abs(scale.Y) < 0.0001f ? 1f : scale.Y);

            foreach (var pair in cells)
            {
                var child = pair.Key;
                if (!ReferenceEquals(child.Parent, this))
                    continue;

                var childSize = child.Size / safeScale;
                var free = cellSize - childSize;
                var offset = CellAlignment == ChildAlignment.Center ? free / 2f
                    : CellAlignment == ChildAlignment.End ? free
                    : Vector2.Zero;

                if (child.Anchor != Anchor.TopLeft)
                    child.Anchor = Anchor.TopLeft;
                var position = CellToPosition(pair.Value.X, pair.Value.Y) + offset + child.Origin / safeScale;
                if (child.PositionAnchor != position)
                    child.PositionAnchor = position;
            }
        }

        protected override Vector2 CalculateSize() => GridLayout.TotalSize(columns, rows, cellSize, spacing, padding);

        private bool TryGetFreeCell(out Point freeCell)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    if (GetChildAt(column, row) == null)
                    {
                        freeCell = new Point(column, row);
                        return true;
                    }
                }
            }

            freeCell = Point.Zero;
            return false;
        }
    }
}
