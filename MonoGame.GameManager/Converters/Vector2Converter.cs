using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace MonoGame.GameManager.Converters
{
    /// <summary>
    /// Converts a <see cref="Vector2"/> (or a nullable one) to and from the JSON string "x y".
    /// </summary>
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Vector2) || objectType == typeof(Vector2?);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var vector = (Vector2)value;
            writer.WriteValue(vector.X.ToString(CultureInfo.InvariantCulture) + " " + vector.Y.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return objectType == typeof(Vector2?) ? (object)null : default(Vector2);

            var values = ConverterHelper.ReadNumbers(reader, 2);
            return new Vector2((float)values[0], (float)values[1]);
        }
    }
}
