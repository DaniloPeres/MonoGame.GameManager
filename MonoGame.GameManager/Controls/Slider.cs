using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Chooses a value between a minimum and a maximum by dragging a thumb along a track (volume, sensitivity...).
    /// Pressing anywhere on the slider moves the thumb there; the drag continues outside of the slider.
    /// </summary>
    /// <example>
    /// <code>
    /// new Slider(new Vector2(20, 20), new Vector2(240, 24))
    ///     .SetValue(audio.MusicVolume, notify: false)
    ///     .AddOnValueChanged(volume => audio.MusicVolume = volume)
    ///     .AddToScreen();
    /// </code>
    /// </example>
    public class Slider : ScalableControlAbstract<Slider>
    {
        private Action<float> onValueChanged;
        private float minimum;
        private float maximum;
        private float value;
        private float step;
        private bool isDragging;

        public Slider(Vector2 position, Vector2 size, float minimum = 0f, float maximum = 1f)
        {
            PositionAnchor = position;
            Size = size;
            this.minimum = minimum;
            this.maximum = Math.Max(minimum, maximum);
            value = this.minimum;
            ThumbSize = new Vector2(Math.Min(size.X, size.Y));
            TrackThickness = Math.Max(2f, Math.Min(size.X, size.Y) * 0.3f);

            AddOnMousePressed(OnPressed);
            AddOnMouseMoved(OnMoved);
            AddOnMouseReleased(OnReleased);
        }

        public float Minimum => minimum;

        public float Maximum => maximum;

        /// <summary>The value, between <see cref="Minimum"/> and <see cref="Maximum"/>.</summary>
        public float Value => value;

        /// <summary>The value as a rate from 0 (minimum) to 1 (maximum).</summary>
        public float NormalizedValue => maximum > minimum ? (value - minimum) / (maximum - minimum) : 0f;

        /// <summary>The value is rounded to a multiple of the step (0 = no rounding).</summary>
        public float Step => step;

        /// <summary>Horizontal (minimum on the left) or vertical (minimum at the bottom).</summary>
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        /// <summary>True while the thumb is dragged.</summary>
        public bool IsDragging => isDragging;

        public Color TrackColor { get; set; } = new Color(70, 70, 70);

        /// <summary>The color of the part of the track before the thumb.</summary>
        public Color FillColor { get; set; } = new Color(80, 160, 230);

        public Color ThumbColor { get; set; } = Color.White;

        /// <summary>The size of the thumb, in local units.</summary>
        public Vector2 ThumbSize { get; set; }

        /// <summary>The thickness of the track, in local units.</summary>
        public float TrackThickness { get; set; }

        /// <summary>When there is no thumb texture, the thumb is a circle (true) or a rectangle.</summary>
        public bool IsThumbRound { get; set; } = true;

        public Texture2D ThumbTexture { get; set; }

        public Texture2D TrackTexture { get; set; }

        /// <summary>Changes the value (clamped and rounded to the step).</summary>
        public Slider SetValue(float value, bool notify = true)
        {
            var newValue = Normalize(value);
            if (newValue == this.value)
                return this;
            this.value = newValue;
            if (notify)
                onValueChanged?.Invoke(newValue);
            return this;
        }

        public Slider SetRange(float minimum, float maximum)
        {
            this.minimum = minimum;
            this.maximum = Math.Max(minimum, maximum);
            return SetValue(value);
        }

        public Slider SetStep(float step)
        {
            this.step = Math.Max(0f, step);
            return SetValue(value);
        }

        public Slider SetOrientation(Orientation orientation)
        {
            Orientation = orientation;
            return this;
        }

        public Slider SetColors(Color track, Color fill, Color thumb)
        {
            TrackColor = track;
            FillColor = fill;
            ThumbColor = thumb;
            return this;
        }

        public Slider SetThumbSize(Vector2 thumbSize)
        {
            ThumbSize = thumbSize;
            return this;
        }

        public Slider SetThumbTexture(Texture2D thumbTexture)
        {
            ThumbTexture = thumbTexture;
            return this;
        }

        public Slider SetTrackTexture(Texture2D trackTexture)
        {
            TrackTexture = trackTexture;
            return this;
        }

        public Slider SetTrackThickness(float trackThickness)
        {
            TrackThickness = trackThickness;
            return this;
        }

        /// <summary>Adds a callback invoked with the new value when it changes.</summary>
        public Slider AddOnValueChanged(Action<float> onValueChanged)
        {
            this.onValueChanged += onValueChanged;
            return this;
        }

        public Slider RemoveOnValueChanged(Action<float> onValueChanged)
        {
            this.onValueChanged -= onValueChanged;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var size = SizeWithoutScale;
            var opacity = NestedOpacity * (IsEnabled ? 1f : 0.5f);
            var rate = NormalizedValue;
            var horizontal = Orientation == Orientation.Horizontal;

            var track = horizontal
                ? new RectangleF(0f, (size.Y - TrackThickness) / 2f, size.X, TrackThickness)
                : new RectangleF((size.X - TrackThickness) / 2f, 0f, TrackThickness, size.Y);
            var fill = horizontal
                ? new RectangleF(track.X, track.Y, size.X * rate, track.Height)
                : new RectangleF(track.X, size.Y * (1f - rate), track.Width, size.Y * rate);

            if (TrackTexture != null)
                DrawLocalTexture(spriteBatch, TrackTexture, null, track, Color * opacity);
            else
                DrawLocalRectangle(spriteBatch, track, TrackColor * opacity);
            DrawLocalRectangle(spriteBatch, fill, FillColor * opacity);

            var thumbCenter = horizontal ? new Vector2(size.X * rate, size.Y / 2f) : new Vector2(size.X / 2f, size.Y * (1f - rate));
            var thumbArea = new RectangleF(thumbCenter - ThumbSize / 2f, ThumbSize);
            if (ThumbTexture != null)
            {
                DrawLocalTexture(spriteBatch, ThumbTexture, null, thumbArea, Color * opacity);
            }
            else if (IsThumbRound)
            {
                var radius = Math.Min(ThumbSize.X, ThumbSize.Y) / 2f * Math.Abs(NestedScale.X);
                spriteBatch.FillCircle(LocalToParentPosition(thumbCenter), radius, ThumbColor * opacity);
            }
            else
            {
                DrawLocalRectangle(spriteBatch, thumbArea, ThumbColor * opacity);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                onValueChanged = null;
            base.Dispose(disposing);
        }

        private void OnPressed(ControlMouseEventArgs args)
        {
            isDragging = true;
            args.CapturePointer();
            SetValueFromPointer(args.Position);
        }

        private void OnMoved(ControlMouseEventArgs args)
        {
            if (!isDragging || !IsMousePressed)
            {
                isDragging = false;
                args.ContinuePropagation();
                return;
            }
            SetValueFromPointer(args.Position);
        }

        private void OnReleased(ControlMouseEventArgs args)
        {
            if (!isDragging)
            {
                args.ContinuePropagation();
                return;
            }
            isDragging = false;
        }

        private void SetValueFromPointer(Point position)
        {
            var local = ToLocalPosition(position);
            var size = SizeWithoutScale;
            var rate = Orientation == Orientation.Horizontal
                ? (size.X > 0f ? local.X / size.X : 0f)
                : (size.Y > 0f ? 1f - local.Y / size.Y : 0f);
            SetValue(minimum + MathHelper.Clamp(rate, 0f, 1f) * (maximum - minimum));
        }

        private float Normalize(float newValue)
        {
            newValue = MathHelper.Clamp(newValue, minimum, maximum);
            if (step > 0f)
                newValue = MathHelper.Clamp(minimum + (float)Math.Round((newValue - minimum) / step) * step, minimum, maximum);
            return newValue;
        }
    }
}
