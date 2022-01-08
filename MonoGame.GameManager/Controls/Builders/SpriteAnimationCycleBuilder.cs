using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Sprites;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.Builders
{
    public class SpriteAnimationCycleBuilder
    {
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
        private const float DefaultFrameDuration = 1 / 60f; // set as 60 frames per second as default
        private float frameDuration = DefaultFrameDuration;
        private Vector2 margin;
        private Dictionary<int, SpriteAnimationFrame> frames;
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
        /// If you don't set the size, the builder will try to calculate it automatically.
        /// </summary>
        /// <param name="width">The width of the sprite</param>
        /// <param name="height">The height of the sprite</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithSize(float width, float height) => WithSize(new Vector2(width, height));

        /// <summary>
        /// Set the size of the sprite.
        /// If you don't set the size, the builder will try to calculate it automatically.
        /// </summary>
        /// <param name="size">The sprite size</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithSize(Vector2 size)
        {
            this.size = size;
            return this;
        }

        /// <summary>
        /// Set the position of the sprite in the texture
        /// </summary>
        /// <param name="left">The left position of the sprite in the texture</param>
        /// <param name="top">The top position of the sprite in the texture</param>
        /// <returns>The Builder</returns>
        public SpriteAnimationCycleBuilder WithTexturePosition(float left, float top) => WithTexturePosition(new Vector2(left, top));

        /// <summary>
        /// Set the position of the sprite in the texture
        /// </summary>
        /// <param name="position">The position of the sprite in the texture</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithTexturePosition(Vector2 position)
        {
            this.position = position;
            return this;
        }

        /// <summary>
        /// Set the total of sprite frames to extract from texture
        /// If you don't set the Total of Frames, the builder will try to calculate it automatically
        /// </summary>
        /// <param name="frameCount">The total of frames from the sprite</param>
        /// <returns>The builder</returns>
        public SpriteAnimationCycleBuilder WithTotalOfFrames(int frameCount)
        {
            this.frameCount = frameCount;
            return this;
        }

        public SpriteAnimationCycleBuilder WithFrame(int frameIndex, SpriteAnimationFrame frame)
        {
            if (frames == null)
                frames = new Dictionary<int, SpriteAnimationFrame>();
            frames.Add(frameIndex, frame);
            return this;
        }

        public SpriteAnimationCycleBuilder WithFrames(List<SpriteAnimationFrame> frames)
        {
            for (var i = 0; i < frames.Count; i++)
            {
                WithFrame(i, frames[i]);
            }
            return this;
        }

        public SpriteAnimationCycleBuilder WithFramesCountOnRow(int frameCountRow)
        {
            this.frameCountRow = frameCountRow;
            return this;
        }

        public SpriteAnimationCycleBuilder WithFrameDuration(float frameDuration)
        {
            this.frameDuration = frameDuration;
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

        private string GetCycleNameToUse()
            => string.IsNullOrEmpty(cycleName)
                ? SpriteAnimationCycle.DefaultCycleName
                : cycleName;

        private SpriteAnimationFrame[] GenerateFrames()
        {
            var frames = new List<SpriteAnimationFrame>();

            var textures = GetTextures();
            var size = GetSize(textures);

            for (var i = 0; i < frameCount; i++)
            {
                frames.Add(CreateSpriteAnimationFrame(textures, size, i));
            }

            return frames.ToArray();
        }

        private Texture2D[] GetTextures()
        {
            if (texture != null)
                return new Texture2D[] { texture };
            else if (textures != null)
                return textures;
            else if (!string.IsNullOrEmpty(texturePath))
                return new Texture2D[] { ServiceProvider.ContentLoaderManager.LoadTexture2D(texturePath) };
            else if (!string.IsNullOrEmpty(texturesPathFormat))
                return ServiceProvider.ContentLoaderManager.LoadMultipleTextures(texturesPathFormat, frameCount, multipleTexturesStartingCount);
            else
                throw new Exception("Texture was not set, set the texture or texturePath");
        }

        private Vector2 GetSize(Texture2D[] textures)
        {
            if (size != default)
                return size;

            var textureSize = textures.First().Size().ToVector2();

            if (textures.Length > 1)
                return textureSize;

            var totalFramesPerRow = GetTotalFramesPerRow();
            return new Vector2(textureSize.X / totalFramesPerRow, textureSize.Y / (float)Math.Ceiling(frameCount / (double)totalFramesPerRow));
        }

        private SpriteAnimationFrame CreateSpriteAnimationFrame(Texture2D[] textures, Vector2 size, int frameIndex)
        {
            var textureIndex = Math.Min(frameIndex, textures.Length - 1);
            var texture = textures[textureIndex];

            var position = this.position;
            if (textureIndex == 0)
            {
                var totalFramesPerRow = GetTotalFramesPerRow();
                var matrixPosition = new Vector2(frameIndex % totalFramesPerRow, (int)Math.Floor(frameIndex / (double)totalFramesPerRow));
                position += matrixPosition * size;
            }

            var sourceRectangle = new Rectangle(position.ToPoint(), size.ToPoint());

            if (frames != null && frames.TryGetValue(frameIndex, out var frame))
            {
                if (frame.Duration == default)
                    frame.Duration = frameDuration;
                if (frame.Margin == default)
                    frame.Margin = margin;
                if (frame.SourceRectangle == default)
                    frame.SourceRectangle = sourceRectangle;
                if (frame.Texture == null)
                    frame.Texture = texture;
            }
            else
            {
                frame = new SpriteAnimationFrame
                {
                    Duration = frameDuration,
                    Margin = margin,
                    SourceRectangle = sourceRectangle,
                    Texture = texture
                };
            }


            return frame;
        }

        private int GetTotalFramesPerRow() => frameCountRow != 0 ? frameCountRow : frameCount;
    }
}
