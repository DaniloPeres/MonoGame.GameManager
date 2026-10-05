using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using MonoGame.GameManager.Managers;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Audio
{
    /// <summary>
    /// Default <see cref="IAudioManager"/>. Sound effect instances are pooled per sound, and the music volume can
    /// fade in and out. It is updated every frame by the screen manager.
    /// </summary>
    /// <example>
    /// <code>
    /// var audio = ServiceProvider.Audio;
    /// audio.PlayMusic("Music/Theme", fadeInSeconds: 1f);
    /// audio.PlaySound("Sounds/Coin", volume: 0.8f, pitch: 0.1f);
    /// audio.SoundVolume = settings.SoundVolume;
    /// </code>
    /// </example>
    public class AudioManager : IAudioManager, IDisposable
    {
        private readonly IContentLoader contentLoader;
        private readonly IMusicPlayer musicPlayer;
        private readonly Dictionary<SoundEffect, List<SoundHandle>> soundHandles = new Dictionary<SoundEffect, List<SoundHandle>>();
        private float masterVolume = 1f;
        private float soundVolume = 1f;
        private float musicVolume = 1f;
        private bool isMuted;
        private float musicFade = 1f;
        private float musicFadeTarget = 1f;
        private float musicFadeSpeed;
        private bool stopMusicWhenFadedOut;

        /// <param name="contentLoader">Loads the sounds and songs played by name (can be null).</param>
        /// <param name="musicPlayer">Plays the music (defaults to the MonoGame media player).</param>
        public AudioManager(IContentLoader contentLoader, IMusicPlayer musicPlayer = null)
        {
            this.contentLoader = contentLoader;
            this.musicPlayer = musicPlayer ?? new MediaPlayerMusicPlayer();
        }

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = MathHelper.Clamp(value, 0f, 1f);
                ApplyVolumes();
            }
        }

        public float SoundVolume
        {
            get => soundVolume;
            set
            {
                soundVolume = MathHelper.Clamp(value, 0f, 1f);
                ApplyVolumes();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = MathHelper.Clamp(value, 0f, 1f);
                ApplyMusicVolume();
            }
        }

        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                ApplyVolumes();
            }
        }

        public int MaxInstancesPerSound { get; set; } = 16;

        public bool IsMusicPlaying => musicPlayer.IsPlaying;

        public SoundHandle PlaySound(string assetName, float volume = 1f, float pitch = 0f, float pan = 0f)
        {
            if (contentLoader == null)
                throw new InvalidOperationException("The audio manager has no content loader to load sounds by name.");
            return PlaySound(contentLoader.LoadSoundEffect(assetName), volume, pitch, pan);
        }

        public SoundHandle PlaySound(SoundEffect soundEffect, float volume = 1f, float pitch = 0f, float pan = 0f, bool isLooped = false)
        {
            if (soundEffect == null || soundEffect.IsDisposed)
                return SoundHandle.None;

            var handle = GetAvailableHandle(soundEffect);
            if (handle == null)
                return SoundHandle.None;

            var instance = handle.Instance;
            instance.IsLooped = isLooped;
            handle.Volume = volume;
            handle.Pitch = pitch;
            handle.Pan = pan;
            instance.Play();
            return handle;
        }

        public void StopAllSounds()
        {
            foreach (var handles in soundHandles.Values)
            {
                foreach (var handle in handles)
                    handle.Stop();
            }
        }

        public void PlayMusic(string assetName, bool isRepeating = true, float fadeInSeconds = 0f)
        {
            if (contentLoader == null)
                throw new InvalidOperationException("The audio manager has no content loader to load songs by name.");
            PlayMusic(contentLoader.LoadSong(assetName), isRepeating, fadeInSeconds);
        }

        public void PlayMusic(Song song, bool isRepeating = true, float fadeInSeconds = 0f)
        {
            if (song == null)
                throw new ArgumentNullException(nameof(song));

            stopMusicWhenFadedOut = false;
            musicPlayer.IsRepeating = isRepeating;
            if (fadeInSeconds > 0f)
            {
                musicFade = 0f;
                StartFade(1f, fadeInSeconds);
            }
            else
            {
                musicFade = 1f;
                musicFadeTarget = 1f;
                musicFadeSpeed = 0f;
            }

            ApplyMusicVolume();
            musicPlayer.Play(song);
        }

        public void StopMusic(float fadeOutSeconds = 0f)
        {
            if (fadeOutSeconds > 0f && musicPlayer.IsPlaying)
            {
                stopMusicWhenFadedOut = true;
                StartFade(0f, fadeOutSeconds);
                return;
            }

            stopMusicWhenFadedOut = false;
            musicFadeSpeed = 0f;
            musicPlayer.Stop();
        }

        public void PauseMusic() => musicPlayer.Pause();

        public void ResumeMusic() => musicPlayer.Resume();

        /// <summary>Advances the music fades.</summary>
        public void Update(float deltaSeconds)
        {
            if (musicFadeSpeed <= 0f)
                return;

            musicFade = MathUtilsMoveTowards(musicFade, musicFadeTarget, musicFadeSpeed * deltaSeconds);
            ApplyMusicVolume();

            if (Math.Abs(musicFade - musicFadeTarget) > 0.0001f)
                return;

            musicFadeSpeed = 0f;
            if (stopMusicWhenFadedOut)
            {
                stopMusicWhenFadedOut = false;
                musicPlayer.Stop();
            }
        }

        public void Dispose()
        {
            foreach (var handles in soundHandles.Values)
            {
                foreach (var handle in handles)
                    handle.Instance?.Dispose();
            }
            soundHandles.Clear();
        }

        internal float GetEffectiveSoundVolume(float volume) => isMuted ? 0f : MathHelper.Clamp(volume * soundVolume * masterVolume, 0f, 1f);

        private SoundHandle GetAvailableHandle(SoundEffect soundEffect)
        {
            if (!soundHandles.TryGetValue(soundEffect, out var handles))
            {
                handles = new List<SoundHandle>();
                soundHandles[soundEffect] = handles;
            }

            handles.RemoveAll(handle => handle.Instance.IsDisposed);

            foreach (var handle in handles)
            {
                if (handle.Instance.State == SoundState.Stopped)
                    return handle;
            }

            if (handles.Count >= Math.Max(1, MaxInstancesPerSound))
                return null;

            var newHandle = new SoundHandle(this, soundEffect.CreateInstance(), 1f);
            handles.Add(newHandle);
            return newHandle;
        }

        private void StartFade(float target, float seconds)
        {
            musicFadeTarget = target;
            musicFadeSpeed = Math.Abs(target - musicFade) / Math.Max(0.0001f, seconds);
            if (musicFadeSpeed <= 0f)
                musicFadeSpeed = 1f / Math.Max(0.0001f, seconds);
        }

        private void ApplyVolumes()
        {
            foreach (var handles in soundHandles.Values)
            {
                foreach (var handle in handles)
                    handle.ApplyVolume();
            }
            ApplyMusicVolume();
        }

        private void ApplyMusicVolume()
            => musicPlayer.Volume = isMuted ? 0f : MathHelper.Clamp(musicVolume * masterVolume * musicFade, 0f, 1f);

        private static float MathUtilsMoveTowards(float current, float target, float maxDelta)
            => Math.Abs(target - current) <= maxDelta ? target : current + Math.Sign(target - current) * maxDelta;
    }
}
