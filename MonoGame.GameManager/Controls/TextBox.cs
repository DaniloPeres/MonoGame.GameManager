using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Services.Inputs;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A single-line text input (player name, chat, seed...). Click it to type; Enter submits, Escape or a click
    /// elsewhere ends the edition.
    /// </summary>
    /// <remarks>
    /// The characters come from <see cref="KeyboardInputListener.TextEntered"/>, which is available on desktop
    /// platforms (it uses the text input of the game window, with the keyboard layout and key repetition of the
    /// system). The focused text box is <see cref="IInputManager.FocusedControl"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// new TextBox(font, new Vector2(20, 20), new Vector2(260, 40))
    ///     .SetPlaceholder("Your name")
    ///     .SetMaxLength(16)
    ///     .AddOnSubmit(name => StartGame(name))
    ///     .AddToScreen();
    /// </code>
    /// </example>
    public class TextBox : ScalableControlAbstract<TextBox>
    {
        private const float CaretBlinkInterval = 0.5f;
        private readonly IInputManager inputOverride;
        private IInputManager subscribedInput;
        private Action<string> onTextChanged;
        private Action<string> onSubmit;
        private Action<bool> onFocusChanged;
        private string text = string.Empty;
        private int caretIndex;
        private float caretTime;
        private float scrollOffset;

        /// <param name="spriteFont">The font of the text.</param>
        /// <param name="position">The position.</param>
        /// <param name="size">The size of the box.</param>
        /// <param name="input">The input manager (default: <see cref="ServiceProvider.Input"/>).</param>
        public TextBox(SpriteFont spriteFont, Vector2 position, Vector2 size, IInputManager input = null)
        {
            SpriteFont = spriteFont;
            PositionAnchor = position;
            Size = size;
            inputOverride = input;
            AddOnClick(OnClicked);
            AddOnUpdateEvent(OnUpdate);
        }

        public SpriteFont SpriteFont { get; set; }

        /// <summary>The text (never null).</summary>
        public string Text => text;

        /// <summary>The text shown in gray while the box is empty.</summary>
        public string Placeholder { get; set; } = string.Empty;

        /// <summary>The maximum number of characters (0 = no limit).</summary>
        public int MaxLength { get; set; }

        /// <summary>When true, the characters are hidden with <see cref="PasswordCharacter"/>.</summary>
        public bool IsPassword { get; set; }

        public char PasswordCharacter { get; set; } = '*';

        /// <summary>Accepts or rejects every typed character (null = every printable character).</summary>
        public Func<char, bool> CharacterFilter { get; set; }

        public bool IsFocused { get; private set; }

        /// <summary>The position of the caret, from 0 (before the first character) to the length of the text.</summary>
        public int CaretIndex => caretIndex;

        public Thickness Padding { get; set; } = new Thickness(8f, 4f);

        public Color BackgroundColor { get; set; } = new Color(30, 30, 30);

        public Color TextColor { get; set; } = Color.White;

        public Color PlaceholderColor { get; set; } = Color.Gray;

        public Color BorderColor { get; set; } = new Color(110, 110, 110);

        public Color FocusedBorderColor { get; set; } = new Color(80, 160, 230);

        public float BorderThickness { get; set; } = 1f;

        public Color CaretColor { get; set; } = Color.White;

        private IInputManager Input => inputOverride ?? ServiceProvider.Input;

        /// <summary>Replaces the text (cut to <see cref="MaxLength"/>) and moves the caret to its end.</summary>
        public TextBox SetText(string text, bool notify = true)
        {
            var value = text ?? string.Empty;
            if (MaxLength > 0 && value.Length > MaxLength)
                value = value.Substring(0, MaxLength);
            caretIndex = value.Length;
            return ChangeText(value, notify);
        }

        public TextBox SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder ?? string.Empty;
            return this;
        }

        public TextBox SetMaxLength(int maxLength)
        {
            MaxLength = Math.Max(0, maxLength);
            return SetText(text, false);
        }

        public TextBox SetIsPassword(bool isPassword)
        {
            IsPassword = isPassword;
            return this;
        }

        public TextBox SetCharacterFilter(Func<char, bool> characterFilter)
        {
            CharacterFilter = characterFilter;
            return this;
        }

        public TextBox SetColors(Color background, Color text, Color border, Color focusedBorder)
        {
            BackgroundColor = background;
            TextColor = text;
            BorderColor = border;
            FocusedBorderColor = focusedBorder;
            return this;
        }

        /// <summary>Starts receiving the keyboard.</summary>
        public TextBox Focus()
        {
            if (IsFocused || IsDisposed)
                return this;

            var input = Input;
            if (input != null)
            {
                if (input.FocusedControl is TextBox previous && !ReferenceEquals(previous, this))
                    previous.Blur();
                input.FocusedControl = this;
                input.Keyboard.TextEntered += OnTextEntered;
                input.Keyboard.KeyPressed += OnKeyPressed;
                subscribedInput = input;
            }

            IsFocused = true;
            caretTime = 0f;
            onFocusChanged?.Invoke(true);
            return this;
        }

        /// <summary>Stops receiving the keyboard.</summary>
        public TextBox Blur()
        {
            if (!IsFocused)
                return this;

            if (subscribedInput != null)
            {
                subscribedInput.Keyboard.TextEntered -= OnTextEntered;
                subscribedInput.Keyboard.KeyPressed -= OnKeyPressed;
                if (ReferenceEquals(subscribedInput.FocusedControl, this))
                    subscribedInput.FocusedControl = null;
                subscribedInput = null;
            }

            IsFocused = false;
            onFocusChanged?.Invoke(false);
            return this;
        }

        /// <summary>Adds a callback invoked with the new text when it changes.</summary>
        public TextBox AddOnTextChanged(Action<string> onTextChanged)
        {
            this.onTextChanged += onTextChanged;
            return this;
        }

        public TextBox RemoveOnTextChanged(Action<string> onTextChanged)
        {
            this.onTextChanged -= onTextChanged;
            return this;
        }

        /// <summary>Adds a callback invoked with the text when Enter is pressed.</summary>
        public TextBox AddOnSubmit(Action<string> onSubmit)
        {
            this.onSubmit += onSubmit;
            return this;
        }

        public TextBox RemoveOnSubmit(Action<string> onSubmit)
        {
            this.onSubmit -= onSubmit;
            return this;
        }

        /// <summary>Adds a callback invoked when the box gains (true) or loses (false) the keyboard.</summary>
        public TextBox AddOnFocusChanged(Action<bool> onFocusChanged)
        {
            this.onFocusChanged += onFocusChanged;
            return this;
        }

        /// <summary>Handles a typed character (it is called by the keyboard while focused; it can simulate typing).</summary>
        public void OnTextEntered(char character)
        {
            switch (character)
            {
                case '\b':
                    if (caretIndex > 0)
                    {
                        caretIndex--;
                        ChangeText(text.Remove(caretIndex, 1), true);
                    }
                    return;
                case '\r':
                case '\n':
                    onSubmit?.Invoke(text);
                    return;
                case (char)27: // Escape
                    Blur();
                    return;
            }

            if (char.IsControl(character) || (MaxLength > 0 && text.Length >= MaxLength))
                return;
            if (CharacterFilter != null && !CharacterFilter(character))
                return;
            if (SpriteFont != null && !SpriteFont.Characters.Contains(character) && !SpriteFont.DefaultCharacter.HasValue)
                return; // the font cannot draw it

            ChangeText(text.Insert(caretIndex, character.ToString()), true);
            caretIndex++;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var opacity = NestedOpacity;
            var bounds = LocalBounds;
            DrawLocalRectangle(spriteBatch, bounds, BackgroundColor * opacity);
            DrawLocalBorder(spriteBatch, bounds, (IsFocused ? FocusedBorderColor : BorderColor) * opacity, BorderThickness);
            if (SpriteFont == null)
                return;

            var area = new RectangleF(Padding.Left, Padding.Top, bounds.Width - Padding.Horizontal, bounds.Height - Padding.Vertical);
            if (area.Width <= 0f || area.Height <= 0f)
                return;

            var displayText = GetDisplayText();
            var caretX = MeasureWidth(displayText.Substring(0, Math.Min(caretIndex, displayText.Length)));
            if (caretX - scrollOffset > area.Width)
                scrollOffset = caretX - area.Width;
            else if (caretX < scrollOffset)
                scrollOffset = caretX;
            scrollOffset = Math.Max(0f, Math.Min(scrollOffset, Math.Max(0f, MeasureWidth(displayText) - area.Width + 2f)));

            var controlManager = ServiceProvider.ControlManager;
            var clipped = controlManager != null && Rotation == 0f && controlManager.TryPushClip(spriteBatch, ToParentRectangle(area));
            if (controlManager != null && Rotation == 0f && !clipped)
                return; // the text area is not visible

            var lineHeight = SpriteFont.LineSpacing;
            var textPosition = new Vector2(area.X - scrollOffset, area.Y + (area.Height - lineHeight) / 2f);
            if (displayText.Length > 0)
                DrawLocalString(spriteBatch, displayText, textPosition, TextColor * opacity);
            else if (!IsFocused && Placeholder.Length > 0)
                DrawLocalString(spriteBatch, Placeholder, new Vector2(area.X, textPosition.Y), PlaceholderColor * opacity);

            if (IsFocused && caretTime % (CaretBlinkInterval * 2f) < CaretBlinkInterval)
                DrawLocalRectangle(spriteBatch, new RectangleF(textPosition.X + caretX, textPosition.Y, 2f, lineHeight), CaretColor * opacity);

            if (clipped)
                controlManager.PopState(spriteBatch);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Blur();
                onTextChanged = null;
                onSubmit = null;
                onFocusChanged = null;
            }
            base.Dispose(disposing);
        }

        private TextBox ChangeText(string value, bool notify)
        {
            caretIndex = Math.Min(caretIndex, value.Length);
            if (value == text)
                return this;
            text = value;
            caretTime = 0f;
            if (notify)
                onTextChanged?.Invoke(text);
            return this;
        }

        private void OnClicked(ControlMouseEventArgs args)
        {
            Focus();
            if (SpriteFont == null)
                return;

            // Move the caret to the character closest to the pointer.
            var x = ToLocalPosition(args.Position).X - Padding.Left + scrollOffset;
            var displayText = GetDisplayText();
            var bestIndex = 0;
            var bestDistance = float.MaxValue;
            for (var i = 0; i <= displayText.Length; i++)
            {
                var distance = Math.Abs(MeasureWidth(displayText.Substring(0, i)) - x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }
            caretIndex = bestIndex;
            caretTime = 0f;
        }

        private void OnKeyPressed(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                    caretIndex = Math.Max(0, caretIndex - 1);
                    break;
                case Keys.Right:
                    caretIndex = Math.Min(text.Length, caretIndex + 1);
                    break;
                case Keys.Home:
                    caretIndex = 0;
                    break;
                case Keys.End:
                    caretIndex = text.Length;
                    break;
                case Keys.Delete:
                    if (caretIndex < text.Length)
                        ChangeText(text.Remove(caretIndex, 1), true);
                    break;
                default:
                    return;
            }
            caretTime = 0f;
        }

        private void OnUpdate(GameTime gameTime)
        {
            if (!IsFocused)
                return;

            caretTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            var input = Input;
            if (input == null)
                return;

            // Another control took the keyboard, or the pointer was pressed somewhere else.
            if (!ReferenceEquals(input.FocusedControl, this) || (PointerPressedThisFrame(input) && !IsMouseHover))
                Blur();
        }

        private static bool PointerPressedThisFrame(IInputManager input)
        {
            if (input.Mouse.WasButtonPressed(MouseButtons.Left))
                return true;
            foreach (var touch in input.Touch.Touches)
            {
                if (touch.State == TouchLocationState.Pressed)
                    return true;
            }
            return false;
        }

        private string GetDisplayText() => IsPassword ? new string(PasswordCharacter, text.Length) : text;

        private float MeasureWidth(string value) => value.Length == 0 || SpriteFont == null ? 0f : SpriteFont.MeasureString(value).X;

        private void DrawLocalString(SpriteBatch spriteBatch, string value, Vector2 localPosition, Color color)
            => spriteBatch.DrawString(SpriteFont, value, GetPosition(), color, Rotation, OriginWithoutScale - localPosition, NestedScale, SpriteEffects.None, LayerDepthDraw);

        private Rectangle ToParentRectangle(RectangleF localArea)
        {
            var topLeft = LocalToParentPosition(localArea.Position);
            var bottomRight = LocalToParentPosition(new Vector2(localArea.Right, localArea.Bottom));
            var min = Vector2.Min(topLeft, bottomRight);
            var max = Vector2.Max(topLeft, bottomRight);
            return new Rectangle((int)Math.Floor(min.X), (int)Math.Floor(min.Y), (int)Math.Ceiling(max.X - min.X), (int)Math.Ceiling(max.Y - min.Y));
        }
    }
}
