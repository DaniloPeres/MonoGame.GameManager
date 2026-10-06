using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Services.Inputs;
using System;
using System.Threading;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Generates unique control identifiers.
    /// </summary>
    internal static class ControlIdGenerator
    {
        private static int lastId;

        public static int Next() => Interlocked.Increment(ref lastId);
    }

    /// <summary>
    /// Base class of every control. The type parameter is the concrete control type, so the fluent methods return
    /// the concrete type (eg: <c>new Label(...).SetColor(...).SetText(...)</c>).
    /// </summary>
    /// <remarks>
    /// The position of a control is relative to an <see cref="Anchor"/> of its parent. The size, scale and
    /// destination rectangle are calculated lazily and cached until something changes (dirty flags).
    /// </remarks>
    public abstract class Control<TControl> : IControl where TControl : IControl
    {
        /// <summary>All the controls are printed in the same LayerDepth; the order comes from the children list sorted by ZIndex.</summary>
        protected const float LayerDepthDraw = 0;

        private event CallbackMouseEvent onMouseEnter;
        private event CallbackMouseEvent onMouseLeave;
        private event CallbackMouseEvent onMousePressed;
        private event CallbackMouseEvent onMouseMoved;
        private event CallbackMouseEvent onMouseReleased;
        private event CallbackMouseEvent onClick;
        private event CallbackMouseEvent onMouseWheel;
        private event CallbackMultipleTouchpointsEvent onMultipleTouchpoints;
        private event Action onUpdateDestinationRectangle;
        private event Action<GameTime> onUpdateEvent;

        private Vector2 origin;
        private IContainer parent;
        private Anchor anchor = Anchor.TopLeft;
        private Vector2 positionAnchor;
        private Vector2 size;
        private Rectangle destinationRectangle;
        private float opacity = 1f;
        private bool isDirty;
        private bool blockSetIsDirty;

        /// <summary>Mark if the size of this control is dirty and need to recalculate.</summary>
        protected bool IsSizeDirty = true;
        protected bool IsNestedScaleDirty = true;

        public int Id { get; } = ControlIdGenerator.Next();

        public string Name { get; set; }

        public object Info { get; set; }

        public virtual Color Color { get; set; } = Color.White;

        /// <summary>The opacity of the control and its children, from 0 (invisible) to 1 (opaque).</summary>
        public float Opacity
        {
            get => opacity;
            set => opacity = MathHelper.Clamp(value, 0f, 1f);
        }

        /// <summary>The opacity multiplied by the opacity of all its parents.</summary>
        public float NestedOpacity => (Parent?.NestedOpacity ?? 1f) * opacity;

        /// <summary>When false, the control and its children are not drawn and do not receive input.</summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>When false, the control and its children do not receive input.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>When true, the controls below never receive pointer events that hit this control.</summary>
        public bool BlocksMouseEvents { get; set; }

        /// <summary>The mouse buttons this control reacts to (default: <see cref="MouseButtons.Left"/>).</summary>
        public MouseButtons AcceptedMouseButtons { get; set; } = MouseButtons.Left;

        public bool IsDisposed { get; private set; }

        public event Action<IControl> Disposed;

        /// <summary>
        /// Z-Index controls the vertical stacking order of elements that overlap.
        /// Used to:
        ///    - Order which control is printed first
        ///    - Order which control is going to have the first interaction (eg: OnClick, OnMousePressed...) [Descendant]
        /// </summary>
        public float ZIndex { get; private set; }

        public SpriteEffects SpriteEffects { get; set; } = SpriteEffects.None;

        /// <summary>Center of the rotation. 0,0 by default.</summary>
        public virtual Vector2 Origin
        {
            get => origin;
            set
            {
                origin = value;
                MarkAsDirty();
            }
        }

        /// <summary>The rotation in radians.</summary>
        public float Rotation { get; private set; }

        public IContainer Parent
        {
            get => parent;
            set
            {
                parent = value;
                MarkAsDirty();
            }
        }

        public Anchor Anchor
        {
            get => anchor;
            set
            {
                anchor = value;
                MarkAsDirty();
            }
        }

        /// <summary>
        /// The position based on the anchor.
        /// To get the position in the screen use the DestinationRectangle.
        /// </summary>
        public Vector2 PositionAnchor
        {
            get => positionAnchor;
            set
            {
                positionAnchor = value;
                MarkAsDirty();
            }
        }

        public virtual Vector2 Size
        {
            get
            {
                CalculateSizeIfIsDirty();
                return size;
            }
            set
            {
                size = value;
                MarkAsDirty();
                IsSizeDirty = false;
            }
        }

        /// <summary>
        /// The calculated scale of control with parent scale.
        /// eg: Control scale 0.5f and parent scale 0.5f, so, nested scale will be 0.25f
        /// </summary>
        public Vector2 NestedScale { get; private set; } = Vector2.One;

        /// <summary>
        /// The calculated destination rectangle of the control.
        /// </summary>
        public virtual Rectangle DestinationRectangle
        {
            get
            {
                UpdateDestinationRectsIfDirty();
                return destinationRectangle;
            }
            private set => destinationRectangle = value;
        }

        public bool IsMouseHover { get; private set; }

        public bool IsMousePressed { get; private set; }

        /// <summary>True for the root panel of the stage, which is positioned relative to the screen origin.</summary>
        internal bool IsStageRoot { get; set; }

        /// <summary>This instance typed as <typeparamref name="TControl"/>, for fluent methods.</summary>
        protected TControl ThisAsT => (TControl)(object)this;

        /// <summary>The <see cref="Color"/> multiplied by <see cref="NestedOpacity"/>, used for drawing.</summary>
        protected Color DrawColor => Color * NestedOpacity;

        /// <summary>
        /// The duration of the frame in seconds, multiplied by the global <see cref="Core.IClock.TimeScale"/>, for
        /// controls that animate themselves in an update event (slow motion and pause follow the game clock).
        /// </summary>
        protected static float GetScaledDeltaSeconds(GameTime gameTime)
            => (float)gameTime.ElapsedGameTime.TotalSeconds * ServiceProvider.Clock.TimeScale;

        /// <summary>
        /// Mark if this control is dirty and need to recalculate its destination rectangle.
        /// </summary>
        protected bool IsDirty
        {
            get => isDirty;
            set => isDirty = value && !blockSetIsDirty;
        }

        protected virtual void MarkSizeAsDirty()
        {
            IsSizeDirty = true;
        }

        public virtual void MarkAsDirty()
        {
            IsDirty = true;
            IsSizeDirty = true;
            IsNestedScaleDirty = true;
        }

        IControl IControl.SetInfo(object info) => SetInfo(info);
        public TControl SetInfo(object info)
        {
            Info = info;
            return ThisAsT;
        }

        IControl IControl.SetName(string name) => SetName(name);
        public TControl SetName(string name)
        {
            Name = name;
            return ThisAsT;
        }

        IControl IControl.SetZIndex(float zIndex) => SetZIndex(zIndex);
        public TControl SetZIndex(float zIndex)
        {
            ZIndex = zIndex;
            Parent?.SetNeedToSortChildren();
            return ThisAsT;
        }

        IControl IControl.SetRotationInDegree(float rotationInDegree) => SetRotationInDegree(rotationInDegree);
        public TControl SetRotationInDegree(float rotationInDegree) => SetRotation(MathHelper.ToRadians(rotationInDegree));

        IControl IControl.SetRotation(float rotation) => SetRotation(rotation);
        public TControl SetRotation(float rotation)
        {
            Rotation = rotation;
            return ThisAsT;
        }

        IControl IControl.SetOrigin(Vector2 origin) => SetOrigin(origin);
        public TControl SetOrigin(Vector2 origin)
        {
            Origin = origin;
            return ThisAsT;
        }

        IControl IControl.SetOriginRate(float originRate) => SetOriginRate(originRate);
        public TControl SetOriginRate(float originRate) => SetOriginRate(new Vector2(originRate));

        IControl IControl.SetOriginRate(Vector2 originRate) => SetOriginRate(originRate);
        public virtual TControl SetOriginRate(Vector2 originRate) => SetOriginRate(originRate, Size);

        IControl IControl.SetOriginRate(Vector2 originRate, Vector2 size) => SetOriginRate(originRate, size);
        public virtual TControl SetOriginRate(Vector2 originRate, Vector2 size) => SetOrigin(size * originRate);

        IControl IControl.SetColor(Color color) => SetColor(color);
        public TControl SetColor(Color color)
        {
            Color = color;
            return ThisAsT;
        }

        IControl IControl.SetOpacity(float opacity) => SetOpacity(opacity);
        public TControl SetOpacity(float opacity)
        {
            Opacity = opacity;
            return ThisAsT;
        }

        IControl IControl.SetIsVisible(bool isVisible) => SetIsVisible(isVisible);
        public TControl SetIsVisible(bool isVisible)
        {
            IsVisible = isVisible;
            return ThisAsT;
        }

        IControl IControl.SetIsEnabled(bool isEnabled) => SetIsEnabled(isEnabled);
        public TControl SetIsEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            return ThisAsT;
        }

        public TControl SetAcceptedMouseButtons(MouseButtons acceptedMouseButtons)
        {
            AcceptedMouseButtons = acceptedMouseButtons;
            return ThisAsT;
        }

        IControl IControl.SetAnchor(Anchor anchor) => SetAnchor(anchor);
        public TControl SetAnchor(Anchor anchor)
        {
            Anchor = anchor;
            return ThisAsT;
        }

        public virtual Vector2 GetPosition() => DestinationRectangle.Location.ToVector2();

        IControl IControl.SetPosition(float x, float y) => SetPosition(new Vector2(x, y));
        public virtual TControl SetPosition(float x, float y) => SetPosition(new Vector2(x, y));

        IControl IControl.SetPosition(Vector2 position) => SetPosition(position);
        public virtual TControl SetPosition(Vector2 position)
        {
            PositionAnchor = position;
            return ThisAsT;
        }

        IControl IControl.SetMouseEventsColor(Color hoverColor, Color pressedColor) => SetMouseEventsColor(hoverColor, pressedColor);
        /// <summary>
        /// Changes the color of the control while the pointer is over it or pressing it. The color at the moment
        /// this method is called is restored when the pointer leaves.
        /// </summary>
        public TControl SetMouseEventsColor(Color hoverColor, Color pressedColor)
        {
            var originalColor = Color;
            AddOnMouseEnter(args => Color = IsMousePressed ? pressedColor : hoverColor);
            AddOnMouseLeave(args => Color = originalColor);
            AddOnMousePressed(args => Color = pressedColor);
            AddOnMouseReleased(args => Color = hoverColor);
            return ThisAsT;
        }

        public virtual void OnBeforeDraw()
        {
            UpdateDestinationRectsIfDirty();
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Draws a texture with the color, opacity, rotation and sprite effects of the control.
        /// </summary>
        protected void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Vector2 origin)
        {
            if (texture == null || texture.IsDisposed)
                return;
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, DrawColor, Rotation, origin, SpriteEffects, LayerDepthDraw);
        }

        IControl IControl.AddOnMouseEnter(CallbackMouseEvent onMouseEnter) => AddOnMouseEnter(onMouseEnter);
        public TControl AddOnMouseEnter(CallbackMouseEvent onMouseEnter)
        {
            this.onMouseEnter += onMouseEnter;
            return ThisAsT;
        }

        IControl IControl.AddOnMouseLeave(CallbackMouseEvent onMouseLeave) => AddOnMouseLeave(onMouseLeave);
        public TControl AddOnMouseLeave(CallbackMouseEvent onMouseLeave)
        {
            this.onMouseLeave += onMouseLeave;
            return ThisAsT;
        }

        IControl IControl.AddOnMousePressed(CallbackMouseEvent onMousePressed) => AddOnMousePressed(onMousePressed);
        public TControl AddOnMousePressed(CallbackMouseEvent onMousePressed)
        {
            this.onMousePressed += onMousePressed;
            return ThisAsT;
        }

        IControl IControl.AddOnMouseMoved(CallbackMouseEvent onMouseMoved) => AddOnMouseMoved(onMouseMoved);
        public TControl AddOnMouseMoved(CallbackMouseEvent onMouseMoved)
        {
            this.onMouseMoved += onMouseMoved;
            return ThisAsT;
        }

        IControl IControl.AddOnMouseReleased(CallbackMouseEvent onMouseReleased) => AddOnMouseReleased(onMouseReleased);
        public TControl AddOnMouseReleased(CallbackMouseEvent onMouseReleased)
        {
            this.onMouseReleased += onMouseReleased;
            return ThisAsT;
        }

        IControl IControl.AddOnClick(CallbackMouseEvent onClick) => AddOnClick(onClick);
        /// <summary>
        /// Adds a click handler. A click is a press followed by a release on the same control. A control with a
        /// click handler receives the press, so the controls below it are not clicked too.
        /// </summary>
        public TControl AddOnClick(CallbackMouseEvent onClick)
        {
            this.onClick += onClick;
            return ThisAsT;
        }

        IControl IControl.AddOnMouseWheel(CallbackMouseEvent onMouseWheel) => AddOnMouseWheel(onMouseWheel);
        /// <summary>Adds a handler for the mouse wheel (<see cref="MouseEventArgs.ScrollWheelDelta"/>).</summary>
        public TControl AddOnMouseWheel(CallbackMouseEvent onMouseWheel)
        {
            this.onMouseWheel += onMouseWheel;
            return ThisAsT;
        }

        IControl IControl.AddOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints) => AddOnMultipleTouchpoints(onMultipleTouchpoints);
        public TControl AddOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints)
        {
            this.onMultipleTouchpoints += onMultipleTouchpoints;
            return ThisAsT;
        }

        IControl IControl.AddOnUpdateDestinationRectangle(Action onUpdateDestinationRectangle) => AddOnUpdateDestinationRectangle(onUpdateDestinationRectangle);
        /// <summary>Adds a callback invoked whenever the destination rectangle of the control is recalculated.</summary>
        public TControl AddOnUpdateDestinationRectangle(Action onUpdateDestinationRectangle)
        {
            this.onUpdateDestinationRectangle += onUpdateDestinationRectangle;
            return ThisAsT;
        }

        [Obsolete("Use AddOnUpdateDestinationRectangle instead.")]
        public TControl AddOnUpddateDestinationRectangle(Action onUpdateDestinationRectangle) => AddOnUpdateDestinationRectangle(onUpdateDestinationRectangle);

        IControl IControl.AddOnUpdateEvent(Action<GameTime> onUpdateEvent) => AddOnUpdateEvent(onUpdateEvent);
        /// <summary>
        /// Adds a callback invoked every frame while the control is on the screen. Adding the same callback twice
        /// has no effect.
        /// </summary>
        public TControl AddOnUpdateEvent(Action<GameTime> onUpdateEvent)
        {
            // Do not accept duplicated events
            this.onUpdateEvent -= onUpdateEvent;
            this.onUpdateEvent += onUpdateEvent;
            return ThisAsT;
        }

        public void RemoveOnMouseEnter(CallbackMouseEvent onMouseEnter) => this.onMouseEnter -= onMouseEnter;
        public void RemoveOnMouseLeave(CallbackMouseEvent onMouseLeave) => this.onMouseLeave -= onMouseLeave;
        public void RemoveOnMousePressed(CallbackMouseEvent onPressed) => this.onMousePressed -= onPressed;
        public void RemoveOnMouseMoved(CallbackMouseEvent onMouseMoved) => this.onMouseMoved -= onMouseMoved;
        public void RemoveOnMouseReleased(CallbackMouseEvent onMouseReleased) => this.onMouseReleased -= onMouseReleased;
        public void RemoveOnClick(CallbackMouseEvent onClick) => this.onClick -= onClick;
        public void RemoveOnMouseWheel(CallbackMouseEvent onMouseWheel) => this.onMouseWheel -= onMouseWheel;
        public void RemoveOnUpdateEvent(Action<GameTime> onUpdateEvent) => this.onUpdateEvent -= onUpdateEvent;
        public void RemoveOnUpdateDestinationRectangle(Action onUpdateDestinationRectangle) => this.onUpdateDestinationRectangle -= onUpdateDestinationRectangle;
        public void RemoveOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints) => this.onMultipleTouchpoints -= onMultipleTouchpoints;

        public virtual void CleanOnUpdateEvent() => onUpdateEvent = null;

        public void SetMouseHover(bool isMouseHover)
        {
            IsMouseHover = isMouseHover;
        }

        public void SetMousePressed(bool isMousePressed)
        {
            IsMousePressed = isMousePressed;
        }

        IControl IControl.BlockMouseEvents() => BlockMouseEvents();
        /// <summary>
        /// Block mouse events to not interact with elements below of it.
        /// </summary>
        /// <returns>The control</returns>
        public TControl BlockMouseEvents()
        {
            BlocksMouseEvents = true;
            return ThisAsT;
        }

        public TControl SetBlocksMouseEvents(bool blocksMouseEvents)
        {
            BlocksMouseEvents = blocksMouseEvents;
            return ThisAsT;
        }

        /// <inheritdoc />
        public bool HasMouseHandlers(MouseEventKinds kinds)
            => (kinds & MouseEventKinds.Enter) != 0 && onMouseEnter != null
            || (kinds & MouseEventKinds.Leave) != 0 && onMouseLeave != null
            || (kinds & MouseEventKinds.Pressed) != 0 && onMousePressed != null
            || (kinds & MouseEventKinds.Moved) != 0 && onMouseMoved != null
            || (kinds & MouseEventKinds.Released) != 0 && onMouseReleased != null
            || (kinds & MouseEventKinds.Click) != 0 && onClick != null
            || (kinds & MouseEventKinds.Wheel) != 0 && onMouseWheel != null
            || (kinds & MouseEventKinds.MultipleTouchpoints) != 0 && onMultipleTouchpoints != null;

        public void FireOnMouseEnter(ControlMouseEventArgs args) => FireMouseEvent(onMouseEnter, args);
        public void FireOnMouseLeave(ControlMouseEventArgs args) => FireMouseEvent(onMouseLeave, args);
        public void FireOnPressed(ControlMouseEventArgs args) => FireMouseEvent(onMousePressed, args);
        public void FireOnMoved(ControlMouseEventArgs args) => FireMouseEvent(onMouseMoved, args);
        public void FireOnReleased(ControlMouseEventArgs args) => FireMouseEvent(onMouseReleased, args);
        public void FireOnClick(ControlMouseEventArgs args) => FireMouseEvent(onClick, args);
        public void FireOnMouseWheel(ControlMouseEventArgs args) => FireMouseEvent(onMouseWheel, args);
        public void FireOnMultipleTouchpoints(ControlMultipleTouchpointsEventArgs args)
        {
            if (onMultipleTouchpoints == null)
            {
                args.ContinuePropagation();
                return;
            }

            onMultipleTouchpoints(args);
        }

        public virtual void FireOnUpdateEvent(GameTime gameTime) => onUpdateEvent?.Invoke(gameTime);

        /// <summary>
        /// Checks if the provided point position intersects the control. The rotation of the control is considered.
        /// </summary>
        /// <param name="pointToCompare">The point position (in the coordinate space of the parent), usually mouse input or touch input.</param>
        /// <returns>True if the provided point position intersects the control.</returns>
        public virtual bool Intersects(Point pointToCompare)
        {
            if (Rotation == 0f)
                return Collision.RectangleContainsPoint(DestinationRectangle, Origin, pointToCompare);

            // The control is rotated around its destination location: rotate the point back and test the unrotated area.
            var bounds = DestinationRectangle;
            var pivot = bounds.Location.ToVector2();
            var point = (pointToCompare.ToVector2() - pivot).Rotated(-Rotation) + pivot;
            var origin = Origin;
            var left = bounds.X - origin.X;
            var top = bounds.Y - origin.Y;
            return point.X >= left && point.X < left + bounds.Width && point.Y >= top && point.Y < top + bounds.Height;
        }

        IControl IControl.AddToScreen(IContainer parent) => AddToScreen(parent);
        /// <summary>
        /// Adds the control to <paramref name="parent"/>, or to the root panel of the current screen.
        /// </summary>
        public TControl AddToScreen(IContainer parent = null)
        {
            var addToParent = parent ?? ServiceProvider.RootPanel
                ?? throw new InvalidOperationException("There is no screen to add the control to. Pass a parent container, or add the control after the ScreenManager was initialized.");
            addToParent.AddChild(this);
            return ThisAsT;
        }

        public void CalculateSizeIfIsDirty()
        {
            if (IsSizeDirty)
            {
                IsSizeDirty = false;
                CalculateNestedScaleIfDirty();
                Size = CalculateSize();
            }
        }

        public virtual void RemoveFromScreen()
        {
            Parent?.RemoveChild(this);
        }

        /// <summary>
        /// Removes the control from the screen, clears all its event handlers and raises <see cref="Disposed"/>.
        /// Containers also dispose their children. Use <see cref="RemoveFromScreen"/> to remove a control that
        /// will be added again later.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void CalculateNestedScaleIfDirty()
        {
            if (IsNestedScaleDirty)
            {
                IsNestedScaleDirty = false;
                NestedScale = CalculateNestedScale();
            }
        }

        [Obsolete("Use CalculateNestedScaleIfDirty instead.")]
        public void CalculatedNestedScaleIfDirty() => CalculateNestedScaleIfDirty();

        public virtual Vector2 CalculateNestedScale() => Parent?.CalculateNestedScale() ?? Vector2.One;

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
                RemoveFromScreen();

            IsDisposed = true;

            var disposed = Disposed;
            Disposed = null;
            disposed?.Invoke(this);

            onMouseEnter = null;
            onMouseLeave = null;
            onMousePressed = null;
            onMouseMoved = null;
            onMouseReleased = null;
            onClick = null;
            onMouseWheel = null;
            onMultipleTouchpoints = null;
            onUpdateDestinationRectangle = null;
            onUpdateEvent = null;
        }

        /// <summary>
        /// Calculates the size of the control. Override it to compute the size from the content (texture, text...).
        /// </summary>
        protected virtual Vector2 CalculateSize() => size;

        /// <summary>
        /// Update destination rectangle.
        /// This is called internally whenever a change is made to the control or its parent.
        /// </summary>
        protected virtual void UpdateDestinationRects()
        {
            IsDirty = false;
            DestinationRectangle = CalculateDestinationRectangle();
            onUpdateDestinationRectangle?.Invoke();
        }

        /// <summary>
        /// Update destination rectangle, but only if needed (eg if something changed since last update).
        /// </summary>
        protected virtual void UpdateDestinationRectsIfDirty()
        {
            // if dirty, update destination rectangles
            if (IsDirty)
            {
                blockSetIsDirty = true;
                CalculateNestedScaleIfDirty();
                CalculateSizeIfIsDirty();
                UpdateDestinationRects();
                blockSetIsDirty = false;
            }
        }

        /// <summary>
        /// Calculate and return the destination rectangle, eg the space this control is rendered on screen.
        /// </summary>
        /// <remarks>
        /// Left and top anchors add <see cref="PositionAnchor"/> to the parent edge; right and bottom anchors
        /// subtract it (the offset points inwards); center anchors add it to the parent center.
        /// </remarks>
        /// <returns>Destination rectangle.</returns>
        public virtual Rectangle CalculateDestinationRectangle()
        {
            IContainer parentContainer = Parent;
            if (parentContainer == null && !IsStageRoot)
            {
                // A control that is not in the tree is positioned relative to the current screen.
                var root = ServiceProvider.RootPanel;
                if (root != null && !ReferenceEquals(root, this))
                    parentContainer = root;
            }

            var parentDestinationRectangle = parentContainer?.DestinationRectangle ?? Rectangle.Empty;
            var parentNestedScale = parentContainer?.NestedScale ?? Vector2.One;

            // Calculate some helpers
            var parentLeft = parentDestinationRectangle.X;
            var parentTop = parentDestinationRectangle.Y;

            var positionAnchor = PositionAnchor * parentNestedScale;

            // calculate position based on anchor and parent

            // calculate position X
            float posX;
            switch (Anchor)
            {
                // Center X
                case Anchor.TopCenter:
                case Anchor.Center:
                case Anchor.BottomCenter:
                    var parentCenterX = parentLeft + parentDestinationRectangle.Width / 2;
                    posX = parentCenterX - Size.X / 2 + positionAnchor.X + Origin.X;
                    break;

                // Right
                case Anchor.TopRight:
                case Anchor.CenterRight:
                case Anchor.BottomRight:
                    var parentRight = parentLeft + parentDestinationRectangle.Width;
                    posX = parentRight - Size.X - positionAnchor.X + Origin.X * 2;
                    break;

                // Left
                default:
                    posX = parentLeft + positionAnchor.X;
                    break;
            }

            // calculate position y
            float posY;
            switch (Anchor)
            {
                // Center Y
                case Anchor.CenterLeft:
                case Anchor.Center:
                case Anchor.CenterRight:
                    var parentCenterY = parentTop + parentDestinationRectangle.Height / 2;
                    posY = parentCenterY - Size.Y / 2 + positionAnchor.Y + Origin.Y;
                    break;

                // Bottom
                case Anchor.BottomLeft:
                case Anchor.BottomCenter:
                case Anchor.BottomRight:
                    var parentBottom = parentTop + parentDestinationRectangle.Height;
                    posY = parentBottom - Size.Y - positionAnchor.Y + Origin.Y * 2;
                    break;

                // Top
                default:
                    posY = parentTop + positionAnchor.Y;
                    break;
            }

            var position = new Vector2(posX, posY);
            return new Rectangle(position.ToPoint(), Size.ToPoint());
        }

        private void FireMouseEvent(CallbackMouseEvent mouseEvent, ControlMouseEventArgs args)
        {
            if (mouseEvent == null)
            {
                args.ContinuePropagation();
                return;
            }

            mouseEvent(args);
        }
    }
}
