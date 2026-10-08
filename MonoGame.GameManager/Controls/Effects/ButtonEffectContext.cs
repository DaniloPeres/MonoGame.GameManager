using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// What a <see cref="ButtonEffect"/> knows about its button when it updates and draws: the shape (in the local,
    /// unscaled space of the button), the visual state, the time and the opacity. One instance is reused by each button.
    /// </summary>
    public sealed class ButtonEffectContext
    {
        private float perimeterLength = -1f;
        private float requestedCornerRadius = -1f;

        /// <summary>The shape of the effects in local units: the area of the button, reduced by its effect padding.</summary>
        public RectangleF Bounds { get; private set; }

        /// <summary>The corner radius of the shape, limited to half of its smallest side.</summary>
        public float CornerRadius { get; private set; }

        /// <summary>The visual state of the button.</summary>
        public ButtonVisualState State { get; internal set; }

        /// <summary>The time of the effects of the button, in seconds (it starts at a random value for each button).</summary>
        public float Time { get; internal set; }

        /// <summary>The duration of the current frame, in seconds (0 while drawing).</summary>
        public float DeltaSeconds { get; internal set; }

        /// <summary>The opacity of the button (with the opacity of its parents).</summary>
        public float Opacity { get; internal set; } = 1f;

        /// <summary>The scale of the button on the screen (with the scale of its parents).</summary>
        public Vector2 Scale { get; internal set; } = Vector2.One;

        /// <summary>The random generator of the game.</summary>
        public IRandom Random { get; internal set; }

        /// <summary>The button.</summary>
        public IControl Host { get; internal set; }

        /// <summary>The rotation of the button, in radians.</summary>
        public float Rotation { get; internal set; }

        /// <summary>The label created by the <c>SetText</c> method of the button, or null.</summary>
        public Label TextLabel => HostInternal?.TextLabel;

        /// <summary>The background color of the button in its current state, or null when it is drawn with a texture.</summary>
        public Color? BackgroundColor => HostInternal?.EffectBackgroundColor;

        /// <summary>
        /// A darker (<paramref name="amount"/> above 0) or lighter (below 0) version of the background color of the
        /// button, or <paramref name="fallback"/> when it has no background color.
        /// </summary>
        public Color GetShadeOfBackground(float amount, Color fallback)
        {
            var background = BackgroundColor;
            if (!background.HasValue)
                return fallback;

            var color = background.Value;
            return amount >= 0f
                ? Color.Lerp(color, new Color(0, 0, 0, (int)color.A), Math.Min(1f, amount))
                : Color.Lerp(color, new Color((int)color.A, (int)color.A, (int)color.A, (int)color.A), Math.Min(1f, -amount));
        }

        internal IButtonEffectHost HostInternal { get; set; }

        /// <summary>
        /// Converts a point of the screen space of the controls (eg: the position of a child) to the local space of the
        /// button.
        /// </summary>
        public Vector2 ToLocalPosition(Vector2 point) => HostInternal != null ? HostInternal.ToLocalPosition(point) : point;

        /// <summary>
        /// The resolution of the generated textures: 1 when the button is drawn at its size, more when it is scaled up,
        /// so the lights stay sharp.
        /// </summary>
        public int TextureDensity => ButtonEffectResources.GetDensity(Scale);

        /// <summary>The length of the outline of the shape.</summary>
        public float PerimeterLength
        {
            get
            {
                if (perimeterLength < 0f)
                    perimeterLength = RoundedRectanglePath.PerimeterLength(Bounds, CornerRadius);
                return perimeterLength;
            }
        }

        /// <summary>The point of the outline of the shape at a distance from its start (see <see cref="RoundedRectanglePath"/>).</summary>
        public Vector2 PointOnBorder(float distance, out Vector2 outwardNormal)
            => RoundedRectanglePath.PointAt(Bounds, CornerRadius, distance, out outwardNormal);

        /// <summary>A remarkable point of the outline of the shape: the middle of an edge or of a corner, or the center.</summary>
        public Vector2 AnchorPoint(Anchor anchor, out Vector2 outwardNormal)
            => RoundedRectanglePath.AnchorPoint(Bounds, CornerRadius, anchor, out outwardNormal);

        internal void SetShape(RectangleF bounds, float cornerRadius)
        {
            if (bounds == Bounds && cornerRadius == requestedCornerRadius)
                return;
            Bounds = bounds;
            requestedCornerRadius = cornerRadius;
            CornerRadius = RoundedRectanglePath.ClampRadius(bounds, cornerRadius);
            perimeterLength = -1f;
        }
    }
}
