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
    /// A container that places its children one after the other, vertically or horizontally, in the order they
    /// were added. By default it takes the size of its content.
    /// </summary>
    /// <remarks>
    /// The stack panel sets the position and the anchor of its children: their own position is ignored. Hidden
    /// children take no space.
    /// </remarks>
    /// <example>
    /// <code>
    /// var menu = new StackPanel(Vector2.Zero)
    ///     .SetSpacing(10)
    ///     .SetChildAlignment(ChildAlignment.Center)
    ///     .SetAnchor(Anchor.Center)
    ///     .AddToScreen();
    /// menu.AddChild(new Button(Vector2.Zero, new Vector2(200, 50), Color.SteelBlue).SetText(font, "Play"));
    /// menu.AddChild(new Button(Vector2.Zero, new Vector2(200, 50), Color.SteelBlue).SetText(font, "Quit"));
    /// </code>
    /// </example>
    public class StackPanel : ContainerAbstract<StackPanel>
    {
        private readonly List<IControl> items = new List<IControl>();
        private readonly List<IControl> arrangedItems = new List<IControl>();
        private readonly List<Vector2> sizes = new List<Vector2>();
        private readonly List<Vector2> positions = new List<Vector2>();
        private Orientation orientation;
        private float spacing;
        private Thickness padding;
        private ChildAlignment childAlignment;
        private bool autoSize;

        /// <summary>Creates a stack panel that takes the size of its content.</summary>
        public StackPanel(Vector2 position, Orientation orientation = Orientation.Vertical)
            : base(position, Vector2.Zero)
        {
            this.orientation = orientation;
            autoSize = true;
        }

        /// <summary>Creates a stack panel with a fixed size (the children are aligned inside it).</summary>
        public StackPanel(Vector2 position, Vector2 size, Orientation orientation = Orientation.Vertical)
            : base(position, size)
        {
            this.orientation = orientation;
        }

        /// <summary>The children in the order they are stacked.</summary>
        public IReadOnlyList<IControl> Items => items;

        public Orientation Orientation
        {
            get => orientation;
            set
            {
                orientation = value;
                MarkSizeAsDirty();
            }
        }

        /// <summary>The space between two children.</summary>
        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                MarkSizeAsDirty();
            }
        }

        /// <summary>The space between the children and the edges of the panel.</summary>
        public Thickness Padding
        {
            get => padding;
            set
            {
                padding = value;
                MarkSizeAsDirty();
            }
        }

        /// <summary>How the children are aligned across the direction of the stack.</summary>
        public ChildAlignment ChildAlignment
        {
            get => childAlignment;
            set => childAlignment = value;
        }

        /// <summary>When true, the size of the panel follows its content.</summary>
        public bool AutoSize
        {
            get => autoSize;
            set
            {
                autoSize = value;
                MarkSizeAsDirty();
            }
        }

        public StackPanel SetOrientation(Orientation orientation)
        {
            Orientation = orientation;
            return this;
        }

        public StackPanel SetSpacing(float spacing)
        {
            Spacing = spacing;
            return this;
        }

        public StackPanel SetPadding(Thickness padding)
        {
            Padding = padding;
            return this;
        }

        public StackPanel SetPadding(float padding) => SetPadding(new Thickness(padding));

        public StackPanel SetChildAlignment(ChildAlignment childAlignment)
        {
            ChildAlignment = childAlignment;
            return this;
        }

        public StackPanel SetAutoSize(bool autoSize)
        {
            AutoSize = autoSize;
            return this;
        }

        public override StackPanel AddChild(IControl child)
        {
            var wasChild = ContainsChild(child);
            base.AddChild(child);
            if (!wasChild && ContainsChild(child))
                items.Add(child);
            MarkSizeAsDirty();
            return this;
        }

        /// <summary>Adds a child (or moves an existing one) at a position of the stack.</summary>
        public StackPanel InsertChild(int index, IControl child)
        {
            base.AddChild(child);
            items.Remove(child);
            items.Insert(MathHelper.Clamp(index, 0, items.Count), child);
            MarkSizeAsDirty();
            return this;
        }

        public override void RemoveChild(IControl child)
        {
            base.RemoveChild(child);
            if (!ContainsChild(child))
                items.Remove(child);
            MarkSizeAsDirty();
        }

        protected override void ArrangeChildren()
        {
            var scale = SafeNestedScale();
            Measure(scale, out var totalSize);

            for (var i = 0; i < arrangedItems.Count; i++)
            {
                var child = arrangedItems[i];
                if (child.Anchor != Anchor.TopLeft)
                    child.Anchor = Anchor.TopLeft;

                // The position of a child is where its origin is drawn.
                var position = positions[i] + child.Origin / scale;
                if (child.PositionAnchor != position)
                    child.PositionAnchor = position;
            }

            if (autoSize && SizeWithoutScale != totalSize)
                Size = totalSize;
        }

        protected override Vector2 CalculateSize()
        {
            if (!autoSize)
                return SizeWithoutScale;
            Measure(SafeNestedScale(), out var totalSize);
            return totalSize;
        }

        /// <summary>Calculates the positions of the visible children (in <see cref="positions"/>) and the size of the content.</summary>
        private void Measure(Vector2 scale, out Vector2 totalSize)
        {
            arrangedItems.Clear();
            sizes.Clear();
            foreach (var item in items)
            {
                if (!item.IsVisible || !ReferenceEquals(item.Parent, this))
                    continue;
                arrangedItems.Add(item);
                sizes.Add(item.Size / scale);
            }

            float? crossSize = null;
            if (!autoSize)
            {
                var size = SizeWithoutScale;
                crossSize = orientation == Orientation.Vertical ? size.X - padding.Horizontal : size.Y - padding.Vertical;
            }

            StackLayout.Arrange(sizes, orientation, spacing, padding, childAlignment, crossSize, positions, out totalSize);
        }

        private Vector2 SafeNestedScale()
        {
            CalculateNestedScaleIfDirty();
            var scale = NestedScale;
            return new Vector2(Math.Abs(scale.X) < 0.0001f ? 1f : scale.X, Math.Abs(scale.Y) < 0.0001f ? 1f : scale.Y);
        }
    }
}
