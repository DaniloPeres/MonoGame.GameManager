using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Polls up to four gamepads once per frame and detects the buttons pressed and released in each frame.
    /// </summary>
    public class GamePadInputListener
    {
        /// <summary>The number of players polled.</summary>
        public const int MaxPlayers = 4;

        private static readonly Buttons[] AllButtons = (Buttons[])Enum.GetValues(typeof(Buttons));

        private readonly GamePadState[] currentStates = new GamePadState[MaxPlayers];
        private readonly GamePadState[] previousStates = new GamePadState[MaxPlayers];
        private readonly bool[] hasState = new bool[MaxPlayers];
        private readonly float[] vibrationTimeLeft = new float[MaxPlayers];

        /// <summary>Raised once for every button pressed in the frame.</summary>
        public event Action<PlayerIndex, Buttons> ButtonPressed;

        /// <summary>Raised once for every button released in the frame.</summary>
        public event Action<PlayerIndex, Buttons> ButtonReleased;

        public event Action<PlayerIndex> Connected;

        public event Action<PlayerIndex> Disconnected;

        /// <summary>How the dead zone of the thumbsticks is applied.</summary>
        public GamePadDeadZone DeadZone { get; set; } = GamePadDeadZone.IndependentAxes;

        public GamePadState GetState(PlayerIndex player = PlayerIndex.One) => currentStates[(int)player];

        public GamePadState GetPreviousState(PlayerIndex player = PlayerIndex.One) => previousStates[(int)player];

        public bool IsConnected(PlayerIndex player = PlayerIndex.One) => currentStates[(int)player].IsConnected;

        public bool IsButtonDown(Buttons button, PlayerIndex player = PlayerIndex.One) => currentStates[(int)player].IsButtonDown(button);

        public bool IsButtonUp(Buttons button, PlayerIndex player = PlayerIndex.One) => currentStates[(int)player].IsButtonUp(button);

        /// <summary>True if the button was held down in the previous frame.</summary>
        public bool WasButtonDown(Buttons button, PlayerIndex player = PlayerIndex.One) => previousStates[(int)player].IsButtonDown(button);

        /// <summary>True if the button was held down in the previous frame on any gamepad.</summary>
        public bool WasButtonDownOnAny(Buttons button)
        {
            for (var i = 0; i < MaxPlayers; i++)
            {
                if (previousStates[i].IsButtonDown(button))
                    return true;
            }
            return false;
        }

        /// <summary>True only in the frame the button was pressed.</summary>
        public bool WasButtonPressed(Buttons button, PlayerIndex player = PlayerIndex.One)
            => currentStates[(int)player].IsButtonDown(button) && previousStates[(int)player].IsButtonUp(button);

        /// <summary>True only in the frame the button was released.</summary>
        public bool WasButtonReleased(Buttons button, PlayerIndex player = PlayerIndex.One)
            => currentStates[(int)player].IsButtonUp(button) && previousStates[(int)player].IsButtonDown(button);

        /// <summary>True if the button is held on any connected gamepad.</summary>
        public bool IsButtonDownOnAny(Buttons button)
        {
            for (var i = 0; i < MaxPlayers; i++)
            {
                if (currentStates[i].IsConnected && currentStates[i].IsButtonDown(button))
                    return true;
            }
            return false;
        }

        /// <summary>True if the button was pressed in this frame on any gamepad, returning the player.</summary>
        public bool WasButtonPressedOnAny(Buttons button, out PlayerIndex player)
        {
            for (var i = 0; i < MaxPlayers; i++)
            {
                if (WasButtonPressed(button, (PlayerIndex)i))
                {
                    player = (PlayerIndex)i;
                    return true;
                }
            }
            player = PlayerIndex.One;
            return false;
        }

        public Vector2 LeftThumbStick(PlayerIndex player = PlayerIndex.One) => currentStates[(int)player].ThumbSticks.Left;

        public Vector2 RightThumbStick(PlayerIndex player = PlayerIndex.One) => currentStates[(int)player].ThumbSticks.Right;

        public float LeftTrigger(PlayerIndex player = PlayerIndex.One) => currentStates[(int)player].Triggers.Left;

        public float RightTrigger(PlayerIndex player = PlayerIndex.One) => currentStates[(int)player].Triggers.Right;

        /// <summary>Vibrates a gamepad for a duration (the motors' speed is from 0 to 1).</summary>
        public void Vibrate(PlayerIndex player, float leftMotor, float rightMotor, float seconds)
        {
            if (!GamePad.SetVibration(player, MathHelper.Clamp(leftMotor, 0f, 1f), MathHelper.Clamp(rightMotor, 0f, 1f)))
                return;
            vibrationTimeLeft[(int)player] = Math.Max(0f, seconds);
        }

        public void StopVibration(PlayerIndex player)
        {
            vibrationTimeLeft[(int)player] = 0f;
            GamePad.SetVibration(player, 0f, 0f);
        }

        public void Update(GameTime gameTime)
        {
            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (var i = 0; i < MaxPlayers; i++)
            {
                var player = (PlayerIndex)i;
                Update(player, GamePad.GetState(player, DeadZone));

                if (vibrationTimeLeft[i] > 0f)
                {
                    vibrationTimeLeft[i] -= elapsed;
                    if (vibrationTimeLeft[i] <= 0f)
                        StopVibration(player);
                }
            }
        }

        /// <summary>
        /// Updates the state of one player, for example to simulate input.
        /// </summary>
        public void Update(PlayerIndex player, GamePadState state)
        {
            var index = (int)player;
            previousStates[index] = hasState[index] ? currentStates[index] : state;
            currentStates[index] = state;
            hasState[index] = true;

            var previous = previousStates[index];
            if (state.IsConnected && !previous.IsConnected)
                Connected?.Invoke(player);
            else if (!state.IsConnected && previous.IsConnected)
                Disconnected?.Invoke(player);

            if (ButtonPressed == null && ButtonReleased == null)
                return;

            foreach (var button in AllButtons)
            {
                var isDown = state.IsButtonDown(button);
                var wasDown = previous.IsButtonDown(button);
                if (isDown && !wasDown)
                    ButtonPressed?.Invoke(player, button);
                else if (!isDown && wasDown)
                    ButtonReleased?.Invoke(player, button);
            }
        }
    }
}
