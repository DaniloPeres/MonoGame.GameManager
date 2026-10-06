using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Samples.Services
{
    public class ContentHandler
    {
        private static readonly Lazy<ContentHandler> lazyInstance = new Lazy<ContentHandler>(() => new ContentHandler());
        public static ContentHandler Instance => lazyInstance.Value;

        public SpriteFont SpriteFontArial { get; private set; }

        #region Images
        public Texture2D TextureButton { get; private set; }
        public Texture2D TextureButtonBackground { get; private set; }
        public Texture2D TextureButtonBackgroundHover { get; private set; }
        public Texture2D TextureButtonBackgroundPressed { get; private set; }
        public Texture2D TextureCalculator { get; private set; }
        public Texture2D TextureCamera { get; private set; }
        public Texture2D TextureImage { get; private set; }
        public Texture2D TextureMultiLineText { get; private set; }
        public Texture2D TexturePanel { get; private set; }
        public Texture2D TextureTextLabel { get; private set; }
        public Texture2D TextureTransition { get; private set; }
        public Texture2D TextureWindowApplication { get; private set; }
        public Texture2D TextureSpriteAngel { get; private set; }
        public Texture2D TextureSpriteCharacter { get; private set; }
        public Texture2D TextureSpriteCoin { get; private set; }
        public Texture2D TextureSpriteDeath { get; private set; }
        public Texture2D TextureSpriteLeviathan { get; private set; }
        public Texture2D TextureSpriteTorchDrippingRed { get; private set; }
        #endregion

        private ContentHandler() { }

        public void LoadAllContents()
        {
            SpriteFontArial = ServiceProvider.ContentLoader.LoadSpriteFont("Arial");

            LoadImages();
        }

        public void LoadImages()
        {
            var contentLoader = ServiceProvider.ContentLoader;
            TextureButton = contentLoader.LoadTexture2D("Images/Button");
            TextureButtonBackground = contentLoader.LoadTexture2D("Images/ButtonBackground");
            TextureButtonBackgroundHover = contentLoader.LoadTexture2D("Images/ButtonBackground-Hover");
            TextureButtonBackgroundPressed = contentLoader.LoadTexture2D("Images/ButtonBackground-Pressed");
            TextureCalculator = contentLoader.LoadTexture2D("Images/Calculator");
            TextureCamera = contentLoader.LoadTexture2D("Images/Camera");
            TextureImage = contentLoader.LoadTexture2D("Images/Image");
            TextureMultiLineText = contentLoader.LoadTexture2D("Images/Multi-line-text");
            TexturePanel = contentLoader.LoadTexture2D("Images/Panel");
            TextureTextLabel = contentLoader.LoadTexture2D("Images/Text-label");
            TextureTransition = contentLoader.LoadTexture2D("Images/Transition");
            TextureWindowApplication = contentLoader.LoadTexture2D("Images/WindowApplication");
            TextureSpriteAngel = contentLoader.LoadTexture2D("Images/Sprites/Angel");
            TextureSpriteCharacter = contentLoader.LoadTexture2D("Images/Sprites/Character");
            TextureSpriteCoin = contentLoader.LoadTexture2D("Images/Sprites/Coin");
            TextureSpriteDeath = contentLoader.LoadTexture2D("Images/Sprites/Death");
            TextureSpriteLeviathan = contentLoader.LoadTexture2D("Images/Sprites/Leviathan");
            TextureSpriteTorchDrippingRed = contentLoader.LoadTexture2D("Images/Sprites/TorchDrippingRed");
        }
    }
}
