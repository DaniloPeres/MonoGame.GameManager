using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using MonoGame.GameManager.Core;

namespace MonoGame.GameManager.Audio
{
    /// <summary>
    /// Plays sound effects and music with separate volume settings.
    /// </summary>
    public interface IAudioManager : IUpdatable
    {
        /// <summary>The volume applied to everything, from 0 to 1.</summary>
        float MasterVolume { get; set; }

        /// <summary>The volume of the sound effects, from 0 to 1.</summary>
        float SoundVolume { get; set; }

        /// <summary>The volume of the music, from 0 to 1.</summary>
        float MusicVolume { get; set; }

        bool IsMuted { get; set; }

        /// <summary>The maximum number of simultaneous instances of the same sound effect.</summary>
        int MaxInstancesPerSound { get; set; }

        bool IsMusicPlaying { get; }

        /// <summary>Loads (with the global content loader) and plays a sound effect.</summary>
        SoundHandle PlaySound(string assetName, float volume = 1f, float pitch = 0f, float pan = 0f);

        SoundHandle PlaySound(SoundEffect soundEffect, float volume = 1f, float pitch = 0f, float pan = 0f, bool isLooped = false);

        void StopAllSounds();

        /// <summary>Loads (with the global content loader) and plays a song, replacing the current one.</summary>
        void PlayMusic(string assetName, bool isRepeating = true, float fadeInSeconds = 0f);

        void PlayMusic(Song song, bool isRepeating = true, float fadeInSeconds = 0f);

        void StopMusic(float fadeOutSeconds = 0f);

        void PauseMusic();

        void ResumeMusic();
    }
}
