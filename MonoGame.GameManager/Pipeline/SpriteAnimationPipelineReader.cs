using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Builders;
using MonoGame.GameManager.Controls.Sprites;
using MonoGame.GameManager.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoGame.GameManager.Pipeline
{
    public class SpriteAnimationPipelineReader
    {
        private string assetName;
        private Dictionary<string, Texture2D> texturesMemoryCache = new Dictionary<string, Texture2D>();

        public SpriteAnimationPipelineReader(string assetName)
        {
            this.assetName = assetName;
        }

        public SpriteAnimationInfo Read()
        {
            using (var stream = ServiceProvider.ContentLoaderManager.GetContentFileStream(assetName))
            {
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var spriteAnimationPipelineFile = JsonConvert.DeserializeObject<SpriteAnimationPipelineFile>(json);
                    var fileCycles = PrepareSpriteAnimationPipelineFileCycles(spriteAnimationPipelineFile);
                    return CreateSpriteAnimationInfo(fileCycles);
                }
            }
        }

        private Dictionary<string, SpriteAnimationPipelineFileCycle> PrepareSpriteAnimationPipelineFileCycles(SpriteAnimationPipelineFile spriteAnimationPipelineFile)
        {
            var cycles = spriteAnimationPipelineFile.Cycles ?? new Dictionary<string, SpriteAnimationPipelineFileCycle>();

            cycles.Values.ToList().ForEach(cycle =>
            {
                // apply the properties from Pipeline File into Cycles in case the cycle has empty values
                if (string.IsNullOrEmpty(cycle.Texture))
                    cycle.Texture = spriteAnimationPipelineFile.Texture;
                if (string.IsNullOrEmpty(cycle.Textures))
                    cycle.Textures = spriteAnimationPipelineFile.Textures;
                if (cycle.Size == default)
                    cycle.Size = spriteAnimationPipelineFile.Size;
                if (cycle.Position == default)
                    cycle.Position = spriteAnimationPipelineFile.Position;
                if (cycle.FrameCount == default)
                    cycle.FrameCount = spriteAnimationPipelineFile.FrameCount;
                if (cycle.FrameCountRow == default)
                    cycle.FrameCountRow = spriteAnimationPipelineFile.FrameCountRow;
                if (cycle.FrameDuration == default)
                    cycle.FrameDuration = spriteAnimationPipelineFile.FrameDuration;
                if (cycle.Margin == default)
                    cycle.Margin = spriteAnimationPipelineFile.Margin;
            });

            if (!cycles.Any())
            {
                cycles = new Dictionary<string, SpriteAnimationPipelineFileCycle>()
                {
                    { SpriteAnimationCycle.DefaultCycleName, spriteAnimationPipelineFile }
                };
            }

            return cycles;
        }

        private SpriteAnimationInfo CreateSpriteAnimationInfo(Dictionary<string, SpriteAnimationPipelineFileCycle> fileCycles)
        {
            // use some memory cache to not re-load multiple times the same texture
            var cycles = fileCycles.Keys.ToList().Select(cycleName =>
            {
                var fileCycle = fileCycles[cycleName];

                var builder = new SpriteAnimationCycleBuilder()
                    .WithName(cycleName)
                    .WithSize(fileCycle.Size)
                    .WithTexturePosition(fileCycle.Position)
                    .WithTotalOfFrames(fileCycle.FrameCount)
                    .WithFramesCountOnRow(fileCycle.FrameCountRow)
                    .WithFrameDuration(fileCycle.FrameDuration)
                    .WithMargin(fileCycle.Margin)
                    .WithMultipleTexturesStartingCount(fileCycle.MultipleTexturesStartingCount);
                if (!string.IsNullOrEmpty(fileCycle.Texture))
                    builder.WithTexture(GetTexture(fileCycle.Texture));
                if (!string.IsNullOrEmpty(fileCycle.Textures))
                    builder.WithMultipleTextures(GetTextureWithPath(fileCycle.Textures));

                if (fileCycle.Frames != null)
                    builder.WithFrames(fileCycle.Frames.Select(CreateSpriteAnimationFrame).ToList());

                if (fileCycle.FramesByIndex != null)
                {
                    fileCycle.FramesByIndex.Keys.ToList().ForEach(frameIndex =>
                    {
                        builder.WithFrame(frameIndex, CreateSpriteAnimationFrame(fileCycle.FramesByIndex[frameIndex]));
                    });
                }

                return builder.Build();
            }).ToList();

            return new SpriteAnimationInfo(cycles);
        }

        private SpriteAnimationFrame CreateSpriteAnimationFrame(SpriteAnimationPipelineFileFrame fileFrame)
        {
            var frame = new SpriteAnimationFrame
            {
                Duration = fileFrame.FrameDuration,
                Margin = fileFrame.Margin
            };

            if (fileFrame.SourceRectangle != null)
                frame.SourceRectangle = fileFrame.SourceRectangle.Value;

            if (!string.IsNullOrEmpty(fileFrame.Texture))
                frame.Texture = GetTexture(fileFrame.Texture);

            return frame;
        }

        private string GetTextureWithPath(string texture)
        {
            var pathArray = assetName.Replace("\\", "/").Split('/');
            var path = string.Join("/", pathArray.Take(pathArray.Length - 1));
            return $"{path}/{texture}";
        }

        private Texture2D LoadTexture(string textureWithPath)
            => ServiceProvider.ContentLoaderManager.LoadTexture2D(textureWithPath);

        private Texture2D GetTexture(string textureString)
        {
            if (!texturesMemoryCache.TryGetValue(textureString, out var texture))
            {
                texture = LoadTexture(GetTextureWithPath(textureString));
                texturesMemoryCache.Add(textureString, texture);
            }

            return texture;
        }
    }
}
