using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Layout;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Timers;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Shows a short text near a control while the pointer stays over it.
    /// </summary>
    /// <example>
    /// <code>
    /// Tooltip.Attach(saveButton, font, "Saves the game in the first slot");
    /// </code>
    /// </example>
    public class Tooltip
    {
        private readonly IControl target;
        private Panel panel;
        private Label label;
        private ScheduledAction pendingShow;

        private Tooltip(IControl target, SpriteFontBase font, string text, float delaySeconds)
        {
            this.target = target;
            Font = font;
            Text = text ?? string.Empty;
            DelaySeconds = delaySeconds;

            target.AddOnMouseEnter(OnTargetEnter);
            target.AddOnMouseLeave(OnTargetLeave);
            target.Disposed += OnTargetDisposed;
        }

        public SpriteFontBase Font { get; set; }

        public string Text { get; set; }

        /// <summary>The time the pointer must stay over the control before the tooltip appears.</summary>
        public float DelaySeconds { get; set; }

        public Color BackgroundColor { get; set; } = new Color(20, 20, 20) * 0.92f;

        public Color TextColor { get; set; } = Color.White;

        public Color BorderColor { get; set; } = new Color(90, 90, 90);

        public Thickness Padding { get; set; } = new Thickness(8f, 4f);

        /// <summary>The scale of the text.</summary>
        public float TextScale { get; set; } = 1f;

        /// <summary>True while the tooltip is shown.</summary>
        public bool IsVisible => panel != null && !panel.IsDisposed;

        /// <summary>Shows <paramref name="text"/> near <paramref name="target"/> while the pointer stays over it.</summary>
        public static Tooltip Attach(IControl target, SpriteFontBase font, string text, float delaySeconds = 0.5f)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (font == null)
                throw new ArgumentNullException(nameof(font));
            return new Tooltip(target, font, text, delaySeconds);
        }

        public Tooltip SetStyle(Color background, Color text, Color border)
        {
            BackgroundColor = background;
            TextColor = text;
            BorderColor = border;
            return this;
        }

        /// <summary>Removes the tooltip from the control.</summary>
        public void Detach()
        {
            Hide();
            target.RemoveOnMouseEnter(OnTargetEnter);
            target.RemoveOnMouseLeave(OnTargetLeave);
            target.Disposed -= OnTargetDisposed;
        }

        /// <summary>Shows the tooltip now, below the control (or above it near the bottom of the screen).</summary>
        public void Show()
        {
            pendingShow?.Cancel();
            pendingShow = null;
            Hide();

            var root = ServiceProvider.RootPanel;
            if (root == null || target.IsDisposed || string.IsNullOrEmpty(Text))
                return;

            label = new Label(Font, Text, new Vector2(Padding.Left, Padding.Top), TextColor).SetScale(TextScale);
            var size = label.Size + new Vector2(Padding.Horizontal, Padding.Vertical);
            panel = new Panel(GetPosition(size, root), size)
                .SetBackgroundColor(BackgroundColor)
                .SetBorder(BorderColor)
                .SetZIndex(ZIndexLayers.Overlay);
            panel.AddChild(label);
            root.AddChild(panel);
        }

        public void Hide()
        {
            pendingShow?.Cancel();
            pendingShow = null;
            panel?.Dispose();
            panel = null;
            label = null;
        }

        private Vector2 GetPosition(Vector2 size, IContainer root)
        {
            const float gap = 4f;
            var bounds = target.DestinationRectangle;
            var origin = target.Origin;
            var screen = root.DestinationRectangle;
            var x = bounds.X - origin.X;
            var y = bounds.Y - origin.Y + bounds.Height + gap;
            if (y + size.Y > screen.Bottom)
                y = bounds.Y - origin.Y - gap - size.Y;
            x = MathHelper.Clamp(x, screen.Left, Math.Max(screen.Left, screen.Right - size.X));
            y = MathHelper.Clamp(y, screen.Top, Math.Max(screen.Top, screen.Bottom - size.Y));
            return new Vector2(x - screen.X, y - screen.Y);
        }

        private void OnTargetEnter(ControlMouseEventArgs args)
        {
            args.ContinuePropagation();
            pendingShow?.Cancel();
            if (DelaySeconds <= 0f)
            {
                Show();
                return;
            }
            pendingShow = ServiceProvider.Scheduler.Delay(DelaySeconds, Show);
        }

        private void OnTargetLeave(ControlMouseEventArgs args)
        {
            args.ContinuePropagation();
            Hide();
        }

        private void OnTargetDisposed(IControl control) => Hide();
    }
}
