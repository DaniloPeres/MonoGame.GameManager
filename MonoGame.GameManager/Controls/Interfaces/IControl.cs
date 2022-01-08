using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.MouseEvent;
using MonoGame.GameManager.Enums;
using System;

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
        void Move(Vector2 distance);
        IControl SetMouseEventsColor(Color hoverColor, Color pressedColor);
        void OnBeforeDraw();
        void Draw(SpriteBatch spriteBatch);
        IControl AddOnMouseEnter(CallbackEvent onMouseEnter);
        IControl AddOnMouseLeave(CallbackEvent onMouseLeave);
        IControl AddOnMousePressed(CallbackEvent onMousePressed);
        IControl AddOnMouseMoved(CallbackEvent onMouseMoved);
        IControl AddOnMouseReleased(CallbackEvent onMouseReleased);
        IControl AddOnClick(CallbackEvent onClick);
        IControl AddOnUpddateDestinationRectangle(Action onUpdateDestinationRectangle);
        IControl AddOnUpdateEvent(Action<GameTime> onUpdateEvent);
        void RemoveOnMouseEnter(CallbackEvent onMouseEnter);
        void RemoveOnMouseLeave(CallbackEvent onMouseLeave);
        void RemoveOnMousePressed(CallbackEvent onPressed);
        void RemoveOnMouseMoved(CallbackEvent onMouseMoved);
        void RemoveOnMouseReleased(CallbackEvent onMouseReleased);
        void RemoveOnClick(CallbackEvent onClick);
        void RemoveOnUpdateEvent(Action<GameTime> onUpdateEvent);
        void CleanOnUpdateEvent();
        void SetMouseHover(bool isMouseHover);
        void SetMousePressed(bool isMousePressed);
        IControl BlockMouseEvents();
        void FireOnMouseEnter(ControlEventArgs args);
        void FireOnMouseLeave(ControlEventArgs args);
        void FireOnPressed(ControlEventArgs args);
        void FireOnMoved(ControlEventArgs args);
        void FireOnReleased(ControlEventArgs args);
        void FireOnClick(ControlEventArgs args);
        void FireOnUpdateEvent(GameTime gameTime);
        bool Intersects(Point pointToCompare);
        IControl AddToScreen(IContainer parent = null);
        void CalculateSizeIfIsDirty();
        void RemoveFromScreen();
        Rectangle CalculateDestinationRectangle();
    }
}
