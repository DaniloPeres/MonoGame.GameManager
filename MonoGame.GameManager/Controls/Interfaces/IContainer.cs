using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Interfaces
{
    /// <summary>
    /// A control that contains other controls (Composite pattern).
    /// </summary>
    public interface IContainer : IScalableControl
    {
        /// <summary>When true, the children are clipped to the area of the container.</summary>
        bool HideOverflow { get; set; }

        /// <summary>When true, the children only receive input inside the area of the container.</summary>
        bool ClipsInput { get; }

        /// <summary>The children sorted by <see cref="IRenderable.ZIndex"/> (drawing order).</summary>
        IEnumerable<IControl> Children { get; }

        IControl AddChild(IControl child);
        bool ContainsChild(IControl child);
        void RemoveChild(IControl child);
        void ClearChildren();
        void SetNeedToSortChildren();
        void IterateChildren(Action<IControl> callback, bool recursive = true);
        IEnumerable<IControl> GetAllNestedControls();
        IEnumerable<IControl> Find(Func<IControl, bool> predicate, bool recursive = true);
        IEnumerable<T> FindByType<T>() where T : IControl;

        /// <summary>Returns the first control with the given <see cref="IControl.Name"/>, or null.</summary>
        IControl FindByName(string name, bool recursive = true);

        /// <summary>
        /// Converts a point from the coordinate space of this container's parent to the coordinate space of its
        /// children. It is the identity for regular containers (see <see cref="CameraPanel"/>).
        /// </summary>
        Point TransformPointToLocal(Point point);

        IControl SetHideOverflow(bool hideOverflow);
        IControl AddOnChildRemoved(Action<IControl> onChildRemoved);
    }
}
