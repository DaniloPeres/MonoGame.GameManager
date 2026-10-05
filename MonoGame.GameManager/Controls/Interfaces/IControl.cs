using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Enums;
using System;

namespace MonoGame.GameManager.Controls.Interfaces
{
    /// <summary>
    /// A UI or game element in the control tree. It combines the layout (<see cref="ILayoutElement"/>), drawing
    /// (<see cref="IRenderable"/>) and input (<see cref="IInputTarget"/>) contracts with the fluent API.
    /// </summary>
    public interface IControl : ILayoutElement, IRenderable, IInputTarget, IDisposable
    {
        /// <summary>A unique identifier.</summary>
        int Id { get; }

        /// <summary>An optional name, used by <see cref="IContainer.FindByName"/>.</summary>
        string Name { get; set; }

        /// <summary>
        /// Used to store any extra info in the control.
        /// </summary>
        object Info { get; set; }

        /// <summary>The container of the control, or null when it is not on the screen.</summary>
        IContainer Parent { get; set; }

        /// <summary>True after <see cref="IDisposable.Dispose"/> was called.</summary>
        bool IsDisposed { get; }

        /// <summary>Raised once when the control is disposed.</summary>
        event Action<IControl> Disposed;

        IControl SetInfo(object info);
        IControl SetName(string name);
        IControl SetZIndex(float zIndex);
        IControl SetRotationInDegree(float rotationInDegree);
        IControl SetRotation(float rotation);
        IControl SetOrigin(Vector2 origin);
        IControl SetOriginRate(float originRate);
        IControl SetOriginRate(Vector2 originRate);
        IControl SetOriginRate(Vector2 originRate, Vector2 size);
        IControl SetColor(Color color);
        IControl SetOpacity(float opacity);
        IControl SetIsVisible(bool isVisible);
        IControl SetIsEnabled(bool isEnabled);
        IControl SetAnchor(Anchor anchor);
        IControl SetPosition(float x, float y);
        IControl SetPosition(Vector2 position);
        IControl SetMouseEventsColor(Color hoverColor, Color pressedColor);
        IControl AddOnMouseEnter(CallbackMouseEvent onMouseEnter);
        IControl AddOnMouseLeave(CallbackMouseEvent onMouseLeave);
        IControl AddOnMousePressed(CallbackMouseEvent onMousePressed);
        IControl AddOnMouseMoved(CallbackMouseEvent onMouseMoved);
        IControl AddOnMouseReleased(CallbackMouseEvent onMouseReleased);
        IControl AddOnClick(CallbackMouseEvent onClick);
        IControl AddOnMouseWheel(CallbackMouseEvent onMouseWheel);
        IControl AddOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints);
        IControl AddOnUpdateDestinationRectangle(Action onUpdateDestinationRectangle);
        IControl AddOnUpdateEvent(Action<GameTime> onUpdateEvent);
        void RemoveOnMouseEnter(CallbackMouseEvent onMouseEnter);
        void RemoveOnMouseLeave(CallbackMouseEvent onMouseLeave);
        void RemoveOnMousePressed(CallbackMouseEvent onPressed);
        void RemoveOnMouseMoved(CallbackMouseEvent onMouseMoved);
        void RemoveOnMouseReleased(CallbackMouseEvent onMouseReleased);
        void RemoveOnClick(CallbackMouseEvent onClick);
        void RemoveOnMouseWheel(CallbackMouseEvent onMouseWheel);
        void RemoveOnMultipleTouchpoints(CallbackMultipleTouchpointsEvent onMultipleTouchpoints);
        void RemoveOnUpdateDestinationRectangle(Action onUpdateDestinationRectangle);
        void RemoveOnUpdateEvent(Action<GameTime> onUpdateEvent);
        void CleanOnUpdateEvent();

        /// <summary>
        /// Blocks the pointer events so the controls below this one do not receive them.
        /// </summary>
        IControl BlockMouseEvents();

        void FireOnUpdateEvent(GameTime gameTime);

        /// <summary>Adds the control to a container, or to the current screen when no container is given.</summary>
        IControl AddToScreen(IContainer parent = null);

        /// <summary>Removes the control from its container (the control can be added again).</summary>
        void RemoveFromScreen();
    }
}
