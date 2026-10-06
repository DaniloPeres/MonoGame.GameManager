using Newtonsoft.Json;

namespace MonoGame.GameManager.Converters
{
    /// <summary>
    /// JSON settings that support the MonoGame types (<see cref="Microsoft.Xna.Framework.Vector2"/>,
    /// <see cref="Microsoft.Xna.Framework.Point"/>, <see cref="Microsoft.Xna.Framework.Rectangle"/> and
    /// <see cref="Microsoft.Xna.Framework.Color"/>).
    /// </summary>
    public static class GameManagerJson
    {
        /// <summary>Creates new settings with the MonoGame converters.</summary>
        public static JsonSerializerSettings CreateSettings(bool indented = true)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = indented ? Formatting.Indented : Formatting.None,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            settings.Converters.Add(new Vector2Converter());
            settings.Converters.Add(new PointConverter());
            settings.Converters.Add(new RectangleConverter());
            settings.Converters.Add(new ColorConverter());
            return settings;
        }
    }
}
