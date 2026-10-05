using MonoGame.GameManager.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MonoGame.GameManager.Storage
{
    /// <summary>
    /// Stores every slot as a JSON file ("slot.json") in a folder of the user's application data.
    /// Files are written to a temporary file first, so a crash while saving does not corrupt the previous save.
    /// </summary>
    /// <example>
    /// <code>
    /// var storage = ServiceProvider.Storage;
    /// storage.Save("settings", new GameSettings { MusicVolume = 0.5f });
    /// var settings = storage.Load("settings", new GameSettings());
    /// </code>
    /// </example>
    public class JsonSaveGameService : ISaveGameService
    {
        private const string Extension = ".json";
        private readonly JsonSerializerSettings settings;

        /// <param name="directory">The folder of the save files (it is created when needed).</param>
        /// <param name="settings">JSON settings (defaults to <see cref="GameManagerJson.CreateSettings"/>).</param>
        public JsonSaveGameService(string directory, JsonSerializerSettings settings = null)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("The directory cannot be empty.", nameof(directory));
            Directory = directory;
            this.settings = settings ?? GameManagerJson.CreateSettings();
        }

        public string Directory { get; }

        /// <summary>
        /// The default folder for an application: "[local application data]/[applicationName]/Saves".
        /// </summary>
        public static string DefaultDirectory(string applicationName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
                applicationName = "MonoGameGame";
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(root))
                root = AppContext.BaseDirectory;
            return Path.Combine(root, SanitizeName(applicationName), "Saves");
        }

        public void Save<T>(string slot, T data)
        {
            System.IO.Directory.CreateDirectory(Directory);
            var path = GetPath(slot);
            var temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(data, settings), Encoding.UTF8);

            if (!File.Exists(path))
            {
                File.Move(temporaryPath, path);
                return;
            }

            try
            {
                File.Replace(temporaryPath, path, null);
            }
            catch (PlatformNotSupportedException)
            {
                File.Delete(path);
                File.Move(temporaryPath, path);
            }
        }

        public T Load<T>(string slot, T defaultValue = default) => TryLoad(slot, out T data) ? data : defaultValue;

        public bool TryLoad<T>(string slot, out T data)
        {
            data = default;
            var path = GetPath(slot);
            if (!File.Exists(path))
                return false;

            try
            {
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path, Encoding.UTF8), settings);
                return data != null;
            }
            catch (JsonException)
            {
                return false; // corrupted or incompatible file
            }
            catch (IOException)
            {
                return false;
            }
        }

        public bool Exists(string slot) => File.Exists(GetPath(slot));

        public void Delete(string slot)
        {
            var path = GetPath(slot);
            if (File.Exists(path))
                File.Delete(path);
        }

        public IEnumerable<string> ListSlots()
        {
            if (!System.IO.Directory.Exists(Directory))
                return Enumerable.Empty<string>();

            return System.IO.Directory.GetFiles(Directory, "*" + Extension)
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string GetPath(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot))
                throw new ArgumentException("The slot name cannot be empty.", nameof(slot));
            return Path.Combine(Directory, SanitizeName(slot) + Extension);
        }

        private static string SanitizeName(string name)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(name.Length);
            foreach (var character in name.Trim())
                builder.Append(Array.IndexOf(invalidCharacters, character) >= 0 ? '_' : character);
            return builder.ToString();
        }
    }
}
