using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Controls.Sprites
{
    /// <summary>
    /// A frame of a sprite animation: a texture region shown for a duration.
    /// </summary>
    public class SpriteAnimationFrame
    {
        /// <summary>The default duration of a frame (60 frames per second).</summary>
        public const float DefaultDuration = 1f / 60f;

        public Texture2D Texture { get; set; }

        public Rectangle SourceRectangle { get; set; }

        public Vector2 Margin { get; set; }

        /// <summary>
        /// How long the frame is shown, in seconds. A value of 0 (or less) means "use the default duration"
        /// (the duration of the cycle when the frame is built by a <see cref="Builders.SpriteAnimationCycleBuilder"/>).
        /// </summary>
        public float Duration { get; set; }

        /// <summary>The duration actually used to play the frame.</summary>
        public float EffectiveDuration => Duration > 0f ? Duration : DefaultDuration;
    }
}
