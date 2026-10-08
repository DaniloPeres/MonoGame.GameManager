using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>One ring of a <see cref="FrameEffect"/>: a color and a thickness.</summary>
    public class FrameLayer
    {
        public FrameLayer(Color color, float thickness)
        {
            Color = color;
            Thickness = thickness;
        }

        /// <summary>The color of the ring.</summary>
        public Color Color { get; set; }

        /// <summary>The thickness of the ring.</summary>
        public float Thickness { get; set; }

        /// <summary>
        /// When set, the ring uses the background color of the button instead of <see cref="Color"/>: darker (0 to 1)
        /// or lighter (0 to -1).
        /// </summary>
        public float? ButtonColorShade { get; set; }

        /// <summary>A ring that uses the background color of the button, darker (0 to 1) or lighter (0 to -1).</summary>
        public static FrameLayer FromButtonColor(float shade, float thickness)
            => new FrameLayer(Color.Black, thickness) { ButtonColorShade = shade };
    }
}
