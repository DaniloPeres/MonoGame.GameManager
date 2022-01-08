using Microsoft.Xna.Framework;
using MonoGame.GameManager.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonoGame.GameManager.Pipeline
{
    public class SpriteAnimationPipelineFileCycle
    {
        public string Texture { get; set; }
        public string Textures { get; set; }
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Size { get; set; }
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Position { get; set; }
        public int FrameCount { get; set; }
        public int FrameCountRow { get; set; }
        public float FrameDuration { get; set; }
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Margin { get; set; }
        public int MultipleTexturesStartingCount { get; set; }
        public List<SpriteAnimationPipelineFileFrame> Frames { get; set; }
        public Dictionary<int, SpriteAnimationPipelineFileFrame> FramesByIndex { get; set; }
    }
}
