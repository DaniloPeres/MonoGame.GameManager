using System.Collections.Generic;

namespace MonoGame.GameManager.Storage
{
    /// <summary>
    /// Saves and loads game data (settings, progress, high scores...) in named slots.
    /// </summary>
    public interface ISaveGameService
    {
        /// <summary>The folder where the slots are stored.</summary>
        string Directory { get; }

        /// <summary>Saves data in a slot, replacing its previous content.</summary>
        void Save<T>(string slot, T data);

        /// <summary>Loads a slot, or returns <paramref name="defaultValue"/> when it does not exist or cannot be read.</summary>
        T Load<T>(string slot, T defaultValue = default);

        bool TryLoad<T>(string slot, out T data);

        bool Exists(string slot);

        void Delete(string slot);

        /// <summary>The names of the existing slots.</summary>
        IEnumerable<string> ListSlots();
    }
}
