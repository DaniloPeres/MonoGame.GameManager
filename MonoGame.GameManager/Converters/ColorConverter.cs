using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace MonoGame.GameManager.Converters
{
    /// <summary>
    /// Converts a <see cref="Color"/> (or a nullable one) to and from the JSON string "r g b a" (0 to 255).
    /// </summary>
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Color) || objectType == typeof(Color?);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var color = (Color)value;
            writer.WriteValue(string.Join(" ",
                color.R.ToString(CultureInfo.InvariantCulture),
                color.G.ToString(CultureInfo.InvariantCulture),
                color.B.ToString(CultureInfo.InvariantCulture),
                color.A.ToString(CultureInfo.InvariantCulture)));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return objectType == typeof(Color?) ? (object)null : default(Color);

            var values = ConverterHelper.ReadNumbers(reader, 3, 4);
            var alpha = values.Length > 3 ? (int)values[3] : 255;
            return new Color((int)values[0], (int)values[1], (int)values[2], alpha);
        }
    }
}
