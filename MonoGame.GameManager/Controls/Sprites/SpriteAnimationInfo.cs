using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Sprites
{
    public class SpriteAnimationInfo
    {
        private List<SpriteAnimationCycle> Cycles { get; set; }
        public int CyclesCount => Cycles.Count;

        public SpriteAnimationInfo(SpriteAnimationFrame[] frames) : this(new List<SpriteAnimationCycle> { new SpriteAnimationCycle(SpriteAnimationCycle.DefaultCycleName, frames) })
        { }

        public SpriteAnimationInfo(SpriteAnimationCycle cycle) : this(new List<SpriteAnimationCycle> { cycle })
        { }

        public SpriteAnimationInfo(List<SpriteAnimationCycle> cycles)
        {
            Cycles = cycles;
        }

        public SpriteAnimationCycle GetSpriteAnimationCycle(string cycleName) => Cycles.Find(x => x.Name.Equals(cycleName));
        public SpriteAnimationCycle GetSpriteAnimationCycleByIndex(int index) => Cycles[index];
        public int FindCycleIndex(string cycleName) => Cycles.FindIndex(x => x.Name.Equals(cycleName));
        public SpriteAnimation CreateSpriteAnimation()
        {
            return new SpriteAnimation(this);
        }
    }
}
