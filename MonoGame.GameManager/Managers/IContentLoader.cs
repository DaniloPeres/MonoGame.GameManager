using FontStashSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.GameManager.Controls.Sprites;
using System;
using System.IO;

namespace MonoGame.GameManager.Managers
{
    /// <summary>
    /// Loads and caches the assets of the game. Every loader wraps a MonoGame <see cref="ContentManager"/>:
    /// <see cref="Services.ServiceProvider.ContentLoader"/> keeps its assets for the whole game and
    /// <see cref="Screens.Screen.Content"/> releases its assets when the screen is closed.
    /// </summary>
    public interface IContentLoader : IDisposable
    {
        /// <summary>The underlying MonoGame content manager.</summary>
        ContentManager ContentManager { get; }

        /// <summary>The cache of the assets that are not cached by the content manager.</summary>
        IAssetCache Cache { get; }

        /// <summary>Loads any asset built by the content pipeline.</summary>
        T Load<T>(string assetName);

        Texture2D LoadTexture2D(string assetName);

        /// <summary>
        /// Loads (and caches) a FontStashSharp font system from TrueType/OpenType files copied to the content folder
        /// (eg: <c>/copy:Fonts/Roboto-Regular.ttf</c> in the .mgcb). The first file is the main font and the others
        /// are fallbacks for the characters it does not have. The font system supports colored outlines
        /// (<see cref="Text.TextOutline"/>) and is released with the loader.
        /// </summary>
        /// <param name="relativePaths">The font files, relative to the content root directory.</param>
        FontSystem LoadFontSystem(params string[] relativePaths);

        /// <summary>Loads a font file with <see cref="LoadFontSystem"/> and returns the font of <paramref name="size"/> pixels.</summary>
        SpriteFontBase LoadFont(string relativePath, float size);

        SoundEffect LoadSoundEffect(string assetName);

        Song LoadSong(string assetName);

        /// <summary>
        /// Loads a numbered sequence of textures, eg: format "Run ({0})" with start 1 loads "Run (1)", "Run (2)"...
        /// </summary>
        Texture2D[] LoadMultipleTextures(string texturesPathFormat, int frameCount, int frameStartCountNumber = 0);

        /// <summary>Loads (and caches) a sprite animation file (.sa).</summary>
        SpriteAnimationInfo LoadSpriteAnimationInfo(string assetName);

        /// <summary>
        /// Loads a texture from an image file (png, jpg...) copied to the content folder, without the content
        /// pipeline. The texture is cached and released with the loader.
        /// </summary>
        Texture2D LoadTexture2DFromFile(string relativePath);

        /// <summary>Opens a file of the content folder (relative to the content root directory).</summary>
        Stream OpenStream(string relativePath);

        [Obsolete("Use OpenStream instead.")]
        Stream GetContentFileStream(string assetName);

        /// <summary>Releases every asset loaded by this loader.</summary>
        void UnloadAll();
    }
}
