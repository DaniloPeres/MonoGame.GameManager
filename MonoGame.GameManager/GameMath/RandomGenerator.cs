using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Default <see cref="IRandom"/> implementation, based on <see cref="System.Random"/>.
    /// </summary>
    public class RandomGenerator : IRandom
    {
        private readonly System.Random random;

        /// <summary>Creates a generator with a time-based seed.</summary>
        public RandomGenerator()
        {
            random = new System.Random();
        }

        /// <summary>Creates a generator that always produces the same sequence for the same seed.</summary>
        public RandomGenerator(int seed)
        {
            Seed = seed;
            random = new System.Random(seed);
        }

        /// <summary>A shared generator, used by the static helpers.</summary>
        public static RandomGenerator Default { get; } = new RandomGenerator();

        /// <summary>The seed, when the generator was created with one.</summary>
        public int? Seed { get; }

        /// <summary>An integer from <paramref name="minValue"/> to <paramref name="maxValue"/> (both included).</summary>
        public static int Random(int minValue, int maxValue) => Default.Next(minValue, maxValue);

        /// <summary>
        /// A number from <paramref name="minValue"/> to <paramref name="maxValue"/>, rounded to
        /// <paramref name="decimalNumbers"/> decimals.
        /// </summary>
        public static float Random(float minValue, float maxValue, int decimalNumbers = 2)
        {
            if (minValue > maxValue)
            {
                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            var value = minValue + Default.NextDouble() * ((double)maxValue - minValue);
            value = Math.Round(value, MathUtils.Clamp(decimalNumbers, 0, 15));
            return (float)MathUtils.Clamp(value, minValue, maxValue);
        }

        public int Next(int minValue, int maxValueInclusive)
        {
            if (minValue > maxValueInclusive)
            {
                var temp = minValue;
                minValue = maxValueInclusive;
                maxValueInclusive = temp;
            }

            var range = (long)maxValueInclusive - minValue + 1;
            return (int)(minValue + (long)(random.NextDouble() * range));
        }

        public double NextDouble() => random.NextDouble();

        public float NextFloat() => (float)random.NextDouble();

        public float NextFloat(float minValue, float maxValue) => minValue + (float)random.NextDouble() * (maxValue - minValue);

        public bool NextBool() => random.Next(2) == 0;

        public bool Chance(float probability) => probability > 0f && random.NextDouble() < probability;

        public float NextAngle() => (float)(random.NextDouble() * Math.PI * 2);

        public Vector2 NextUnitVector() => MathUtils.AngleToVector(NextAngle());

        public Vector2 NextPointInRectangle(RectangleF area)
            => new Vector2(area.X + NextFloat() * area.Width, area.Y + NextFloat() * area.Height);

        public Vector2 NextPointInCircle(Circle circle)
            => circle.Center + MathUtils.AngleToVector(NextAngle(), circle.Radius * (float)Math.Sqrt(random.NextDouble()));

        public Color NextColor(bool randomAlpha = false)
            => new Color(random.Next(256), random.Next(256), random.Next(256), randomAlpha ? random.Next(256) : 255);

        public T Pick<T>(IList<T> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("The list is empty.", nameof(items));
            return items[random.Next(items.Count)];
        }

        public T PickWeighted<T>(IList<T> items, Func<T, float> getWeight)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("The list is empty.", nameof(items));
            if (getWeight == null)
                throw new ArgumentNullException(nameof(getWeight));

            var total = 0f;
            foreach (var item in items)
                total += Math.Max(0f, getWeight(item));

            if (total <= 0f)
                return Pick(items);

            var target = NextFloat() * total;
            foreach (var item in items)
            {
                target -= Math.Max(0f, getWeight(item));
                if (target < 0f)
                    return item;
            }
            return items[items.Count - 1];
        }

        public void Shuffle<T>(IList<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                var temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
    }
}
