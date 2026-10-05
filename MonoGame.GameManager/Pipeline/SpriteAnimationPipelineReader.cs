using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Builders;
using MonoGame.GameManager.Controls.Sprites;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoGame.GameManager.Pipeline
{
    /// <summary>
    /// Reads a sprite animation file (.sa): a JSON file, copied to the content folder, that describes the frames
    /// and cycles of a sprite sheet or of a sequence of textures. The textures are loaded relative to the folder
    /// of the file.
    /// </summary>
    public class SpriteAnimationPipelineReader
    {
        private readonly IContentLoader contentLoader;
        private readonly string assetName;
        private readonly Dictionary<string, Texture2D> texturesMemoryCache = new Dictionary<string, Texture2D>();

        public SpriteAnimationPipelineReader(string assetName) : this(ServiceProvider.ContentLoader, assetName) { }

        public SpriteAnimationPipelineReader(IContentLoader contentLoader, string assetName)
        {
            this.contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            this.assetName = assetName ?? throw new ArgumentNullException(nameof(assetName));
        }

        public SpriteAnimationInfo Read()
        {
            SpriteAnimationPipelineFile spriteAnimationPipelineFile;
            using (var stream = contentLoader.OpenStream(assetName))
            using (var reader = new StreamReader(stream))
            {
                try
                {
                    spriteAnimationPipelineFile = JsonConvert.DeserializeObject<SpriteAnimationPipelineFile>(reader.ReadToEnd());
                }
                catch (JsonException exception)
                {
                    throw new InvalidDataException($"The sprite animation file '{assetName}' is not valid: {exception.Message}", exception);
                }
            }

            if (spriteAnimationPipelineFile == null)
                throw new InvalidDataException($"The sprite animation file '{assetName}' is empty.");

            var fileCycles = PrepareSpriteAnimationPipelineFileCycles(spriteAnimationPipelineFile);
            return CreateSpriteAnimationInfo(fileCycles);
        }

        private static Dictionary<string, SpriteAnimationPipelineFileCycle> PrepareSpriteAnimationPipelineFileCycles(SpriteAnimationPipelineFile file)
        {
            var cycles = file.Cycles ?? new Dictionary<string, SpriteAnimationPipelineFileCycle>();

            foreach (var cycle in cycles.Values)
            {
                // The values of the file are the defaults of every cycle.
                if (string.IsNullOrEmpty(cycle.Texture))
                    cycle.Texture = file.Texture;
                if (string.IsNullOrEmpty(cycle.Textures))
                    cycle.Textures = file.Textures;
                if (cycle.Size == default)
                    cycle.Size = file.Size;
                if (cycle.Position == default)
                    cycle.Position = file.Position;
                if (cycle.FrameCount == default)
                    cycle.FrameCount = file.FrameCount;
                if (cycle.FrameCountRow == default)
                    cycle.FrameCountRow = file.FrameCountRow;
                if (cycle.FrameDuration == default)
                    cycle.FrameDuration = file.FrameDuration;
                if (cycle.Margin == default)
                    cycle.Margin = file.Margin;
                if (cycle.MultipleTexturesStartingCount == default)
                    cycle.MultipleTexturesStartingCount = file.MultipleTexturesStartingCount;
                if (cycle.Frames == null)
                    cycle.Frames = file.Frames;
                if (cycle.FramesByIndex == null)
                    cycle.FramesByIndex = file.FramesByIndex;
            }

            if (!cycles.Any())
            {
                cycles = new Dictionary<string, SpriteAnimationPipelineFileCycle>
                {
                    { SpriteAnimationCycle.DefaultCycleName, file }
                };
            }

            return cycles;
        }

        private SpriteAnimationInfo CreateSpriteAnimationInfo(Dictionary<string, SpriteAnimationPipelineFileCycle> fileCycles)
        {
            var cycles = fileCycles.Select(pair =>
            {
                var cycleName = pair.Key;
                var fileCycle = pair.Value;

                var builder = new SpriteAnimationCycleBuilder()
                    .WithContentLoader(contentLoader)
                    .WithName(cycleName)
                    .WithSize(fileCycle.Size)
                    .WithTexturePosition(fileCycle.Position)
                    .WithTotalOfFrames(fileCycle.FrameCount)
                    .WithFramesCountOnRow(fileCycle.FrameCountRow)
                    .WithMargin(fileCycle.Margin)
                    .WithMultipleTexturesStartingCount(fileCycle.MultipleTexturesStartingCount);

                if (fileCycle.FrameDuration > 0f)
                    builder.WithFrameDuration(fileCycle.FrameDuration);
                if (!string.IsNullOrEmpty(fileCycle.Texture))
                    builder.WithTexture(GetTexture(fileCycle.Texture));
                if (!string.IsNullOrEmpty(fileCycle.Textures))
                    builder.WithMultipleTextures(GetTextureWithPath(fileCycle.Textures));

                if (fileCycle.Frames != null)
                    builder.WithFrames(fileCycle.Frames.Select(CreateSpriteAnimationFrame).ToList());

                if (fileCycle.FramesByIndex != null)
                {
                    foreach (var frame in fileCycle.FramesByIndex)
                        builder.WithFrame(frame.Key, CreateSpriteAnimationFrame(frame.Value));
                }

                try
                {
                    return builder.Build();
                }
                catch (InvalidOperationException exception)
                {
                    throw new InvalidDataException($"The cycle '{cycleName}' of the sprite animation file '{assetName}' is not valid: {exception.Message}", exception);
                }
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

        /// <summary>Resolves a texture name relative to the folder of the .sa file.</summary>
        private string GetTextureWithPath(string texture)
        {
            var normalizedAssetName = assetName.Replace('\\', '/');
            var separatorIndex = normalizedAssetName.LastIndexOf('/');
            return separatorIndex < 0 ? texture : normalizedAssetName.Substring(0, separatorIndex + 1) + texture;
        }

        private Texture2D GetTexture(string textureString)
        {
            if (!texturesMemoryCache.TryGetValue(textureString, out var texture))
            {
                texture = contentLoader.LoadTexture2D(GetTextureWithPath(textureString));
                texturesMemoryCache.Add(textureString, texture);
            }

            return texture;
        }
    }
}
