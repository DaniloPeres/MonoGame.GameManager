using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Services;
using System;

namespace Snake
{
    public class ContentHandler
    {
        private static readonly Lazy<ContentHandler> lazyInstance = new Lazy<ContentHandler>(() => new ContentHandler());
        public static ContentHandler Instance => lazyInstance.Value;

        public SpriteFont HoboStdSpriteFont { get; private set; }
        public Texture2D TextureFood { get; private set; }

        private ContentHandler() { }

        public void LoadAllContents()
        {
            HoboStdSpriteFont = ServiceProvider.ContentLoaderManager.LoadSpriteFont("HoboStd");
            TextureFood = ServiceProvider.ContentLoaderManager.LoadTexture2D("Food");
        }
    }
}
