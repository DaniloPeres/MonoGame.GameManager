using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MonoGame.GameManager.Converters
{
    internal static class ConverterHelper
    {
        private static readonly char[] Separators = { ' ', ',', ';', '\t' };

        /// <summary>
        /// Reads numbers written as a string ("1 2 3") or as an array ([1, 2, 3]).
        /// </summary>
        public static double[] ReadNumbers(JsonReader reader, int minimumCount, int maximumCount = -1)
        {
            if (maximumCount < 0)
                maximumCount = minimumCount;

            var values = new List<double>();
            if (reader.TokenType == JsonToken.StartArray)
            {
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                    values.Add(Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture));
            }
            else
            {
                var text = Convert.ToString(reader.Value, CultureInfo.InvariantCulture) ?? string.Empty;
                foreach (var part in text.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                    values.Add(double.Parse(part, NumberStyles.Float, CultureInfo.InvariantCulture));
            }

            if (values.Count < minimumCount || values.Count > maximumCount)
            {
                var expected = minimumCount == maximumCount ? minimumCount.ToString(CultureInfo.InvariantCulture) : $"{minimumCount} to {maximumCount}";
                throw new JsonSerializationException($"Expected {expected} numbers at '{reader.Path}' but found {values.Count}.");
            }

            return values.ToArray();
        }
    }
}
