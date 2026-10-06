namespace MonoGame.GameManager.Particles
{
    /// <summary>
    /// What happens when a particle reaches <see cref="ParticleSettings.Floor"/> or leaves <see cref="ParticleSettings.Bounds"/>.
    /// </summary>
    public enum ParticleBoundsMode
    {
        /// <summary>The floor and the bounds are ignored.</summary>
        None,

        /// <summary>The particle dies (the <see cref="ParticleSettings.OnDeath"/> sub-emitter is triggered).</summary>
        Kill,

        /// <summary>
        /// The particle bounces, keeping <see cref="ParticleSettings.Bounciness"/> of its speed and losing
        /// <see cref="ParticleSettings.Friction"/> of the speed along the surface.
        /// </summary>
        Bounce
    }
}
