using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.Abstracts
{
    /// <summary>
    /// Base class of the controls that contain other controls (Composite pattern).
    /// </summary>
    /// <remarks>
    /// Children are kept sorted by <see cref="IRenderable.ZIndex"/> (then by creation order). The sorted list is
    /// rebuilt only when it changes, and it can be modified safely while it is being iterated (for example when a
    /// click handler removes a control).
    /// </remarks>
    public abstract class ContainerAbstract<TControl> : ScalableControlAbstract<TControl>, IContainer where TControl : IScalableControl
    {
        private readonly Dictionary<int, IControl> children = new Dictionary<int, IControl>();
        private IControl[] sortedChildren = Array.Empty<IControl>();
        private bool needToSortChildren;
        private RenderTarget2D containerRenderTarget; // Used when the overflow is hidden with a render target
        private SpriteBatch containerSpriteBatch; // Used when the overflow is hidden with a render target
        private Action<IControl> onChildRemoved;
        private Action<IControl> onChildAdded;
        private bool hideOverflow;
        private bool? clipsInput;

        public ContainerAbstract() { }

        public ContainerAbstract(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public ContainerAbstract(Vector2 position, Vector2 size)
        {
            SetPosition(position);
            Size = size;
        }

        /// <summary>The children sorted by drawing order.</summary>
        public IEnumerable<IControl> Children => GetSortedChildren();

        /// <summary>The number of children.</summary>
        public int ChildrenCount => children.Count;

        /// <summary>When true, the children are clipped to the area of the container.</summary>
        public bool HideOverflow
        {
            get => hideOverflow;
            set
            {
                hideOverflow = value;
                MarkAsDirty();
            }
        }

        /// <summary>How the children are clipped when <see cref="HideOverflow"/> is enabled.</summary>
        public ContainerClipMode ClipMode { get; set; } = ContainerClipMode.Scissor;

        /// <summary>
        /// When true, the children only receive input inside the area of the container.
        /// By default it follows <see cref="HideOverflow"/>.
        /// </summary>
        public virtual bool ClipsInput
        {
            get => clipsInput ?? HideOverflow;
            set => clipsInput = value;
        }

        public TControl SetSize(Vector2 size)
        {
            Size = size;
            return ThisAsT;
        }

        IControl IContainer.AddChild(IControl child) => AddChild(child);

        /// <summary>
        /// Adds a child. A control that already has another parent is moved to this container.
        /// </summary>
        public virtual TControl AddChild(IControl child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (ReferenceEquals(child, this) || IsDescendantOf(child))
                throw new ArgumentException("A control cannot be added to itself or to one of its children.", nameof(child));

            if (ReferenceEquals(child.Parent, this) && ContainsChild(child))
                return ThisAsT;

            child.Parent?.RemoveChild(child);
            children[child.Id] = child;
            child.Parent = this;
            SetNeedToSortChildren();
            onChildAdded?.Invoke(child);
            return ThisAsT;
        }

        public bool ContainsChild(IControl child)
            => child != null && children.TryGetValue(child.Id, out var existing) && ReferenceEquals(existing, child);

        IControl IContainer.SetHideOverflow(bool hideOverflow) => SetHideOverflow(hideOverflow);
        public TControl SetHideOverflow(bool hideOverflow)
        {
            HideOverflow = hideOverflow;
            return ThisAsT;
        }

        public TControl SetClipMode(ContainerClipMode clipMode)
        {
            ClipMode = clipMode;
            return ThisAsT;
        }

        public TControl SetClipsInput(bool clipsInput)
        {
            ClipsInput = clipsInput;
            return ThisAsT;
        }

        public override void MarkAsDirty()
        {
            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
                items[i].MarkAsDirty();
            base.MarkAsDirty();
        }

        /// <summary>
        /// Removes a child. Nothing happens if the control is not a child of this container.
        /// </summary>
        public virtual void RemoveChild(IControl child)
        {
            if (!ContainsChild(child))
                return;

            children.Remove(child.Id);
            if (ReferenceEquals(child.Parent, this))
                child.Parent = null;
            SetNeedToSortChildren();
            onChildRemoved?.Invoke(child);
        }

        public virtual void ClearChildren()
        {
            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
                RemoveChild(items[i]);
        }

        public void SetNeedToSortChildren() => needToSortChildren = true;

        [Obsolete("Use SetNeedToSortChildren instead.")]
        public void SetNeedToShortChildren() => SetNeedToSortChildren();

        public override void OnBeforeDraw()
        {
            base.OnBeforeDraw();
            ArrangeChildren();

            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
            {
                var child = items[i];
                if (child.IsVisible && ReferenceEquals(child.Parent, this))
                    child.OnBeforeDraw();
            }

            if (HideOverflow && UsesRenderTargetClipping)
                RenderChildrenToTarget();
            else
                ReleaseRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (HideOverflow)
                DrawChildrenClipped(spriteBatch);
            else
                DrawChildren(spriteBatch);
        }

        /// <summary>
        /// Invokes <paramref name="callback"/> for every child (and the children of the children when
        /// <paramref name="recursive"/> is true). The children can be modified by the callback.
        /// </summary>
        public void IterateChildren(Action<IControl> callback, bool recursive = true)
        {
            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
            {
                var child = items[i];
                callback(child);
                if (recursive && child is IContainer container)
                    container.IterateChildren(callback, true);
            }
        }

        public IEnumerable<IControl> GetAllNestedControls() => Find(control => true, true);

        public IEnumerable<IControl> Find(Func<IControl, bool> predicate, bool recursive = true)
        {
            var output = new List<IControl>();
            IterateChildren(control =>
            {
                if (predicate(control))
                    output.Add(control);
            }, recursive);
            return output;
        }

        /// <summary>Returns the direct children of the given type.</summary>
        public IEnumerable<T> FindByType<T>() where T : IControl
            => GetSortedChildren().OfType<T>().ToList();

        /// <inheritdoc />
        public IControl FindByName(string name, bool recursive = true)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].Name == name)
                    return items[i];
            }

            if (!recursive)
                return null;

            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is IContainer container)
                {
                    var found = container.FindByName(name, true);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        /// <summary>Returns the first control with the given name and type, or null.</summary>
        public T FindByName<T>(string name, bool recursive = true) where T : class, IControl
            => FindByName(name, recursive) as T;

        /// <inheritdoc />
        public virtual Point TransformPointToLocal(Point point) => point;

        public override void FireOnUpdateEvent(GameTime gameTime)
        {
            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
            {
                var child = items[i];
                if (ReferenceEquals(child.Parent, this))
                    child.FireOnUpdateEvent(gameTime);
            }
            base.FireOnUpdateEvent(gameTime);
        }

        public override void CleanOnUpdateEvent()
        {
            IterateChildren(child => child.CleanOnUpdateEvent(), false);
            base.CleanOnUpdateEvent();
        }

        IControl IContainer.AddOnChildRemoved(Action<IControl> onChildRemoved) => AddOnChildRemoved(onChildRemoved);
        public TControl AddOnChildRemoved(Action<IControl> onChildRemoved)
        {
            this.onChildRemoved += onChildRemoved;
            return ThisAsT;
        }

        public TControl RemoveOnChildRemoved(Action<IControl> onChildRemoved)
        {
            this.onChildRemoved -= onChildRemoved;
            return ThisAsT;
        }

        public TControl AddOnChildAdded(Action<IControl> onChildAdded)
        {
            this.onChildAdded += onChildAdded;
            return ThisAsT;
        }

        public TControl RemoveOnChildAdded(Action<IControl> onChildAdded)
        {
            this.onChildAdded -= onChildAdded;
            return ThisAsT;
        }

        /// <summary>
        /// Called once per frame before the children are prepared for drawing. Layout containers override it to
        /// position their children.
        /// </summary>
        protected virtual void ArrangeChildren() { }

        /// <summary>Draws the visible children in their drawing order.</summary>
        protected void DrawChildren(SpriteBatch spriteBatch)
        {
            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
            {
                var child = items[i];
                if (child.IsVisible && ReferenceEquals(child.Parent, this))
                    child.Draw(spriteBatch);
            }
        }

        /// <summary>Draws the children clipped to the area of the container.</summary>
        protected void DrawChildrenClipped(SpriteBatch spriteBatch)
        {
            if (UsesRenderTargetClipping)
            {
                if (containerRenderTarget != null)
                    DrawTexture(spriteBatch, containerRenderTarget, DestinationRectangle, null, Origin);
                return;
            }

            var controlManager = ServiceProvider.ControlManager;
            if (controlManager == null)
            {
                DrawChildren(spriteBatch);
                return;
            }

            if (!controlManager.TryPushClip(spriteBatch, GetClipBounds()))
                return; // nothing of the container is visible

            DrawChildren(spriteBatch);
            controlManager.PopState(spriteBatch);
        }

        /// <summary>The area of the container on the screen, used for clipping.</summary>
        protected Rectangle GetClipBounds()
        {
            var bounds = DestinationRectangle;
            var origin = Origin;
            return new Rectangle(bounds.X - (int)origin.X, bounds.Y - (int)origin.Y, bounds.Width, bounds.Height);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                onChildRemoved = null;
                onChildAdded = null;

                var items = GetSortedChildren();
                for (var i = 0; i < items.Length; i++)
                    items[i].Dispose();

                ReleaseRenderTarget();
                containerSpriteBatch?.Dispose();
                containerSpriteBatch = null;
            }

            base.Dispose(disposing);
        }

        private bool UsesRenderTargetClipping => ClipMode == ContainerClipMode.RenderTarget || Rotation != 0f;

        private IControl[] GetSortedChildren()
        {
            if (needToSortChildren)
            {
                // A new array is created, so code iterating over the previous one is not affected.
                sortedChildren = children
                    .Values
                    .OrderBy(x => x.ZIndex)
                    .ThenBy(x => x.Id)
                    .ToArray();

                needToSortChildren = false;
            }

            return sortedChildren;
        }

        private bool IsDescendantOf(IControl control)
        {
            for (var current = Parent; current != null; current = current.Parent)
            {
                if (ReferenceEquals(current, control))
                    return true;
            }
            return false;
        }

        private void RenderChildrenToTarget()
        {
            var controlManager = ServiceProvider.ControlManager;
            var graphicsDevice = ServiceProvider.GraphicsDevice;
            var bounds = DestinationRectangle;
            if (controlManager == null || graphicsDevice == null || bounds.Width <= 0 || bounds.Height <= 0)
                return;

            if (containerRenderTarget == null || containerRenderTarget.IsDisposed
                || containerRenderTarget.Width != bounds.Width || containerRenderTarget.Height != bounds.Height)
            {
                containerRenderTarget?.Dispose();
                containerRenderTarget = new RenderTarget2D(graphicsDevice, bounds.Width, bounds.Height, false,
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }

            if (containerSpriteBatch == null)
                containerSpriteBatch = new SpriteBatch(graphicsDevice);

            var previousRenderTargets = graphicsDevice.GetRenderTargets();
            graphicsDevice.SetRenderTarget(containerRenderTarget);
            graphicsDevice.Clear(Color.Transparent);

            var offset = Matrix.CreateTranslation(-bounds.X, -bounds.Y, 0);
            controlManager.BeginOffscreen(containerSpriteBatch, controlManager.BaseState.WithTransform(offset).WithScissor(null));
            DrawChildren(containerSpriteBatch);
            controlManager.EndOffscreen(containerSpriteBatch);

            graphicsDevice.SetRenderTargets(previousRenderTargets);
        }

        private void ReleaseRenderTarget()
        {
            if (containerRenderTarget == null)
                return;
            containerRenderTarget.Dispose();
            containerRenderTarget = null;
        }
    }
}
