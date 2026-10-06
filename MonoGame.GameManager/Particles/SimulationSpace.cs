namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// The coordinate space where the particles live once they are born.
    /// </summary>
    public enum SimulationSpace
    {
        /// <summary>
        /// The particles stay where they were born when the emitter moves (smoke behind a car, a trail behind the
        /// pointer). This is the behaviour of version 2.0.
        /// </summary>
        World,

        /// <summary>
        /// The particles move, rotate and scale with the emitter (the flame of a torch carried by a character).
        /// </summary>
        Local
    }
}
