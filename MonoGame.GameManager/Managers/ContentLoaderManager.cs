using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Services;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Controls.Sprites;
using Microsoft.Xna.Framework;
using System.IO;
using MonoGame.GameManager.Pipeline;

namespace MonoGame.GameManager.Managers
{
    public class ContentLoaderManager
    {
        private readonly MemoryManager memoryManager;

        public ContentLoaderManager(MemoryManager memoryManager)
        {
            this.memoryManager = memoryManager;
        }

        public Texture2D LoadTexture2D(string name)
        {
            if (memoryManager.TryGetAsset(name, out Texture2D texture))
                return texture;

            // TODO option to use Load as stream
            //if (VariaveisDP.LoadImagesAsStream)
            //{
            //    using (var fileStream = GetContentFileStream($"{image_link}.{GetImageTypeExtension(imgType)}"))
            //    {
            //        image = StreamToTexture2D(fileStream);
            //    }
            //}
            //else
            texture = LoadFromContent<Texture2D>(name);

            memoryManager.AddAsset(name, texture);

            return texture;
        }

        public SpriteFont LoadSpriteFont(string name)
        {
            SpriteFont spriteFont;
            if (memoryManager.TryGetAsset(name, out spriteFont))
                return spriteFont;

            spriteFont = LoadFromContent<SpriteFont>(name);
            memoryManager.AddAsset(name, spriteFont);
            return spriteFont;
        }

        public Texture2D[] LoadMultipleTextures(string texturesPathFormat, int frameCount, int frameStartCountNumber)
        {
            if (frameCount == 0)
                throw new Exception("Frame count was not set, set the frame count to load multiple textures");

            var textures = new List<Texture2D>();

            for (var i = 0; i < frameCount; i++)
                textures.Add(LoadTexture2D(string.Format(texturesPathFormat, i + frameStartCountNumber)));

            return textures.ToArray();
        }

        public SpriteAnimationInfo LoadSpriteAnimationInfo(string name)
        {
            return new SpriteAnimationPipelineReader(name)
                .Read();
        }

        public Stream GetContentFileStream(string assetName)
        {
            assetName = assetName.Replace("\\", "/");
            return TitleContainer.OpenStream("Content/" + assetName);
        }

        private T LoadFromContent<T>(string assetName) => ServiceProvider.ScreenManager.Content.Load<T>(assetName);
    }

}
