using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace MonoGame.GameManager.Converters
{
    /// <summary>
    /// Converts a <see cref="Point"/> (or a nullable one) to and from the JSON string "x y".
    /// </summary>
    public class PointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Point) || objectType == typeof(Point?);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var point = (Point)value;
            writer.WriteValue(point.X.ToString(CultureInfo.InvariantCulture) + " " + point.Y.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return objectType == typeof(Point?) ? (object)null : default(Point);

            var values = ConverterHelper.ReadNumbers(reader, 2);
            return new Point((int)values[0], (int)values[1]);
        }
    }
}
