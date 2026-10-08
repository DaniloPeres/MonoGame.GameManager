using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.Services
{
    public class ContentHandler
    {
        private static readonly Lazy<ContentHandler> lazyInstance = new Lazy<ContentHandler>(() => new ContentHandler());
        public static ContentHandler Instance => lazyInstance.Value;

        public FontSystem FontSystem { get; private set; }
        public SpriteFontBase Font { get; private set; }

        /// <summary>The fonts of the demos, by name (Roboto first), at size 36.</summary>
        public IReadOnlyList<(string Name, SpriteFontBase Font)> Fonts { get; private set; }

        public SpriteFontBase FontAlfaSlabOne { get; private set; }
        public SpriteFontBase FontLilitaOne { get; private set; }
        public SpriteFontBase FontBangers { get; private set; }
        public SpriteFontBase FontPressStart { get; private set; }
        public SpriteFontBase FontAudiowide { get; private set; }
        public SpriteFontBase FontCreepster { get; private set; }
        public SpriteFontBase FontPacifico { get; private set; }
        public SpriteFontBase FontLuckiestGuy { get; private set; }
        public SpriteFontBase FontRye { get; private set; }
        public SpriteFontBase FontBlackOpsOne { get; private set; }
        public SpriteFontBase FontMonoton { get; private set; }
        public SpriteFontBase FontPermanentMarker { get; private set; }
        public SpriteFontBase FontCinzelDecorative { get; private set; }
        public SpriteFontBase FontBungee { get; private set; }
        public SpriteFontBase FontSpecialElite { get; private set; }
        public SpriteFontBase FontKaushanScript { get; private set; }
        public SpriteFontBase FontUnifraktur { get; private set; }

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
            FontSystem = ServiceProvider.ContentLoader.LoadFontSystem("Fonts/Roboto-Regular.ttf");
            Font = FontSystem.GetFont(36);

            // Display fonts (SIL Open Font License or Apache 2.0, see the license files next to them), with Roboto for missing characters.
            SpriteFontBase Load(string file) => ServiceProvider.ContentLoader.LoadFontSystem($"Fonts/{file}", "Fonts/Roboto-Regular.ttf").GetFont(36);
            FontAlfaSlabOne = Load("AlfaSlabOne-Regular.ttf");
            FontLilitaOne = Load("LilitaOne-Regular.ttf");
            FontBangers = Load("Bangers-Regular.ttf");
            FontPressStart = Load("PressStart2P-Regular.ttf");
            FontAudiowide = Load("Audiowide-Regular.ttf");
            FontCreepster = Load("Creepster-Regular.ttf");
            FontPacifico = Load("Pacifico-Regular.ttf");
            FontLuckiestGuy = Load("LuckiestGuy-Regular.ttf");
            FontRye = Load("Rye-Regular.ttf");
            FontBlackOpsOne = Load("BlackOpsOne-Regular.ttf");
            FontMonoton = Load("Monoton-Regular.ttf");
            FontPermanentMarker = Load("PermanentMarker-Regular.ttf");
            FontCinzelDecorative = Load("CinzelDecorative-Bold.ttf");
            FontBungee = Load("Bungee-Regular.ttf");
            FontSpecialElite = Load("SpecialElite-Regular.ttf");
            FontKaushanScript = Load("KaushanScript-Regular.ttf");
            FontUnifraktur = Load("UnifrakturMaguntia-Book.ttf");
            Fonts = new List<(string, SpriteFontBase)>
            {
                ("Roboto", Font),
                ("Alfa Slab One", FontAlfaSlabOne),
                ("Lilita One", FontLilitaOne),
                ("Bangers", FontBangers),
                ("Press Start 2P", FontPressStart),
                ("Audiowide", FontAudiowide),
                ("Creepster", FontCreepster),
                ("Pacifico", FontPacifico),
                ("Luckiest Guy", FontLuckiestGuy),
                ("Rye", FontRye),
                ("Black Ops One", FontBlackOpsOne),
                ("Monoton", FontMonoton),
                ("Permanent Marker", FontPermanentMarker),
                ("Cinzel Decorative", FontCinzelDecorative),
                ("Bungee", FontBungee),
                ("Special Elite", FontSpecialElite),
                ("Kaushan Script", FontKaushanScript),
                ("UnifrakturMaguntia", FontUnifraktur)
            };

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
