using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>A gamepad axis that can be bound to a named axis of an <see cref="InputMap"/>.</summary>
    public enum GamePadAxis
    {
        LeftThumbStickX,
        LeftThumbStickY,
        RightThumbStickX,
        RightThumbStickY,
        LeftTrigger,
        RightTrigger
    }

    /// <summary>The kind of device input of an <see cref="InputBinding"/>.</summary>
    public enum InputBindingType
    {
        Key,
        GamePadButton,
        MouseButton
    }

    /// <summary>A key, gamepad button or mouse button bound to an action.</summary>
    public sealed class InputBinding
    {
        private InputBinding(InputBindingType type) => Type = type;

        public InputBindingType Type { get; }

        public Keys Key { get; private set; }

        public Buttons Button { get; private set; }

        public MouseButtons MouseButton { get; private set; }

        /// <summary>The gamepad of the binding, or null for any connected gamepad.</summary>
        public PlayerIndex? Player { get; private set; }

        public static InputBinding ForKey(Keys key) => new InputBinding(InputBindingType.Key) { Key = key };

        public static InputBinding ForGamePadButton(Buttons button, PlayerIndex? player = null)
            => new InputBinding(InputBindingType.GamePadButton) { Button = button, Player = player };

        public static InputBinding ForMouseButton(MouseButtons button)
            => new InputBinding(InputBindingType.MouseButton) { MouseButton = button };

        public override string ToString()
        {
            switch (Type)
            {
                case InputBindingType.Key: return $"Key {Key}";
                case InputBindingType.GamePadButton: return $"GamePad {Button}" + (Player.HasValue ? $" ({Player})" : string.Empty);
                default: return $"Mouse {MouseButton}";
            }
        }
    }

    /// <summary>A pair of keys or a gamepad axis bound to a named axis.</summary>
    public sealed class AxisBinding
    {
        private AxisBinding() { }

        /// <summary>True when the binding is a pair of keys (negative and positive).</summary>
        public bool IsKeyPair { get; private set; }

        public Keys NegativeKey { get; private set; }

        public Keys PositiveKey { get; private set; }

        public GamePadAxis Axis { get; private set; }

        /// <summary>The gamepad of the binding, or null for any connected gamepad.</summary>
        public PlayerIndex? Player { get; private set; }

        /// <summary>When true, the value of the gamepad axis is negated.</summary>
        public bool Invert { get; private set; }

        public static AxisBinding ForKeys(Keys negativeKey, Keys positiveKey)
            => new AxisBinding { IsKeyPair = true, NegativeKey = negativeKey, PositiveKey = positiveKey };

        public static AxisBinding ForGamePadAxis(GamePadAxis axis, PlayerIndex? player = null, bool invert = false)
            => new AxisBinding { Axis = axis, Player = player, Invert = invert };
    }

    /// <summary>
    /// Maps named actions ("Jump", "Pause"...) and axes ("MoveX"...) to keys, gamepad buttons, gamepad axes and
    /// mouse buttons, so the game logic does not depend on a specific device and the controls can be rebound.
    /// </summary>
    /// <example>
    /// <code>
    /// var map = ServiceProvider.Input.Map;
    /// map.Bind("Jump", Keys.Space).Bind("Jump", Buttons.A);
    /// map.BindAxis("MoveX", Keys.A, Keys.D).BindAxis("MoveX", Keys.Left, Keys.Right).BindAxis("MoveX", GamePadAxis.LeftThumbStickX);
    ///
    /// if (ServiceProvider.Input.IsActionPressed("Jump")) Jump();
    /// var moveX = ServiceProvider.Input.GetAxis("MoveX");
    /// </code>
    /// </example>
    public class InputMap
    {
        private readonly Dictionary<string, List<InputBinding>> actions = new Dictionary<string, List<InputBinding>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<AxisBinding>> axes = new Dictionary<string, List<AxisBinding>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>The names of the bound actions.</summary>
        public IEnumerable<string> Actions => actions.Keys;

        /// <summary>The names of the bound axes.</summary>
        public IEnumerable<string> Axes => axes.Keys;

        public InputMap Bind(string action, Keys key) => Bind(action, InputBinding.ForKey(key));

        /// <param name="action">The action name.</param>
        /// <param name="button">The gamepad button.</param>
        /// <param name="player">The gamepad, or null for any connected gamepad.</param>
        public InputMap Bind(string action, Buttons button, PlayerIndex? player = null) => Bind(action, InputBinding.ForGamePadButton(button, player));

        public InputMap Bind(string action, MouseButtons button) => Bind(action, InputBinding.ForMouseButton(button));

        public InputMap Bind(string action, InputBinding binding)
        {
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("The action name cannot be empty.", nameof(action));
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            if (!actions.TryGetValue(action, out var bindings))
            {
                bindings = new List<InputBinding>();
                actions[action] = bindings;
            }
            bindings.Add(binding);
            return this;
        }

        /// <summary>Binds a pair of keys to an axis: the axis is -1 with the negative key and 1 with the positive key.</summary>
        public InputMap BindAxis(string axis, Keys negativeKey, Keys positiveKey) => BindAxis(axis, AxisBinding.ForKeys(negativeKey, positiveKey));

        /// <param name="axis">The axis name.</param>
        /// <param name="gamePadAxis">The gamepad axis.</param>
        /// <param name="player">The gamepad, or null for any connected gamepad.</param>
        /// <param name="invert">
        /// Negates the value. Use it for the Y axes of the thumbsticks, which are positive upwards while the screen
        /// Y axis points downwards.
        /// </param>
        public InputMap BindAxis(string axis, GamePadAxis gamePadAxis, PlayerIndex? player = null, bool invert = false)
            => BindAxis(axis, AxisBinding.ForGamePadAxis(gamePadAxis, player, invert));

        public InputMap BindAxis(string axis, AxisBinding binding)
        {
            if (string.IsNullOrEmpty(axis))
                throw new ArgumentException("The axis name cannot be empty.", nameof(axis));
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            if (!axes.TryGetValue(axis, out var bindings))
            {
                bindings = new List<AxisBinding>();
                axes[axis] = bindings;
            }
            bindings.Add(binding);
            return this;
        }

        /// <summary>Removes every binding of an action.</summary>
        public bool Unbind(string action) => action != null && actions.Remove(action);

        /// <summary>Removes every binding of an axis.</summary>
        public bool UnbindAxis(string axis) => axis != null && axes.Remove(axis);

        public void Clear()
        {
            actions.Clear();
            axes.Clear();
        }

        public bool HasAction(string action) => action != null && actions.ContainsKey(action);

        public bool HasAxis(string axis) => axis != null && axes.ContainsKey(axis);

        public IReadOnlyList<InputBinding> GetBindings(string action)
            => action != null && actions.TryGetValue(action, out var bindings) ? bindings : (IReadOnlyList<InputBinding>)Array.Empty<InputBinding>();

        public IReadOnlyList<AxisBinding> GetAxisBindings(string axis)
            => axis != null && axes.TryGetValue(axis, out var bindings) ? bindings : (IReadOnlyList<AxisBinding>)Array.Empty<AxisBinding>();
    }
}
