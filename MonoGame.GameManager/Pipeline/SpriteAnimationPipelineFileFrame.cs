using Microsoft.Xna.Framework;
using MonoGame.GameManager.Converters;
using Newtonsoft.Json;

namespace MonoGame.GameManager.Pipeline
{
    public class SpriteAnimationPipelineFileFrame
    {
        public string Texture { get; set; }
        [JsonConverter(typeof(RectangleConverter))]
        public Rectangle? SourceRectangle { get; set; }
        public float FrameDuration { get; set; }
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Margin { get; set; }
    }
}
