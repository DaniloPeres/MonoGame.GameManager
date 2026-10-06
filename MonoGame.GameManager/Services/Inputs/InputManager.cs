using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;
using System.Reflection;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Default <see cref="IInputManager"/>: polls the keyboard, gamepads, mouse and touch panel once per frame and
    /// evaluates the named actions and axes of its <see cref="InputMap"/>.
    /// </summary>
    public class InputManager : IInputManager
    {
        /// <param name="window">The game window, used to receive typed text on desktop platforms (optional).</param>
        /// <param name="windowToScreen">Converts window coordinates to virtual screen coordinates (optional).</param>
        public InputManager(GameWindow window = null, Func<Vector2, Vector2> windowToScreen = null)
            : this(new KeyboardInputListener(), new MouseInputListener(windowToScreen), new TouchInputListener(windowToScreen), new GamePadInputListener())
        {
            if (window != null)
                TextInputBridge.TrySubscribe(window, Keyboard.OnTextInput);
        }

        public InputManager(KeyboardInputListener keyboard, MouseInputListener mouse, TouchInputListener touch, GamePadInputListener gamePads, InputMap map = null)
        {
            Keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
            Mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
            Touch = touch ?? throw new ArgumentNullException(nameof(touch));
            GamePads = gamePads ?? throw new ArgumentNullException(nameof(gamePads));
            Map = map ?? new InputMap();
        }

        public KeyboardInputListener Keyboard { get; }

        public MouseInputListener Mouse { get; }

        public TouchInputListener Touch { get; }

        public GamePadInputListener GamePads { get; }

        public InputMap Map { get; }

        public bool IsEnabled { get; set; } = true;

        public IControl FocusedControl { get; set; }

        public void Update(GameTime gameTime)
        {
            if (!IsEnabled)
                return;

            Keyboard.Update(gameTime);
            GamePads.Update(gameTime);
            Mouse.Update(gameTime);
            Touch.Update(gameTime);
        }

        public bool IsActionDown(string action) => EvaluateAction(action, previousFrame: false);

        public bool IsActionPressed(string action) => EvaluateAction(action, previousFrame: false) && !EvaluateAction(action, previousFrame: true);

        public bool IsActionReleased(string action) => !EvaluateAction(action, previousFrame: false) && EvaluateAction(action, previousFrame: true);

        public float GetAxis(string axis)
        {
            var value = 0f;
            foreach (var binding in Map.GetAxisBindings(axis))
            {
                var bindingValue = EvaluateAxis(binding);
                if (Math.Abs(bindingValue) > Math.Abs(value))
                    value = bindingValue;
            }
            return MathHelper.Clamp(value, -1f, 1f);
        }

        public Vector2 GetVector(string horizontalAxis, string verticalAxis)
        {
            var vector = new Vector2(GetAxis(horizontalAxis), GetAxis(verticalAxis));
            return vector.LengthSquared() > 1f ? Vector2.Normalize(vector) : vector;
        }

        /// <summary>
        /// Evaluates an action against the device states of the current or the previous frame. Using the previous
        /// states of the listeners (instead of remembering the results) keeps the edge detection right when the
        /// listeners are updated with simulated states.
        /// </summary>
        private bool EvaluateAction(string action, bool previousFrame)
        {
            foreach (var binding in Map.GetBindings(action))
            {
                switch (binding.Type)
                {
                    case InputBindingType.Key:
                        if (previousFrame ? Keyboard.WasKeyDown(binding.Key) : Keyboard.IsKeyDown(binding.Key))
                            return true;
                        break;
                    case InputBindingType.GamePadButton:
                        if (IsGamePadButtonDown(binding, previousFrame))
                            return true;
                        break;
                    case InputBindingType.MouseButton:
                        if (previousFrame ? Mouse.WasButtonDown(binding.MouseButton) : Mouse.IsButtonDown(binding.MouseButton))
                            return true;
                        break;
                }
            }
            return false;
        }

        private bool IsGamePadButtonDown(InputBinding binding, bool previousFrame)
        {
            if (binding.Player.HasValue)
            {
                return previousFrame
                    ? GamePads.WasButtonDown(binding.Button, binding.Player.Value)
                    : GamePads.IsButtonDown(binding.Button, binding.Player.Value);
            }

            return previousFrame ? GamePads.WasButtonDownOnAny(binding.Button) : GamePads.IsButtonDownOnAny(binding.Button);
        }

        private float EvaluateAxis(AxisBinding binding)
        {
            if (binding.IsKeyPair)
                return (Keyboard.IsKeyDown(binding.PositiveKey) ? 1f : 0f) - (Keyboard.IsKeyDown(binding.NegativeKey) ? 1f : 0f);

            var value = 0f;
            if (binding.Player.HasValue)
            {
                value = ReadAxis(binding.Axis, binding.Player.Value);
            }
            else
            {
                for (var i = 0; i < GamePadInputListener.MaxPlayers; i++)
                {
                    var player = (PlayerIndex)i;
                    if (!GamePads.IsConnected(player))
                        continue;
                    var playerValue = ReadAxis(binding.Axis, player);
                    if (Math.Abs(playerValue) > Math.Abs(value))
                        value = playerValue;
                }
            }

            return binding.Invert ? -value : value;
        }

        private float ReadAxis(GamePadAxis axis, PlayerIndex player)
        {
            switch (axis)
            {
                case GamePadAxis.LeftThumbStickX: return GamePads.LeftThumbStick(player).X;
                case GamePadAxis.LeftThumbStickY: return GamePads.LeftThumbStick(player).Y;
                case GamePadAxis.RightThumbStickX: return GamePads.RightThumbStick(player).X;
                case GamePadAxis.RightThumbStickY: return GamePads.RightThumbStick(player).Y;
                case GamePadAxis.LeftTrigger: return GamePads.LeftTrigger(player);
                case GamePadAxis.RightTrigger: return GamePads.RightTrigger(player);
                default: return 0f;
            }
        }

        /// <summary>
        /// Subscribes to <c>GameWindow.TextInput</c> through reflection: the event only exists on desktop builds of
        /// MonoGame, and this library is compiled once for every platform.
        /// </summary>
        private static class TextInputBridge
        {
            public static bool TrySubscribe(GameWindow window, Action<char> onCharacter)
            {
                try
                {
                    var eventInfo = window.GetType().GetEvent("TextInput", BindingFlags.Public | BindingFlags.Instance);
                    var handlerType = eventInfo?.EventHandlerType;
                    var argumentsType = handlerType?.GetMethod("Invoke")?.GetParameters()[1].ParameterType;
                    if (argumentsType == null)
                        return false;

                    var relay = new Relay(onCharacter, argumentsType);
                    if (!relay.IsValid)
                        return false;

                    var handler = Delegate.CreateDelegate(handlerType, relay, typeof(Relay).GetMethod(nameof(Relay.Handle)));
                    eventInfo.AddEventHandler(window, handler);
                    return true;
                }
                catch (Exception)
                {
                    return false; // text input is optional
                }
            }

            private sealed class Relay
            {
                private readonly Action<char> onCharacter;
                private readonly FieldInfo characterField;
                private readonly PropertyInfo characterProperty;

                public Relay(Action<char> onCharacter, Type argumentsType)
                {
                    this.onCharacter = onCharacter;
                    characterField = argumentsType.GetField("Character");
                    characterProperty = characterField == null ? argumentsType.GetProperty("Character") : null;
                }

                public bool IsValid => characterField != null || characterProperty != null;

                public void Handle(object sender, EventArgs args)
                {
                    var value = characterField != null ? characterField.GetValue(args) : characterProperty.GetValue(args);
                    if (value is char character)
                        onCharacter(character);
                }
            }
        }
    }
}
