namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// Where the particles of a <see cref="ParticleSystem"/> are born, around the emitter position.
    /// </summary>
    public enum EmitterShape
    {
        /// <summary>
        /// At the emitter position. When <see cref="ParticleSettings.SpawnRadius"/> is greater than 0 it behaves like
        /// <see cref="Circle"/>.
        /// </summary>
        Point,

        /// <summary>
        /// Inside a circle of <see cref="ParticleSettings.SpawnRadius"/>, or on its edge with
        /// <see cref="ParticleSettings.EmitFromEdge"/>.
        /// </summary>
        Circle,

        /// <summary>
        /// Between <see cref="ParticleSettings.SpawnInnerRadius"/> and <see cref="ParticleSettings.SpawnRadius"/>.
        /// </summary>
        Ring,

        /// <summary>
        /// Inside a rectangle of <see cref="ParticleSettings.SpawnSize"/> centered on the emitter (rotated by
        /// <see cref="ParticleSettings.SpawnRotation"/>), or on its perimeter with <see cref="ParticleSettings.EmitFromEdge"/>.
        /// </summary>
        Rectangle,

        /// <summary>
        /// On a segment of <see cref="ParticleSettings.SpawnSize"/>.X pixels centered on the emitter, horizontal unless
        /// rotated by <see cref="ParticleSettings.SpawnRotation"/> (eg: rain and snow along the top of the screen).
        /// </summary>
        Line
    }
}
