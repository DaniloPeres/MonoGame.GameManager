using System;

namespace MonoGame.GameManager.Controls.Sprites
{
    /// <summary>
    /// A named sequence of frames (eg: "idle", "run", "jump").
    /// </summary>
    public class SpriteAnimationCycle
    {
        public const string DefaultCycleName = "default";

        public SpriteAnimationCycle(string name, SpriteAnimationFrame[] frames)
        {
            Name = string.IsNullOrEmpty(name) ? DefaultCycleName : name;
            Frames = frames ?? throw new ArgumentNullException(nameof(frames));
            if (frames.Length == 0)
                throw new ArgumentException($"The cycle '{Name}' has no frames.", nameof(frames));
        }

        public string Name { get; set; }

        public SpriteAnimationFrame[] Frames { get; set; }
    }
}
