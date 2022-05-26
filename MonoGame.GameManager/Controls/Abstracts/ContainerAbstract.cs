using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.Abstracts
{
    public abstract class ContainerAbstract<TControl> : ScalableControlAbstract<TControl>, IContainer where TControl : IScalableControl
    {
        private readonly ConcurrentDictionary<int, IControl> children = new ConcurrentDictionary<int, IControl>();
        public IEnumerable<IControl> Children => GetSortedChildren();
        private List<IControl> sortedChildren;
        private bool needToSortChildren = true;
        private RenderTarget2D containerRenderTarget; // Used when we hide the overflow
        private SpriteBatch containerSpriteBatch; // Used when we hide the overflow
        private Action<IControl> onChildRemoved;
        private bool hideOnverflow;
        public bool HideOverflow
        {
            get => hideOnverflow;
            set
            {
                hideOnverflow = value;
                MarkAsDirty();
            }
        }

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
        public virtual TControl AddChild(IControl child)
        {
            child.Parent = this;
            children.TryAdd(child.Id, child);
            SetNeedToShortChildren();
            return (TControl)(object)this;
        }

        public bool ContainsChild(IControl child) => children.ContainsKey(child.Id);

        IControl IContainer.SetHideOverflow(bool hideOverflow) => SetHideOverflow(hideOverflow);
        public TControl SetHideOverflow(bool hideOverflow)
        {
            HideOverflow = hideOverflow;
            return (TControl)(object)this;
        }

        public override void MarkAsDirty()
        {
            IterateChildren(child => child.MarkAsDirty(), false);
            base.MarkAsDirty();
        }

        public virtual void RemoveChild(IControl child)
        {
            child.Parent = null;
            children.TryRemove(child.Id, out _);
            onChildRemoved?.Invoke(child);
            SetNeedToShortChildren();
        }

        public virtual void ClearChildren()
            => children.Values.ToList().ForEach(RemoveChild);

        public void SetNeedToShortChildren()
            => needToSortChildren = true;

        public override void OnBeforeDraw()
        {
            base.OnBeforeDraw();
            IterateChildren(x => x.OnBeforeDraw(), false);

            if (HideOverflow)
                GenerateContainerImage();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (HideOverflow)
                DrawTexture(spriteBatch, containerRenderTarget, DestinationRectangle, DestinationRectangle, OriginWithoutScale);
            else
                DrawChildren(spriteBatch);
        }

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
                var panelItems = FindByType<IContainer>().ToList();
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

        private void GenerateContainerImage()
        {
            CreateSpriteBatchIfNull();

            var game = ServiceProvider.Game;
            if (containerRenderTarget == null)
            {
                containerRenderTarget = new RenderTarget2D(game.GraphicsDevice, (int)ServiceProvider.ScreenManager.ScreenSize.X, (int)ServiceProvider.ScreenManager.ScreenSize.Y);
                ServiceProvider.MemoryManager.AddAssetToDispose(containerRenderTarget);
            }

            game.GraphicsDevice.SetRenderTarget(containerRenderTarget);
            game.GraphicsDevice.Clear(Color.Transparent);
            containerSpriteBatch.Begin();

            DrawChildren(containerSpriteBatch);

            containerSpriteBatch.End();
            game.GraphicsDevice.SetRenderTarget(null);
        }

        private void CreateSpriteBatchIfNull()
        {
            if (containerSpriteBatch != null)
                return;

            containerSpriteBatch = new SpriteBatch(ServiceProvider.GraphicsDevice);
            ServiceProvider.MemoryManager.AddAssetToDispose(containerSpriteBatch);
        }

        IControl IContainer.AddOnChildRemoved(Action<IControl> onChildRemoved) => AddOnChildRemoved(onChildRemoved);
        public TControl AddOnChildRemoved(Action<IControl> onChildRemoved)
        {
            this.onChildRemoved += onChildRemoved;
            return (TControl)(object)this;
        }
    }
}
