namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// How the <see cref="ParticleSettings.Frames"/> of a sprite sheet are used by the particles.
    /// </summary>
    public enum ParticleFrameMode
    {
        /// <summary>Each particle shows one frame chosen at random when it is born.</summary>
        Random,

        /// <summary>Each particle plays all the frames once along its life (born = first frame, dead = last frame).</summary>
        Animate,

        /// <summary>Each particle loops the frames at <see cref="ParticleSettings.FrameRate"/>, starting at a random frame.</summary>
        Loop
    }
}
