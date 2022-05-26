using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Enums;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Interfaces
{
    public interface IControl : IDisposable
    {
        int Id { get; }
        /// <summary>
        /// Used to storage any extra info to control
        /// </summary>
        object Info { get; set;  }
        Color Color { get; set; }
        float ZIndex { get; }
        SpriteEffects SpriteEffects { get; set; }
        Vector2 Origin { get; set; }
        float Rotation { get; }
        IContainer Parent { get; set; }
        Anchor Anchor { get; set; }
        Vector2 PositionAnchor { get; set; }
        Vector2 Size { get; set; }
        Vector2 NestedScale { get; }
        Rectangle DestinationRectangle { get; }
        void MarkAsDirty();
        bool IsMouseHover { get; }
        bool IsMousePressed { get; }
        IControl SetInfo(object info);
        IControl SetZIndex(float zIndex);
        IControl SetRotationInDegree(float rotationInDegree);
        IControl SetRotation(float rotation);
        IControl SetOrigin(Vector2 origin);
        IControl SetOriginRate(float originRate);
        IControl SetOriginRate(Vector2 originRate);
        IControl SetOriginRate(Vector2 originRate, Vector2 size);
        IControl SetColor(Color color);
        IControl SetAnchor(Anchor anchor);
        Vector2 GetPosition();
        IControl SetPosition(float x, float y);
        IControl SetPosition(Vector2 position);
        IControl SetMouseEventsColor(Color hoverColor, Color pressedColor);
        void OnBeforeDraw();
        void Draw(SpriteBatch spriteBatch);
        IControl AddOnMouseEnter(CallbackMouseEvent onMouseEnter);
        IControl AddOnMouseLeave(CallbackMouseEvent onMouseLeave);
        IControl AddOnMousePressed(CallbackMouseEvent onMousePressed);
        IControl AddOnMouseMoved(CallbackMouseEvent onMouseMoved);
        IControl AddOnMouseReleased(CallbackMouseEvent onMouseReleased);
        IControl AddOnClick(CallbackMouseEvent onClick);
        IControl AddOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints);
        IControl AddOnUpddateDestinationRectangle(Action onUpdateDestinationRectangle);
        IControl AddOnUpdateEvent(Action<GameTime> onUpdateEvent);
        void RemoveOnMouseEnter(CallbackMouseEvent onMouseEnter);
        void RemoveOnMouseLeave(CallbackMouseEvent onMouseLeave);
        void RemoveOnMousePressed(CallbackMouseEvent onPressed);
        void RemoveOnMouseMoved(CallbackMouseEvent onMouseMoved);
        void RemoveOnMouseReleased(CallbackMouseEvent onMouseReleased);
        void RemoveOnClick(CallbackMouseEvent onClick);
        void RemoveOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints);
        void RemoveOnUpdateEvent(Action<GameTime> onUpdateEvent);
        void CleanOnUpdateEvent();
        void SetMouseHover(bool isMouseHover);
        void SetMousePressed(bool isMousePressed);
        IControl BlockMouseEvents();
        void FireOnMouseEnter(ControlMouseEventArgs args);
        void FireOnMouseLeave(ControlMouseEventArgs args);
        void FireOnPressed(ControlMouseEventArgs args);
        void FireOnMoved(ControlMouseEventArgs args);
        void FireOnReleased(ControlMouseEventArgs args);
        void FireOnClick(ControlMouseEventArgs args);
        void FireOnMultipleTouchpoints(ControlMultipleTouchpointsEventArgs args);
        void FireOnUpdateEvent(GameTime gameTime);
        bool Intersects(Point pointToCompare);
        IControl AddToScreen(IContainer parent = null);
        void CalculateSizeIfIsDirty();
        void RemoveFromScreen();
        Rectangle CalculateDestinationRectangle();
        Vector2 CalculateNestedScale();
    }
}
