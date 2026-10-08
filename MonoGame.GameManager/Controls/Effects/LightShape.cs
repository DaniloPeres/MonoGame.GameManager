namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>The shape of a point of light (gems, running lights, sparkles), see <see cref="ButtonEffectResources.GetShape"/>.</summary>
    public enum LightShape
    {
        /// <summary>A soft round light.</summary>
        Glow,

        /// <summary>A four-pointed sparkle with a bright center.</summary>
        Flare,

        /// <summary>A five-pointed star.</summary>
        Star,

        /// <summary>A diamond (a gem).</summary>
        Diamond,

        /// <summary>A filled circle.</summary>
        Circle,

        /// <summary>The outline of a circle.</summary>
        Ring
    }
}
