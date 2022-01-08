using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace MonoGame.GameManager.Converters
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteValue(value.X.ToString(CultureInfo.InvariantCulture) + " " + value.Y.ToString(CultureInfo.InvariantCulture));
        }

        /// <inheritdoc />
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return default;

            var value = reader.Value.ToString().Split(' ');
            return new Vector2(float.Parse(value[0], CultureInfo.InvariantCulture), float.Parse(value[1], CultureInfo.InvariantCulture));
        }
    }
}
