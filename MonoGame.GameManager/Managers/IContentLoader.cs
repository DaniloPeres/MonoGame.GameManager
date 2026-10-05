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

        SpriteFont LoadSpriteFont(string assetName);

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
