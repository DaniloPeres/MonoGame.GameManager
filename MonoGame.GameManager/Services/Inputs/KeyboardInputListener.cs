using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Polls the keyboard once per frame, detects the keys pressed and released in each frame and relays the
    /// typed characters (desktop platforms).
    /// </summary>
    /// <example>
    /// <code>
    /// var keyboard = ServiceProvider.Input.Keyboard;
    /// if (keyboard.WasKeyPressed(Keys.Space))
    ///     Jump();
    /// </code>
    /// </example>
    public class KeyboardInputListener
    {
        private KeyboardState currentState;
        private KeyboardState previousState;
        private Keys[] currentKeys = Array.Empty<Keys>();
        private Keys[] previousKeys = Array.Empty<Keys>();
        private bool hasState;

        /// <summary>Raised once for every key pressed in the frame.</summary>
        public event Action<Keys> KeyPressed;

        /// <summary>Raised once for every key released in the frame.</summary>
        public event Action<Keys> KeyReleased;

        /// <summary>Raised for every typed character, with key repetition (desktop platforms only).</summary>
        public event Action<char> TextEntered;

        public KeyboardState CurrentState => currentState;

        public KeyboardState PreviousState => previousState;

        /// <summary>The keys held down in the current frame.</summary>
        public Keys[] PressedKeys => currentKeys;

        public bool IsKeyDown(Keys key) => currentState.IsKeyDown(key);

        public bool IsKeyUp(Keys key) => currentState.IsKeyUp(key);

        /// <summary>True only in the frame the key was pressed.</summary>
        public bool WasKeyPressed(Keys key) => currentState.IsKeyDown(key) && previousState.IsKeyUp(key);

        /// <summary>True only in the frame the key was released.</summary>
        public bool WasKeyReleased(Keys key) => currentState.IsKeyUp(key) && previousState.IsKeyDown(key);

        public bool IsShiftDown => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);

        public bool IsControlDown => IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);

        public bool IsAltDown => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);

        public void Update(GameTime gameTime) => Update(Keyboard.GetState());

        /// <summary>
        /// Updates the listener with a given state, for example to simulate input.
        /// </summary>
        public void Update(KeyboardState state)
        {
            previousState = hasState ? currentState : state;
            currentState = state;
            previousKeys = hasState ? currentKeys : state.GetPressedKeys();
            currentKeys = state.GetPressedKeys();
            hasState = true;

            if (KeyPressed != null)
            {
                foreach (var key in currentKeys)
                {
                    if (Array.IndexOf(previousKeys, key) < 0)
                        KeyPressed(key);
                }
            }

            if (KeyReleased != null)
            {
                foreach (var key in previousKeys)
                {
                    if (Array.IndexOf(currentKeys, key) < 0)
                        KeyReleased(key);
                }
            }
        }

        /// <summary>Relays a typed character to <see cref="TextEntered"/>.</summary>
        public void OnTextInput(char character) => TextEntered?.Invoke(character);
    }
}
