using System;

namespace MonoGame.GameManager.Layout
{
    /// <summary>Distances from the four edges of an area (padding or margin).</summary>
    public struct Thickness : IEquatable<Thickness>
    {
        /// <summary>The same distance on every edge.</summary>
        public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }

        /// <summary>One distance for the left and right edges and one for the top and bottom edges.</summary>
        public Thickness(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }

        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static Thickness Zero => default;

        public float Left { get; set; }

        public float Top { get; set; }

        public float Right { get; set; }

        public float Bottom { get; set; }

        /// <summary>Left + Right.</summary>
        public float Horizontal => Left + Right;

        /// <summary>Top + Bottom.</summary>
        public float Vertical => Top + Bottom;

        public static bool operator ==(Thickness a, Thickness b) => a.Equals(b);

        public static bool operator !=(Thickness a, Thickness b) => !a.Equals(b);

        public bool Equals(Thickness other) => Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

        public override bool Equals(object obj) => obj is Thickness other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Left.GetHashCode();
                hash = hash * 397 ^ Top.GetHashCode();
                hash = hash * 397 ^ Right.GetHashCode();
                return hash * 397 ^ Bottom.GetHashCode();
            }
        }

        public override string ToString() => $"{{Left:{Left} Top:{Top} Right:{Right} Bottom:{Bottom}}}";
    }
}
