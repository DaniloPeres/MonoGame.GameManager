using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Controls.Shading;
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
        private RenderTarget2D cacheTarget; // Used by the render cache (RenderCacheRate)
        private long cacheTicks;
        private Point cacheSize;
        private Point cacheOffset;
        private bool cacheInUse;
        private long cacheLastFrameTicks;
        private double cacheFrameSeconds = 1.0; // the mean time between two frames (smoothed)

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

            if (UsesRenderCache)
                UpdateRenderCache();
            else
                ReleaseRenderCache();

            // After the children: the silhouette of the container includes them.
            PrepareShading();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (UsesRenderCache && cacheInUse && cacheTarget != null && !cacheTarget.IsDisposed && !IsCapturingShading())
            {
                // The children as they were last rendered, at the place of the container now (it may have moved).
                var bounds = GetClipBounds();
                spriteBatch.Draw(cacheTarget, new Vector2(bounds.X + cacheOffset.X, bounds.Y + cacheOffset.Y),
                    new Rectangle(0, 0, cacheSize.X, cacheSize.Y), Color.White, 0f,
                    Vector2.Zero, 1f, SpriteEffects.None, LayerDepthDraw);
                return;
            }

            if (HideOverflow)
                DrawChildrenClipped(spriteBatch);
            else
                DrawChildren(spriteBatch);
        }

        /// <summary>
        /// When above 0 and the game draws more than twice this many frames per second (eg: an unlocked frame rate on a
        /// computer), the children are drawn into a render target at most this many times per second and the container draws
        /// that texture in between: one draw call for many controls whose animations do not need every frame (60 shows no
        /// difference). Slower, the children are drawn as usual (no extra render target switches on a phone at 60 FPS).
        /// Only for a container that is not rotated and does not hide its overflow; what the children draw outside the
        /// container must stay within <see cref="RenderCacheMargin"/>. Their input is not affected.
        /// </summary>
        public float RenderCacheRate { get; set; }

        /// <summary>How far, in local units, the drawing of the children may reach outside the container (glows, particles).</summary>
        public float RenderCacheMargin { get; set; }

        private bool UsesRenderCache => RenderCacheRate > 0f && !HideOverflow && Rotation == 0f;

        /// <summary>True while the silhouette of a control is captured for its shading (the children are drawn as they are).</summary>
        private static bool IsCapturingShading() => ShadingRenderer.IsCapturing;

        /// <summary>Draws the children into the cache when it is due (or missing, or its size changed).</summary>
        private void UpdateRenderCache()
        {
            var controlManager = ServiceProvider.ControlManager;
            var graphicsDevice = ServiceProvider.GraphicsDevice;
            var bounds = GetClipBounds();
            var scale = NestedScale;
            var margin = (int)Math.Ceiling(RenderCacheMargin * Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y)));
            var size = new Point(bounds.Width + 2 * margin, bounds.Height + 2 * margin);
            if (controlManager == null || graphicsDevice == null || bounds.Width <= 0 || bounds.Height <= 0 || size.X > 4096 || size.Y > 4096)
            {
                ReleaseRenderCache();
                return;
            }

            var now = System.Diagnostics.Stopwatch.GetTimestamp();
            if (cacheLastFrameTicks != 0)
            {
                var frame = (now - cacheLastFrameTicks) / (double)System.Diagnostics.Stopwatch.Frequency;
                cacheFrameSeconds += (Math.Min(frame, 1.0) - cacheFrameSeconds) * 0.1;
            }

            cacheLastFrameTicks = now;
            cacheInUse = cacheFrameSeconds * RenderCacheRate < 0.5;
            // Out of sight in a scroll viewer, or drawn as usual at this frame rate: nothing to render.
            if (!cacheInUse || Effects.EffectCulling.IsOutsideClippingParents(this, GetPosition(), SizeWithoutScale, OriginWithoutScale,
                    NestedScale, Rotation, RenderCacheMargin))
                return;

            // A size that changes (a pop-in) is rendered at the same pace: in between, the last picture is drawn.
            var valid = cacheTarget != null && !cacheTarget.IsDisposed && !cacheTarget.IsContentLost
                && cacheTarget.Width >= size.X && cacheTarget.Height >= size.Y;
            if (valid && (now - cacheTicks) * RenderCacheRate < System.Diagnostics.Stopwatch.Frequency)
                return;

            // Rounded up, so a container that grows or shrinks (a popup that pops in) keeps its target.
            if (cacheTarget == null || cacheTarget.IsDisposed || cacheTarget.Width < size.X || cacheTarget.Height < size.Y
                || cacheTarget.Width > size.X + 256 || cacheTarget.Height > size.Y + 256)
            {
                cacheTarget?.Dispose();
                cacheTarget = new RenderTarget2D(graphicsDevice, (size.X + 63) / 64 * 64, (size.Y + 63) / 64 * 64, false, SurfaceFormat.Color,
                    DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }

            if (containerSpriteBatch == null)
                containerSpriteBatch = new SpriteBatch(graphicsDevice);

            var previousRenderTargets = graphicsDevice.GetRenderTargets();
            graphicsDevice.SetRenderTarget(cacheTarget);
            graphicsDevice.Clear(Color.Transparent);
            var offset = Matrix.CreateTranslation(margin - bounds.X, margin - bounds.Y, 0);
            controlManager.BeginOffscreen(containerSpriteBatch, controlManager.BaseState.WithTransform(offset).WithScissor(null));
            DrawChildren(containerSpriteBatch);
            controlManager.EndOffscreen(containerSpriteBatch);
            graphicsDevice.SetRenderTargets(previousRenderTargets);

            cacheTicks = now;
            cacheSize = size;
            cacheOffset = new Point(-margin, -margin);
        }

        private void ReleaseRenderCache()
        {
            if (cacheTarget == null)
                return;
            cacheTarget.Dispose();
            cacheTarget = null;
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

        /// <summary>
        /// What the container and its children draw (their signatures, places, sizes, colors and opacities), so its
        /// shading effects are rendered again only when it changes; null when the container draws more than its own look
        /// (<see cref="GetOwnContentSignature"/>) and its children, or when a child cannot tell.
        /// </summary>
        protected override int? GetContentSignature()
        {
            var own = GetOwnContentSignature();
            if (!own.HasValue)
                return null;

            var hash = new HashCode();
            hash.Add(own.Value);
            hash.Add(HideOverflow);
            var items = GetSortedChildren();
            for (var i = 0; i < items.Length; i++)
            {
                var child = items[i];
                if (!ReferenceEquals(child.Parent, this))
                    continue;
                hash.Add(child.IsVisible);
                if (!child.IsVisible)
                    continue;
                if (!(child is IShadingHost shadingHost))
                    return null;
                var signature = shadingHost.GetShadingContentSignature();
                if (!signature.HasValue)
                    return null;

                hash.Add(signature.Value);
                hash.Add(child.PositionAnchor);
                hash.Add(child.Anchor);
                hash.Add(child.Origin);
                hash.Add(child.Size);
                hash.Add(child.Rotation);
                hash.Add(child.Color.PackedValue);
                hash.Add(child.Opacity);
                hash.Add(child.SpriteEffects);
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// A value that changes when what the container draws itself (not its children) changes, or null when it is unknown
        /// (the default: its shading effects are rendered again every frame). <see cref="Panel"/> and the buttons know theirs.
        /// </summary>
        protected virtual int? GetOwnContentSignature() => null;

        private static readonly Dictionary<Type, Type> drawDeclaringTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// The class that declares the <c>Draw</c> method of this control: a derived control that draws more than its base
        /// class has no known signature.
        /// </summary>
        protected Type DrawDeclaringType
        {
            get
            {
                var type = GetType();
                if (!drawDeclaringTypes.TryGetValue(type, out var declaring))
                {
                    declaring = type.GetMethod(nameof(Draw), new[] { typeof(SpriteBatch) })?.DeclaringType;
                    if (declaring != null && declaring.IsGenericType)
                        declaring = declaring.GetGenericTypeDefinition();
                    drawDeclaringTypes[type] = declaring;
                }

                return declaring;
            }
        }

        /// <summary>Draws the visible children in their drawing order.</summary>
        protected void DrawChildren(SpriteBatch spriteBatch)
        {
            var items = GetSortedChildren();
            // The content panel of a scroll viewer is as big as the viewport while its children reach far beyond it: never culled.
            var cullArea = Rectangle.Empty;
            var hasCullArea = !(this is ScrollViewer) && TryGetCullArea(out cullArea);
            for (var i = 0; i < items.Length; i++)
            {
                var child = items[i];
                if (child.IsVisible && ReferenceEquals(child.Parent, this) && !(hasCullArea && IsOutside(child, cullArea)))
                    ShadingRenderer.DrawControl(child, spriteBatch);
            }
        }

        /// <summary>
        /// How far, in local units, the drawing of a child of a scroll viewer may reach outside its area (glows, shadows,
        /// sparkles): a child further than this outside the visible part of the scroll viewer is not drawn.
        /// </summary>
        public static float ScrolledChildCullMargin { get; set; } = 48f;

        /// <summary>
        /// The clipping area children are culled against: only inside a <see cref="ScrollViewer"/> (where most of the
        /// content is out of sight), while a clipping rectangle is set and no transformation is applied.
        /// </summary>
        private bool TryGetCullArea(out Rectangle area)
        {
            area = Rectangle.Empty;
            var controlManager = ServiceProvider.ControlManager;
            if (controlManager == null)
                return false;
            ref readonly var state = ref controlManager.CurrentState;
            if (!state.ScissorRectangle.HasValue || state.TransformMatrix.HasValue)
                return false;

            var insideScrollViewer = false;
            for (IControl control = this; control != null; control = control.Parent)
            {
                if (control is CameraPanel)
                    return false;
                if (control is ScrollViewer)
                    insideScrollViewer = true;
            }

            area = state.ScissorRectangle.Value;
            return insideScrollViewer;
        }

        /// <summary>True when <paramref name="child"/>, with <see cref="ScrolledChildCullMargin"/> around it, is outside <paramref name="area"/>.</summary>
        private static bool IsOutside(IControl child, Rectangle area)
        {
            var size = child.Size;
            // A control without a size (eg: a particle emitter) may draw anywhere.
            if (size.X <= 0f || size.Y <= 0f)
                return false;

            var bounds = child.DestinationRectangle;
            var origin = child.Origin;
            var scale = child.NestedScale;
            var margin = ScrolledChildCullMargin * Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y));
            if (child.Rotation != 0f)
                margin += size.Length() / 2f;
            var left = bounds.X - origin.X - margin;
            var top = bounds.Y - origin.Y - margin;
            return left > area.Right || top > area.Bottom
                || left + bounds.Width + 2f * margin < area.Left || top + bounds.Height + 2f * margin < area.Top;
        }

        /// <summary>Draws the children clipped to the area of the container.</summary>
        protected void DrawChildrenClipped(SpriteBatch spriteBatch)
        {
            if (UsesRenderTargetClipping)
            {
                // The children were drawn with their own opacity: the result is drawn without tint.
                if (containerRenderTarget != null && !containerRenderTarget.IsDisposed)
                    spriteBatch.Draw(containerRenderTarget, DestinationRectangle, null, Color.White, Rotation, Origin, SpriteEffects.None, LayerDepthDraw);
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

                ReleaseRenderTargetAndCache();
                containerSpriteBatch?.Dispose();
                containerSpriteBatch = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>True when the overflow is hidden with a render target instead of the scissor rectangle.</summary>
        protected virtual bool UsesRenderTargetClipping => ClipMode == ContainerClipMode.RenderTarget || Rotation != 0f;

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
            var bounds = GetClipBounds(); // the visual area: the render target is drawn back with the origin of the container
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

        private void ReleaseRenderTargetAndCache()
        {
            ReleaseRenderTarget();
            ReleaseRenderCache();
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
