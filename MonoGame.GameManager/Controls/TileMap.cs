using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A grid of tiles drawn from a tileset texture (levels, maps, backgrounds). Only the visible tiles are drawn,
    /// and solid tiles can be used for collisions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tiles are indexes in the tileset, counted from the top-left, row by row; -1 is an empty cell. In the local
    /// space of the map one unit is one pixel of the tileset, so cell (c, r) starts at (c * TileWidth, r * TileHeight).
    /// </para>
    /// <para>
    /// Use a point sampler (<c>ScreenManagerSettings.PixelArt</c>) or a tileset with spacing between tiles to avoid
    /// lines between tiles when the map is scaled.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var map = new TileMap(tileset, new Point(16, 16), columns: 3, rows: 2)
    ///     .SetTiles(new[,] { { 0, 1, 2 }, { 3, -1, 3 } }) // [row, column], as written
    ///     .SetSolidTiles(3)
    ///     .AddToScreen(worldPanel);
    /// var blocked = map.CollidesWith(playerLocalBounds);
    /// </code>
    /// </example>
    public class TileMap : ScalableControlAbstract<TileMap>
    {
        private readonly HashSet<int> solidTiles = new HashSet<int>();
        private int[,] tiles;
        private Texture2D tileset;
        private Point tileSize;

        public TileMap(Texture2D tileset, Point tileSize, int columns, int rows)
        {
            if (tileSize.X <= 0 || tileSize.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(tileSize), "The tiles must have a positive size.");

            this.tileset = tileset;
            this.tileSize = tileSize;
            Resize(columns, rows);
        }

        public Texture2D Tileset
        {
            get => tileset;
            set => tileset = value;
        }

        public Point TileSize => tileSize;

        public int Columns => tiles.GetLength(1);

        public int Rows => tiles.GetLength(0);

        /// <summary>The space around the tiles of the tileset, in pixels.</summary>
        public int TilesetMargin { get; set; }

        /// <summary>The space between the tiles of the tileset, in pixels.</summary>
        public int TilesetSpacing { get; set; }

        /// <summary>
        /// Restricts the drawing to an area of the local space (null = automatic: the camera of a parent
        /// <see cref="CameraPanel"/>, or the screen).
        /// </summary>
        public RectangleF? CullArea { get; set; }

        /// <summary>Changes the number of cells; the existing tiles are kept, new cells are empty.</summary>
        public TileMap Resize(int columns, int rows)
        {
            if (columns <= 0 || rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns), "A map needs at least one cell.");

            var resized = new int[rows, columns];
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                    resized[row, column] = tiles != null && row < Rows && column < Columns ? tiles[row, column] : -1;
            }

            tiles = resized;
            Size = new Vector2(columns * tileSize.X, rows * tileSize.Y);
            return this;
        }

        /// <summary>Replaces every tile. The array is indexed [row, column], as it is written in code.</summary>
        public TileMap SetTiles(int[,] tilesByRow)
        {
            if (tilesByRow == null)
                throw new ArgumentNullException(nameof(tilesByRow));

            tiles = (int[,])tilesByRow.Clone();
            Size = new Vector2(Columns * tileSize.X, Rows * tileSize.Y);
            return this;
        }

        /// <summary>The tile of a cell (-1 when empty or outside of the map).</summary>
        public int GetTile(int column, int row) => IsInside(column, row) ? tiles[row, column] : -1;

        public TileMap SetTile(int column, int row, int tileIndex)
        {
            if (!IsInside(column, row))
                throw new ArgumentOutOfRangeException(nameof(column), $"The cell ({column}, {row}) is outside of the map.");
            tiles[row, column] = tileIndex;
            return this;
        }

        public bool IsInside(int column, int row) => column >= 0 && row >= 0 && column < Columns && row < Rows;

        /// <summary>Declares tiles as solid (see <see cref="IsSolid"/> and <see cref="CollidesWith"/>).</summary>
        public TileMap SetSolidTiles(params int[] tileIndexes)
        {
            solidTiles.Clear();
            if (tileIndexes != null)
            {
                foreach (var tileIndex in tileIndexes)
                    solidTiles.Add(tileIndex);
            }
            return this;
        }

        /// <summary>True if the cell contains a solid tile (cells outside of the map are not solid).</summary>
        public bool IsSolid(int column, int row) => solidTiles.Contains(GetTile(column, row));

        /// <summary>The cell at a position of the local space (it can be outside of the map).</summary>
        public Point LocalToCell(Vector2 localPosition)
            => new Point((int)Math.Floor(localPosition.X / tileSize.X), (int)Math.Floor(localPosition.Y / tileSize.Y));

        /// <summary>The top-left position of a cell in the local space.</summary>
        public Vector2 CellToLocal(int column, int row) => new Vector2(column * tileSize.X, row * tileSize.Y);

        /// <summary>The area of a cell in the local space.</summary>
        public RectangleF CellBounds(int column, int row) => new RectangleF(CellToLocal(column, row), tileSize.ToVector2());

        /// <summary>The cells of the map that touch an area of the local space.</summary>
        public IEnumerable<Point> CellsInArea(RectangleF localArea)
        {
            GetCellRange(localArea, out var first, out var last);
            for (var row = first.Y; row <= last.Y; row++)
            {
                for (var column = first.X; column <= last.X; column++)
                    yield return new Point(column, row);
            }
        }

        /// <summary>True if an area of the local space overlaps a solid tile.</summary>
        public bool CollidesWith(RectangleF localArea)
        {
            GetCellRange(localArea, out var first, out var last);
            for (var row = first.Y; row <= last.Y; row++)
            {
                for (var column = first.X; column <= last.X; column++)
                {
                    if (IsSolid(column, row) && CellBounds(column, row).Intersects(localArea))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// The areas of the solid tiles that overlap an area, to resolve collisions (see
        /// <see cref="Collision.RectanglesIntersect(RectangleF, RectangleF, out Vector2)"/>).
        /// </summary>
        public IEnumerable<RectangleF> GetSolidCellBounds(RectangleF localArea)
        {
            foreach (var cell in CellsInArea(localArea))
            {
                if (!IsSolid(cell.X, cell.Y))
                    continue;
                var bounds = CellBounds(cell.X, cell.Y);
                if (bounds.Intersects(localArea))
                    yield return bounds;
            }
        }

        /// <summary>The area of the tileset of a tile index.</summary>
        public Rectangle GetTileSourceRectangle(int tileIndex)
        {
            var tilesPerRow = tileset == null ? 1 : Math.Max(1, (tileset.Width - TilesetMargin * 2 + TilesetSpacing) / (tileSize.X + TilesetSpacing));
            var column = tileIndex % tilesPerRow;
            var row = tileIndex / tilesPerRow;
            return new Rectangle(
                TilesetMargin + column * (tileSize.X + TilesetSpacing),
                TilesetMargin + row * (tileSize.Y + TilesetSpacing),
                tileSize.X,
                tileSize.Y);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (tileset == null || tileset.IsDisposed)
                return;

            var visibleArea = GetVisibleArea();
            if (visibleArea.Width <= 0f || visibleArea.Height <= 0f)
                return;

            GetCellRange(visibleArea, out var first, out var last);
            var color = DrawColor;
            for (var row = first.Y; row <= last.Y; row++)
            {
                for (var column = first.X; column <= last.X; column++)
                {
                    var tile = tiles[row, column];
                    if (tile >= 0)
                        DrawLocalTexture(spriteBatch, tileset, GetTileSourceRectangle(tile), CellBounds(column, row), color);
                }
            }
        }

        private RectangleF GetVisibleArea()
        {
            var bounds = LocalBounds;
            if (CullArea.HasValue)
                return RectangleF.Intersect(bounds, CullArea.Value);
            if (Rotation != 0f)
                return bounds;

            if (Parent is CameraPanel cameraPanel)
            {
                // The world of a camera panel starts at the position of the panel: convert the visible world area
                // to the coordinates of the children, then to the local space of the map.
                var world = cameraPanel.Camera.GetVisibleArea();
                var offset = cameraPanel.DestinationRectangle.Location.ToVector2();
                var topLeft = ToLocalPosition(world.Position + offset);
                var bottomRight = ToLocalPosition(new Vector2(world.Right, world.Bottom) + offset);
                return RectangleF.Intersect(bounds, new RectangleF(topLeft, bottomRight - topLeft));
            }

            var screen = ServiceProvider.ScreenManager?.ScreenSize;
            if (!screen.HasValue || ServiceProvider.ControlManager?.CurrentState.TransformMatrix != null)
                return bounds;

            var screenTopLeft = ToLocalPosition(Vector2.Zero);
            var screenBottomRight = ToLocalPosition(screen.Value.ToVector2());
            return RectangleF.Intersect(bounds, new RectangleF(Vector2.Min(screenTopLeft, screenBottomRight), (screenBottomRight - screenTopLeft).Abs()));
        }

        private void GetCellRange(RectangleF localArea, out Point first, out Point last)
        {
            first = new Point(
                Math.Max(0, (int)Math.Floor(localArea.Left / tileSize.X)),
                Math.Max(0, (int)Math.Floor(localArea.Top / tileSize.Y)));
            last = new Point(
                Math.Min(Columns - 1, (int)Math.Ceiling(localArea.Right / tileSize.X) - 1),
                Math.Min(Rows - 1, (int)Math.Ceiling(localArea.Bottom / tileSize.Y) - 1));
        }
    }
}
