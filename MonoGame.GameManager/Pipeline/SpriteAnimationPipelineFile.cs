using System.Collections.Generic;

namespace MonoGame.GameManager.Pipeline
{
    public class SpriteAnimationPipelineFile : SpriteAnimationPipelineFileCycle
    {
        public Dictionary<string, SpriteAnimationPipelineFileCycle> Cycles { get; set; }
    }
}
