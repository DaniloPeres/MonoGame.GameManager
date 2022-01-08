using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Interfaces
{
    public interface IContainer : IControl
    {
        IEnumerable<IControl> Children { get; }
        IControl AddChild(IControl child);
        void RemoveChild(IControl child);
        void ClearChildren();
        void SetNeedToShortChildren();
        void IterateChildren(Action<IControl> callback, bool recursive = true);
        IEnumerable<IControl> GetAllNestedControls();
        IEnumerable<IControl> Find(Func<IControl, bool> predicate, bool recursive = true);
        IEnumerable<T> FindByType<T>() where T : IControl;
    }
}
