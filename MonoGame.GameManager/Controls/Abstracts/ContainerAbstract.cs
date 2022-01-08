using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.Abstracts
{
    public abstract class ContainerAbstract<TControl> : Control<TControl>, IContainer where TControl : IControl
    {
        private readonly ConcurrentDictionary<int, IControl> children = new ConcurrentDictionary<int, IControl>();
        public IEnumerable<IControl> Children => GetSortedChildren();
        private List<IControl> sortedChildren;
        private bool needToSortChildren = true;

        public ContainerAbstract() { }

        public ContainerAbstract(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public ContainerAbstract(Vector2 position, Vector2 size)
        {
            SetPosition(position);
            Size = size;
        }

        public TControl SetSize(Vector2 size)
        {
            Size = size;
            return (TControl)(object)this;
        }

        IControl IContainer.AddChild(IControl child) => AddChild(child);
        public TControl AddChild(IControl child)
        {
            child.Parent = this;
            children.TryAdd(child.Id, child);
            SetNeedToShortChildren();
            return (TControl)(object)this;
        }

        public override void MarkAsDirty()
        {
            IterateChildren(child => child.MarkAsDirty(), false);
            base.MarkAsDirty();
        }

        public void RemoveChild(IControl child)
        {
            child.Parent = null;
            children.TryRemove(child.Id, out _);
            SetNeedToShortChildren();
        }

        public void ClearChildren()
            => children.Values.ToList().ForEach(RemoveChild);

        public void SetNeedToShortChildren()
            => needToSortChildren = true;

        public override void Draw(SpriteBatch spriteBatch)
            => DrawChildren(spriteBatch);

        public void IterateChildren(Action<IControl> callback, bool recursive = true)
        {
            var controls = Find(control => true, recursive).ToList();
            controls.ForEach(callback);
        }

        public IEnumerable<IControl> GetAllNestedControls()
        {
            return Find(control => true, true);
        }

        public IEnumerable<IControl> Find(Func<IControl, bool> predicate, bool recursive = true)
        {
            var childrenTemp = GetSortedChildren();
            var output = childrenTemp.Where(predicate).ToList();

            if (recursive)
            {
                var panelItems = FindByType<Panel>().ToList();
                panelItems.ForEach(panel =>
                {
                    output.AddRange(panel.Find(predicate, recursive));
                });
            }

            return output;
        }

        public IEnumerable<T> FindByType<T>() where T : IControl
        {
            var childrenTemp = GetSortedChildren();
            return childrenTemp
                .Where(control => control is T)
                .Cast<T>();
        }

        public override void OnBeforeDraw()
        {
            base.OnBeforeDraw();
            IterateChildren(x => x.OnBeforeDraw(), false);
        }

        public override void FireOnUpdateEvent(GameTime gameTime)
        {
            IterateChildren(child => child.FireOnUpdateEvent(gameTime), false);
            base.FireOnUpdateEvent(gameTime);
        }

        public override void CleanOnUpdateEvent()
        {
            IterateChildren(child => child.CleanOnUpdateEvent(), false);
            base.CleanOnUpdateEvent();
        }

        private void DrawChildren(SpriteBatch spriteBatch)
            => IterateChildren(child => child.Draw(spriteBatch), false);


        private List<IControl> GetSortedChildren()
        {
            if (needToSortChildren)
            {
                sortedChildren = children
                    .Values
                    .OrderBy(x => x.ZIndex)
                    .ThenBy(x => x.Id)
                    .ToList();

                needToSortChildren = false;
            }

            return sortedChildren;
        }
    }
}
