using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MonoGame.GameManager.Audio
{
    /// <summary>
    /// Controls a sound started by <see cref="IAudioManager.PlaySound(SoundEffect, float, float, float, bool)"/>.
    /// The volume is relative: the master and sound effect volumes of the audio manager are applied on top.
    /// </summary>
    public sealed class SoundHandle
    {
        private readonly AudioManager audioManager;
        private float volume;

        internal SoundHandle(AudioManager audioManager, SoundEffectInstance instance, float volume)
        {
            this.audioManager = audioManager;
            Instance = instance;
            this.volume = volume;
        }

        /// <summary>A handle that does nothing, returned when a sound could not be played.</summary>
        public static SoundHandle None { get; } = new SoundHandle(null, null, 0f);

        /// <summary>The underlying MonoGame instance (null for <see cref="None"/>).</summary>
        public SoundEffectInstance Instance { get; }

        public bool IsPlaying => Instance != null && !Instance.IsDisposed && Instance.State == SoundState.Playing;

        public float Volume
        {
            get => volume;
            set
            {
                volume = MathHelper.Clamp(value, 0f, 1f);
                ApplyVolume();
            }
        }

        public float Pitch
        {
            get => Instance?.Pitch ?? 0f;
            set
            {
                if (IsValid)
                    Instance.Pitch = MathHelper.Clamp(value, -1f, 1f);
            }
        }

        public float Pan
        {
            get => Instance?.Pan ?? 0f;
            set
            {
                if (IsValid)
                    Instance.Pan = MathHelper.Clamp(value, -1f, 1f);
            }
        }

        public void Stop()
        {
            if (IsValid)
                Instance.Stop();
        }

        public void Pause()
        {
            if (IsValid)
                Instance.Pause();
        }

        public void Resume()
        {
            if (IsValid)
                Instance.Resume();
        }

        internal void ApplyVolume()
        {
            if (IsValid && audioManager != null)
                Instance.Volume = audioManager.GetEffectiveSoundVolume(volume);
        }

        private bool IsValid => Instance != null && !Instance.IsDisposed;
    }
}
