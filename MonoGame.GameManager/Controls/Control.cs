using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Controls
{
    public abstract class Control<TControl> : IControl where TControl : IControl
    {
        public int Id { get; } = ServiceProvider.ControlCounterService.GenerateControlId();

        public object Info { get; set; }

        public virtual Color Color { get; set; } = Color.White;

        /// <summary>
        /// Z-Index controls the vertical stacking order of elements that overlap.
        /// Used to:
        ///    - Order which control is printed first
        ///    - Order which control is going to have the first interaction (eg: OnClick, OnMousePressed...) [Descendant]
        /// </summary>
        public float ZIndex { get; private set; }

        public SpriteEffects SpriteEffects { get; set; } = SpriteEffects.None;

        /// <summary>All the controls are printed in the same LayerDepth, we order by sorting the children list by ZIndex</summary>
        protected const float LayerDepthDraw = 0;

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
        private Vector2 origin;

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
        private IContainer parent;

        public Anchor Anchor
        {
            get => anchor;
            set
            {
                anchor = value;
                MarkAsDirty();
            }
        }
        private Anchor anchor = Anchor.TopLeft;


        /// <summary>
        /// The position based on the anchor
        /// To get the position in the screen use the DestinationRectangle
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
        private Vector2 positionAnchor;

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
        private Vector2 size;

        /// <summary>
        /// The calculated scale of control with parent scale
        /// eg: Control scale 0.5f and parent scale 0.5f, so, nested scale will be 0.25f
        /// </summary>
        public Vector2 NestedScale { get; private set; } = Vector2.One;

        private Rectangle destinationRectangle;

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

        /// <summary>
        /// Mark if the size of this control is dirty and need to recalculate.
        /// </summary>
        protected bool IsSizeDirty = true;
        protected bool IsNestedScaleDirty = true;

        private TControl ThisAsT => (TControl)(object)this;


        protected virtual void MarkSizeAsDirty()
        {
            IsSizeDirty = true;
        }

        /// <summary>
        /// Mark if this control is dirty and need to recalculate its destination rectangle.
        /// </summary>
        protected bool IsDirty
        {
            get => isDirty;
            set => isDirty = value && !blockSetIsDirty;
        }
        private bool isDirty;

        public virtual void MarkAsDirty()
        {
            IsDirty = true;
            IsSizeDirty = true;
            IsNestedScaleDirty = true;
        }

        public bool IsMouseHover { get; private set; }
        public bool IsMousePressed { get; private set; }

        private bool isDispossed;
        private bool blockSetIsDirty = false;

        IControl IControl.SetInfo(object info) => SetInfo(info);
        public TControl SetInfo(object info)
        {
            Info = info;
            return ThisAsT;
        }

        IControl IControl.SetZIndex(float zIndex) => SetZIndex(zIndex);
        public TControl SetZIndex(float zIndex)
        {
            ZIndex = zIndex;
            Parent?.SetNeedToShortChildren();
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
        protected void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Vector2 origin)
        {
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color, Rotation, origin, SpriteEffects, LayerDepthDraw);
        }

        private event CallbackMouseEvent onMouseEnter;
        private event CallbackMouseEvent onMouseLeave;
        private event CallbackMouseEvent onMousePressed;
        private event CallbackMouseEvent onMouseMoved;
        private event CallbackMouseEvent onMouseReleased;
        private event CallbackMouseEvent onClick;
        private event CallbackMultipleTouchpointsEvent onMultipleTouchpoints;
        private event Action onUpdateDestinationRectangle;
        private event Action<GameTime> onUpdateEvent;

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
        public TControl AddOnClick(CallbackMouseEvent onClick)
        {
            this.onClick += onClick;
            return ThisAsT;
        }
        IControl IControl.AddOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints) => AddOnMultipleTouchpoints(onMultipleTouchpoints);
        public TControl AddOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints)
        {
            this.onMultipleTouchpoints += onMultipleTouchpoints;
            return ThisAsT;
        }

        IControl IControl.AddOnUpddateDestinationRectangle(Action onUpdateDestinationRectangle) => AddOnUpddateDestinationRectangle(onUpdateDestinationRectangle);
        public TControl AddOnUpddateDestinationRectangle(Action onUpdateDestinationRectangle)
        {
            this.onUpdateDestinationRectangle += onUpdateDestinationRectangle;
            return ThisAsT;
        }

        IControl IControl.AddOnUpdateEvent(Action<GameTime> onUpdateEvent)
        {
            AddOnUpdateEvent(onUpdateEvent);
            return this;
        }
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
        public void RemoveOnUpdateEvent(Action<GameTime> onUpdateEvent) => this.onUpdateEvent -= onUpdateEvent;
        public void RemoveOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpointsEvent) => this.onMultipleTouchpoints -= onMultipleTouchpointsEvent;

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
        /// Block mouse events to not interact with elements below of it
        /// </summary>
        /// <returns>The control</returns>
        public TControl BlockMouseEvents()
        {
            CallbackMouseEvent doNothingEvent = args => { };

            AddOnMouseEnter(doNothingEvent);
            AddOnMouseLeave(doNothingEvent);
            AddOnMousePressed(doNothingEvent);
            AddOnMouseMoved(doNothingEvent);
            AddOnMouseReleased(doNothingEvent);
            AddOnClick(doNothingEvent);

            return ThisAsT;
        }

        public void FireOnMouseEnter(ControlMouseEventArgs args) => FireMouseEvent(onMouseEnter, args);
        public void FireOnMouseLeave(ControlMouseEventArgs args) => FireMouseEvent(onMouseLeave, args);
        public void FireOnPressed(ControlMouseEventArgs args) => FireMouseEvent(onMousePressed, args);
        public void FireOnMoved(ControlMouseEventArgs args) => FireMouseEvent(onMouseMoved, args);
        public void FireOnReleased(ControlMouseEventArgs args) => FireMouseEvent(onMouseReleased, args);
        public void FireOnClick(ControlMouseEventArgs args) => FireMouseEvent(onClick, args);
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
        /// Virtual method to check if the provided point position intersects the control
        /// </summary>
        /// <param name="pointToCompare">The point position, usually mouse input or touch input</param>
        /// <returns>True if the provided point position intersects the control</returns>
        public virtual bool Intersects(Point pointToCompare)
            => Intersection.IntersectsWithPoint(DestinationRectangle, Origin, pointToCompare);

        IControl IControl.AddToScreen(IContainer parent) => AddToScreen(parent);
        public TControl AddToScreen(IContainer parent = null)
        {
            var addToParent = parent ?? ServiceProvider.RootPanel;
            addToParent.AddChild(this);
            return ThisAsT;
        }

        public void CalculateSizeIfIsDirty()
        {
            if (IsSizeDirty)
            {
                IsSizeDirty = false;
                CalculatedNestedScaleIfDirty();
                Size = CalculateSize();
            }
        }

        public virtual void RemoveFromScreen()
        {
            Parent?.RemoveChild(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void CalculatedNestedScaleIfDirty()
        {
            if (IsNestedScaleDirty)
            {
                IsNestedScaleDirty = false;
                NestedScale = CalculateNestedScale();
            }
        }

        public virtual Vector2 CalculateNestedScale() => Parent?.CalculateNestedScale() ?? Vector2.One;
        protected virtual void Dispose(bool disposing)
        {
            if (!isDispossed)
            {
                if (disposing)
                {
                    RemoveFromScreen();
                }

                isDispossed = true;
            }
        }

        protected virtual Vector2 CalculateSize() => Size * NestedScale;

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

        // <summary>
        /// Update destination rectangle, but only if needed (eg if something changed since last update).
        /// </summary>
        protected virtual void UpdateDestinationRectsIfDirty()
        {
            // if dirty, update destination rectangles
            if (IsDirty)
            {
                blockSetIsDirty = true;
                CalculatedNestedScaleIfDirty();
                CalculateSizeIfIsDirty();
                UpdateDestinationRects();
                blockSetIsDirty = false;
            }
        }

        /// <summary>
        /// Calculate and return the destination rectangle, eg the space this control is rendered on screen.
        /// </summary>
        /// <returns>Destination rectangle.</returns>
        public virtual Rectangle CalculateDestinationRectangle()
        {
            var parent = Parent ?? ServiceProvider.RootPanel;
            var parentDestinationRectangle = parent.DestinationRectangle;

            // Calculate some helpers
            var parentLeft = parentDestinationRectangle.X;
            var parentTop = parentDestinationRectangle.Y;

            var positionAnchor = PositionAnchor * parent.NestedScale;

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
