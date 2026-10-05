using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Layout
{
    /// <summary>
    /// Calculates the positions of items placed one after the other (used by <see cref="Controls.StackPanel"/>).
    /// </summary>
    public static class StackLayout
    {
        /// <summary>
        /// Calculates the top-left position of every item.
        /// </summary>
        /// <param name="sizes">The size of every item, in order.</param>
        /// <param name="orientation">The direction of the stack.</param>
        /// <param name="spacing">The space between two items.</param>
        /// <param name="padding">The space between the items and the edges.</param>
        /// <param name="alignment">How the items are aligned across the direction of the stack.</param>
        /// <param name="crossSize">
        /// The space available across the direction of the stack, without the padding (null = the largest item).
        /// </param>
        /// <param name="totalSize">The size of the stack, with the padding.</param>
        public static Vector2[] Arrange(IList<Vector2> sizes, Orientation orientation, float spacing, Thickness padding,
            ChildAlignment alignment, float? crossSize, out Vector2 totalSize)
        {
            if (sizes == null)
                throw new ArgumentNullException(nameof(sizes));

            var positions = new List<Vector2>(sizes.Count);
            Arrange(sizes, orientation, spacing, padding, alignment, crossSize, positions, out totalSize);
            return positions.ToArray();
        }

        /// <summary>
        /// Calculates the top-left position of every item into <paramref name="positions"/> (cleared first), without
        /// allocating memory when the list is reused.
        /// </summary>
        public static void Arrange(IList<Vector2> sizes, Orientation orientation, float spacing, Thickness padding,
            ChildAlignment alignment, float? crossSize, List<Vector2> positions, out Vector2 totalSize)
        {
            if (sizes == null)
                throw new ArgumentNullException(nameof(sizes));
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            var vertical = orientation == Orientation.Vertical;
            var largestCross = 0f;
            var length = 0f;
            for (var i = 0; i < sizes.Count; i++)
            {
                largestCross = Math.Max(largestCross, vertical ? sizes[i].X : sizes[i].Y);
                length += vertical ? sizes[i].Y : sizes[i].X;
            }
            if (sizes.Count > 1)
                length += spacing * (sizes.Count - 1);

            var cross = crossSize ?? largestCross;
            var crossStart = vertical ? padding.Left : padding.Top;
            positions.Clear();
            var cursor = vertical ? padding.Top : padding.Left;

            for (var i = 0; i < sizes.Count; i++)
            {
                var itemCross = vertical ? sizes[i].X : sizes[i].Y;
                var crossOffset = alignment == ChildAlignment.Center ? (cross - itemCross) / 2f
                    : alignment == ChildAlignment.End ? cross - itemCross
                    : 0f;

                positions.Add(vertical
                    ? new Vector2(crossStart + crossOffset, cursor)
                    : new Vector2(cursor, crossStart + crossOffset));
                cursor += (vertical ? sizes[i].Y : sizes[i].X) + spacing;
            }

            totalSize = vertical
                ? new Vector2(cross + padding.Horizontal, length + padding.Vertical)
                : new Vector2(length + padding.Horizontal, cross + padding.Vertical);
        }
    }
}
