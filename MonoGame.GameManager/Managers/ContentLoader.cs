using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.GameManager.Controls.Sprites;
using MonoGame.GameManager.Pipeline;
using MonoGame.GameManager.Text;
using System;
using System.IO;
using System.Linq;

namespace MonoGame.GameManager.Managers
{
    /// <summary>
    /// Default <see cref="IContentLoader"/> implementation.
    /// </summary>
    public class ContentLoader : IContentLoader
    {
        private const string SpriteAnimationCachePrefix = "sprite-animation:";
        private const string FileTextureCachePrefix = "file-texture:";
        private const string FontSystemCachePrefix = "font-system:";

        private readonly bool ownsContentManager;
        private bool isDisposed;

        /// <param name="contentManager">The content manager used to load the assets.</param>
        /// <param name="cache">The cache for the derived assets (a new one is created when null).</param>
        /// <param name="ownsContentManager">When true, the content manager is disposed with this loader.</param>
        public ContentLoader(ContentManager contentManager, IAssetCache cache = null, bool ownsContentManager = false)
        {
            ContentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
            Cache = cache ?? new AssetCache();
            this.ownsContentManager = ownsContentManager;
        }

        public ContentManager ContentManager { get; }

        public IAssetCache Cache { get; }

        public T Load<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentException("The asset name cannot be empty.", nameof(assetName));
            return ContentManager.Load<T>(NormalizePath(assetName));
        }

        public Texture2D LoadTexture2D(string assetName) => Load<Texture2D>(assetName);

        public FontSystem LoadFontSystem(params string[] relativePaths)
        {
            if (relativePaths == null || relativePaths.Length == 0 || relativePaths.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Set at least one font file, without empty paths.", nameof(relativePaths));

            var key = FontSystemCachePrefix + string.Join("|", relativePaths.Select(NormalizePath));
            if (Cache.TryGet(key, out FontSystem fontSystem))
                return fontSystem;

            fontSystem = new FontSystem(TextOutline.CreateFontSystemSettings());
            try
            {
                foreach (var relativePath in relativePaths)
                {
                    using (var stream = OpenStream(relativePath))
                        fontSystem.AddFont(stream);
                }
            }
            catch
            {
                fontSystem.Dispose();
                throw;
            }

            Cache.Add(key, fontSystem, disposeOnClear: true);
            return fontSystem;
        }

        public SpriteFontBase LoadFont(string relativePath, float size)
        {
            if (size <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), "Set a font size greater than zero.");
            return LoadFontSystem(relativePath).GetFont(size);
        }

        public SoundEffect LoadSoundEffect(string assetName) => Load<SoundEffect>(assetName);

        public Song LoadSong(string assetName) => Load<Song>(assetName);

        public Texture2D[] LoadMultipleTextures(string texturesPathFormat, int frameCount, int frameStartCountNumber = 0)
        {
            if (string.IsNullOrEmpty(texturesPathFormat))
                throw new ArgumentException("The textures path format cannot be empty.", nameof(texturesPathFormat));
            if (frameCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(frameCount), "Set a frame count greater than zero to load multiple textures.");

            var textures = new Texture2D[frameCount];
            for (var i = 0; i < frameCount; i++)
                textures[i] = LoadTexture2D(string.Format(texturesPathFormat, i + frameStartCountNumber));
            return textures;
        }

        public SpriteAnimationInfo LoadSpriteAnimationInfo(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentException("The asset name cannot be empty.", nameof(assetName));

            var key = SpriteAnimationCachePrefix + NormalizePath(assetName);
            if (Cache.TryGet(key, out SpriteAnimationInfo info))
                return info;

            info = new SpriteAnimationPipelineReader(this, assetName).Read();
            Cache.Add(key, info);
            return info;
        }

        public Texture2D LoadTexture2DFromFile(string relativePath)
        {
            var key = FileTextureCachePrefix + NormalizePath(relativePath);
            if (Cache.TryGet(key, out Texture2D texture) && !texture.IsDisposed)
                return texture;

            var graphicsDeviceService = (IGraphicsDeviceService)ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            var graphicsDevice = graphicsDeviceService?.GraphicsDevice
                ?? throw new InvalidOperationException("The graphics device is not available yet.");

            using (var stream = OpenStream(relativePath))
                texture = Texture2D.FromStream(graphicsDevice, stream);

            Cache.Add(key, texture, disposeOnClear: true);
            return texture;
        }

        public Stream OpenStream(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("The path cannot be empty.", nameof(relativePath));

            var rootDirectory = ContentManager.RootDirectory ?? string.Empty;
            var path = NormalizePath(Path.Combine(rootDirectory, relativePath));
            return Path.IsPathRooted(path) ? File.OpenRead(path) : TitleContainer.OpenStream(path);
        }

        [Obsolete("Use OpenStream instead.")]
        public Stream GetContentFileStream(string assetName) => OpenStream(assetName);

        public void UnloadAll()
        {
            Cache.Clear();
            ContentManager.Unload();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;

            Cache.Clear();
            if (ownsContentManager)
            {
                ContentManager.Unload();
                ContentManager.Dispose();
            }
        }

        private static string NormalizePath(string path) => path.Replace('\\', '/');
    }

    /// <summary>
    /// Kept for compatibility with version 1.x.
    /// </summary>
    [Obsolete("Use ContentLoader (IContentLoader) instead.")]
    public class ContentLoaderManager : ContentLoader
    {
        public ContentLoaderManager(ContentManager contentManager) : base(contentManager) { }
    }
}
