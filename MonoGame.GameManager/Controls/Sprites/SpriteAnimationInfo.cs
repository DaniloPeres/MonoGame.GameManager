using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.Sprites
{
    /// <summary>
    /// The cycles of a sprite animation (eg: loaded from a .sa file).
    /// </summary>
    public class SpriteAnimationInfo
    {
        private readonly List<SpriteAnimationCycle> cycles;

        public SpriteAnimationInfo(SpriteAnimationFrame[] frames)
            : this(new List<SpriteAnimationCycle> { new SpriteAnimationCycle(SpriteAnimationCycle.DefaultCycleName, frames) })
        { }

        public SpriteAnimationInfo(SpriteAnimationCycle cycle) : this(new List<SpriteAnimationCycle> { cycle })
        { }

        public SpriteAnimationInfo(List<SpriteAnimationCycle> cycles)
        {
            if (cycles == null)
                throw new ArgumentNullException(nameof(cycles));
            if (cycles.Count == 0)
                throw new ArgumentException("A sprite animation needs at least one cycle.", nameof(cycles));
            this.cycles = new List<SpriteAnimationCycle>(cycles);
        }

        public int CyclesCount => cycles.Count;

        /// <summary>The names of the cycles, in order.</summary>
        public IEnumerable<string> CycleNames => cycles.Select(cycle => cycle.Name);

        /// <summary>Returns the cycle with the given name, or null.</summary>
        public SpriteAnimationCycle GetSpriteAnimationCycle(string cycleName)
            => cycles.Find(cycle => string.Equals(cycle.Name, cycleName, StringComparison.Ordinal));

        public SpriteAnimationCycle GetSpriteAnimationCycleByIndex(int index) => cycles[index];

        public int FindCycleIndex(string cycleName)
            => cycles.FindIndex(cycle => string.Equals(cycle.Name, cycleName, StringComparison.Ordinal));

        public SpriteAnimation CreateSpriteAnimation() => new SpriteAnimation(this);

        public SpriteAnimation CreateSpriteAnimation(string cycleName) => new SpriteAnimation(this, cycleName);
    }
}
