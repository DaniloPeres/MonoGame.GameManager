using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace MonoGame.GameManager.Converters
{
    /// <summary>
    /// Converts a <see cref="Rectangle"/> (or a nullable one) to and from the JSON string "x y width height".
    /// </summary>
    public class RectangleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Rectangle) || objectType == typeof(Rectangle?);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var rectangle = (Rectangle)value;
            writer.WriteValue(string.Join(" ",
                rectangle.X.ToString(CultureInfo.InvariantCulture),
                rectangle.Y.ToString(CultureInfo.InvariantCulture),
                rectangle.Width.ToString(CultureInfo.InvariantCulture),
                rectangle.Height.ToString(CultureInfo.InvariantCulture)));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return objectType == typeof(Rectangle?) ? (object)null : default(Rectangle);

            var values = ConverterHelper.ReadNumbers(reader, 4);
            return new Rectangle((int)values[0], (int)values[1], (int)values[2], (int)values[3]);
        }
    }
}
