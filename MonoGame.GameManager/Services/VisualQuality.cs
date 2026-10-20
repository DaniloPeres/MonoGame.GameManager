namespace MonoGame.GameManager.Services
{
    /// <summary>
    /// Global knobs that make the effects cheaper, set by the game from its quality setting (eg: a Low / Medium / High
    /// option for phones). The defaults draw everything.
    /// </summary>
    /// <remarks>
    /// Particles and button effects read them every frame, so a change applies at once to what is already on the screen.
    /// </remarks>
    public static class VisualQuality
    {
        private static float particleEmissionScale = 1f;

        /// <summary>
        /// Multiplies every particle emitted (continuous, by distance, bursts, sub-emitters and <c>Emit</c> calls), from 0 (none)
        /// to 1 (the default). The fractions are carried over, so 0.4 emits 4 particles out of 10.
        /// </summary>
        public static float ParticleEmissionScale
        {
            get => particleEmissionScale;
            set => particleEmissionScale = value < 0f ? 0f : value > 1f ? 1f : value;
        }

        /// <summary>
        /// False hides the decorative animated button effects (sparkles, particle fields, running lights, glints, light rays,
        /// shine sweeps; <see cref="Controls.Effects.ButtonEffect.IsDecorativeAnimation"/>). Glows, gloss, frames, shades and
        /// click ripples stay.
        /// </summary>
        public static bool DecorativeButtonEffects { get; set; } = true;

        /// <summary>False stops the pulses of the button effects: their lights stay steady, so their render targets are kept.</summary>
        public static bool ButtonPulses { get; set; } = true;

        /// <summary>
        /// The most times per second the animated inside layer of a button (shine sweeps, twinkling particle fields, ripples)
        /// is rendered again into its render targets; in between, the last one is drawn. 60 by default: no change at 60 FPS,
        /// and a game running far faster does not switch render targets at every frame. 0 renders it at every change.
        /// </summary>
        public static float ButtonEffectRefreshRate { get; set; } = 60f;
    }
}
