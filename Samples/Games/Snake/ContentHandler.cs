using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Services;
using System;

namespace Snake
{
    public class ContentHandler
    {
        private static readonly Lazy<ContentHandler> lazyInstance = new Lazy<ContentHandler>(() => new ContentHandler());
        public static ContentHandler Instance => lazyInstance.Value;

        public SpriteFontBase Font { get; private set; }
        public Texture2D TextureFood { get; private set; }

        private ContentHandler() { }

        public void LoadAllContents()
        {
            Font = ServiceProvider.ContentLoader.LoadFont("Fonts/Roboto-Regular.ttf", 37);
            TextureFood = ServiceProvider.ContentLoader.LoadTexture2D("food");
        }
    }
}
