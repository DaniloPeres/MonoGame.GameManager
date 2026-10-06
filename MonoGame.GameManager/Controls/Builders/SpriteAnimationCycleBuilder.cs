using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Sprites;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.Builders
{
    /// <summary>
    /// Builds a <see cref="SpriteAnimationCycle"/> from a sprite sheet (frames in a grid) or from a sequence of
    /// textures (Builder pattern).
    /// </summary>
    /// <example>
    /// <code>
    /// var walk = new SpriteAnimationCycleBuilder()
    ///     .WithName("walk")
    ///     .WithTexture(characterSheet)
    ///     .WithSize(32, 32)
    ///     .WithTexturePosition(0, 64)
    ///     .WithTotalOfFrames(6)
    ///     .WithFrameDuration(0.1f)
    ///     .Build();
    /// </code>
    /// </example>
    public class SpriteAnimationCycleBuilder
    {
        private const float DefaultFrameDuration = SpriteAnimationFrame.DefaultDuration;

        private IContentLoader contentLoader;
        private string cycleName;
        private string texturePath;
        private Texture2D texture;
        private Texture2D[] textures;
        private string texturesPathFormat;
        private Vector2 size;
        private Vector2 position;
        private int frameCount;
        private int frameCountRow;
        private int multipleTexturesStartingCount;
        private float frameDuration = DefaultFrameDuration;
        private Vector2 margin;
        private Dictionary<int, SpriteAnimationFrame> frames;

        /// <summary>The loader used to load textures by path (defaults to <see cref="ServiceProvider.ContentLoader"/>).</summary>
        public SpriteAnimationCycleBuilder WithContentLoader(IContentLoader contentLoader)
        {
            this.contentLoader = contentLoader;
            return this;
        }

        public SpriteAnimationCycleBuilder WithName(string cycleName)
        {
            this.cycleName = cycleName;
            return this;
        }

        public SpriteAnimationCycleBuilder WithTexture(string texturePath)
        {
            this.texturePath = texturePath;
            return this;
        }

        public SpriteAnimationCycleBuilder WithTexture(Texture2D texture)
        {
            this.texture = texture;
            return this;
        }

        /// <summary>A numbered sequence of textures, eg: "Run ({0})" (see <see cref="WithMultipleTexturesStartingCount"/>).</summary>
        public SpriteAnimationCycleBuilder WithMultipleTextures(string texturesPathFormat)
        {
            this.texturesPathFormat = texturesPathFormat;
            return this;
        }

        public SpriteAnimationCycleBuilder WithMultipleTextures(Texture2D[] textures)
        {
            this.textures = textures;
            return this;
        }

        /// <summary>
        /// Set the size of the sprite.
        /// If you don't set the size, the builder will calculate it from the texture and the number of frames.
        /// </summary>
        /// <param name="width">The width of the sprite</param>
        /// <param name="height">The height of the sprite</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithSize(float width, float height) => WithSize(new Vector2(width, height));

        /// <summary>
        /// Set the size of the sprite.
        /// If you don't set the size, the builder will calculate it from the texture and the number of frames.
        /// </summary>
        /// <param name="size">The sprite size</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithSize(Vector2 size)
        {
            this.size = size;
            return this;
        }

        /// <summary>
        /// Set the position of the sprite in the texture.
        /// </summary>
        /// <param name="left">The left position of the sprite in the texture</param>
        /// <param name="top">The top position of the sprite in the texture</param>
        /// <returns>The Builder</returns>
        public SpriteAnimationCycleBuilder WithTexturePosition(float left, float top) => WithTexturePosition(new Vector2(left, top));

        /// <summary>
        /// Set the position of the sprite in the texture.
        /// </summary>
        /// <param name="position">The position of the sprite in the texture</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithTexturePosition(Vector2 position)
        {
            this.position = position;
            return this;
        }

        /// <summary>
        /// Set the total of sprite frames to extract from the texture.
        /// If you don't set it, it is calculated from the frames, the textures or the size of the sprite.
        /// </summary>
        /// <param name="frameCount">The total of frames from the sprite</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithTotalOfFrames(int frameCount)
        {
            this.frameCount = frameCount;
            return this;
        }

        /// <summary>Sets or replaces the frame at an index (values that are not set use the cycle defaults).</summary>
        public SpriteAnimationCycleBuilder WithFrame(int frameIndex, SpriteAnimationFrame frame)
        {
            if (frameIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(frameIndex));
            if (frames == null)
                frames = new Dictionary<int, SpriteAnimationFrame>();
            frames[frameIndex] = frame ?? throw new ArgumentNullException(nameof(frame));
            return this;
        }

        public SpriteAnimationCycleBuilder WithFrames(List<SpriteAnimationFrame> frames)
        {
            for (var i = 0; i < frames.Count; i++)
                WithFrame(i, frames[i]);
            return this;
        }

        public SpriteAnimationCycleBuilder WithFramesCountOnRow(int frameCountRow)
        {
            this.frameCountRow = frameCountRow;
            return this;
        }

        /// <summary>The duration of the frames, in seconds (frames can override it).</summary>
        public SpriteAnimationCycleBuilder WithFrameDuration(float frameDuration)
        {
            this.frameDuration = frameDuration > 0f ? frameDuration : DefaultFrameDuration;
            return this;
        }

        public SpriteAnimationCycleBuilder WithMargin(Vector2 margin)
        {
            this.margin = margin;
            return this;
        }

        public SpriteAnimationCycleBuilder WithMultipleTexturesStartingCount(int multipleTexturesStartingCount)
        {
            this.multipleTexturesStartingCount = multipleTexturesStartingCount;
            return this;
        }

        public SpriteAnimationCycle Build()
        {
            return new SpriteAnimationCycle(GetCycleNameToUse(), GenerateFrames());
        }

        private IContentLoader ContentLoader => contentLoader ?? ServiceProvider.ContentLoader;

        private string GetCycleNameToUse()
            => string.IsNullOrEmpty(cycleName)
                ? SpriteAnimationCycle.DefaultCycleName
                : cycleName;

        private SpriteAnimationFrame[] GenerateFrames()
        {
            var cycleTextures = GetTextures();
            var totalFrames = GetFrameCount(cycleTextures);
            var frameSize = GetSize(cycleTextures, totalFrames);

            var generatedFrames = new SpriteAnimationFrame[totalFrames];
            for (var i = 0; i < totalFrames; i++)
                generatedFrames[i] = CreateSpriteAnimationFrame(cycleTextures, frameSize, totalFrames, i);

            return generatedFrames;
        }

        private Texture2D[] GetTextures()
        {
            if (texture != null)
                return new[] { texture };
            if (textures != null && textures.Length > 0)
                return textures;
            if (!string.IsNullOrEmpty(texturePath))
                return new[] { ContentLoader.LoadTexture2D(texturePath) };
            if (!string.IsNullOrEmpty(texturesPathFormat))
            {
                if (frameCount <= 0)
                    throw new InvalidOperationException("Set the total of frames to load a sequence of textures.");
                return ContentLoader.LoadMultipleTextures(texturesPathFormat, frameCount, multipleTexturesStartingCount);
            }

            throw new InvalidOperationException("The texture was not set: set the texture, the texture path or the textures.");
        }

        private int GetFrameCount(Texture2D[] cycleTextures)
        {
            if (frameCount > 0)
                return frameCount;
            if (frames != null && frames.Count > 0)
                return frames.Keys.Max() + 1;
            if (cycleTextures.Length > 1)
                return cycleTextures.Length;
            if (size.X > 0f && size.Y > 0f)
            {
                // Every frame of the sheet, from the start position.
                var available = cycleTextures[0].Bounds.Size.ToVector2() - position;
                var columns = Math.Max(1, (int)(available.X / size.X));
                var rows = Math.Max(1, (int)(available.Y / size.Y));
                return frameCountRow > 0 ? Math.Min(columns, frameCountRow) * rows : columns * rows;
            }
            return 1;
        }

        private Vector2 GetSize(Texture2D[] cycleTextures, int totalFrames)
        {
            if (size != default)
                return size;

            var textureSize = cycleTextures[0].Bounds.Size.ToVector2();

            if (cycleTextures.Length > 1)
                return textureSize;

            var totalFramesPerRow = GetTotalFramesPerRow(totalFrames);
            return new Vector2(textureSize.X / totalFramesPerRow, textureSize.Y / (float)Math.Ceiling(totalFrames / (double)totalFramesPerRow));
        }

        private SpriteAnimationFrame CreateSpriteAnimationFrame(Texture2D[] cycleTextures, Vector2 frameSize, int totalFrames, int frameIndex)
        {
            var textureIndex = Math.Min(frameIndex, cycleTextures.Length - 1);
            var frameTexture = cycleTextures[textureIndex];

            var framePosition = position;
            if (cycleTextures.Length == 1)
            {
                // A single sprite sheet: the frames are in a grid.
                var totalFramesPerRow = GetTotalFramesPerRow(totalFrames);
                var gridPosition = new Vector2(frameIndex % totalFramesPerRow, frameIndex / totalFramesPerRow);
                framePosition += gridPosition * frameSize;
            }

            var sourceRectangle = new Rectangle(framePosition.ToPoint(), frameSize.ToPoint());

            if (frames != null && frames.TryGetValue(frameIndex, out var frame))
            {
                // Copy the frame so the instance given by the caller is not modified.
                return new SpriteAnimationFrame
                {
                    Duration = frame.Duration > 0f ? frame.Duration : frameDuration,
                    Margin = frame.Margin == default ? margin : frame.Margin,
                    SourceRectangle = frame.SourceRectangle == default ? sourceRectangle : frame.SourceRectangle,
                    Texture = frame.Texture ?? frameTexture
                };
            }

            return new SpriteAnimationFrame
            {
                Duration = frameDuration,
                Margin = margin,
                SourceRectangle = sourceRectangle,
                Texture = frameTexture
            };
        }

        private int GetTotalFramesPerRow(int totalFrames) => frameCountRow > 0 ? frameCountRow : Math.Max(1, totalFrames);
    }
}
