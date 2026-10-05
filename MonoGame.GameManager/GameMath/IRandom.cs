using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Random values for games. Use a seeded instance to reproduce the same sequence (replays, tests, daily levels).
    /// </summary>
    public interface IRandom
    {
        /// <summary>An integer from <paramref name="minValue"/> to <paramref name="maxValueInclusive"/> (both included).</summary>
        int Next(int minValue, int maxValueInclusive);

        /// <summary>A double from 0 (included) to 1 (excluded).</summary>
        double NextDouble();

        /// <summary>A float from 0 (included) to 1 (excluded).</summary>
        float NextFloat();

        /// <summary>A float from <paramref name="minValue"/> (included) to <paramref name="maxValue"/> (excluded).</summary>
        float NextFloat(float minValue, float maxValue);

        bool NextBool();

        /// <summary>True with the given probability (from 0 to 1).</summary>
        bool Chance(float probability);

        /// <summary>An angle in radians from 0 to 2π.</summary>
        float NextAngle();

        /// <summary>A vector of length 1 in a random direction.</summary>
        Vector2 NextUnitVector();

        /// <summary>A random point inside a rectangle.</summary>
        Vector2 NextPointInRectangle(RectangleF area);

        /// <summary>A random point inside a circle (uniformly distributed).</summary>
        Vector2 NextPointInCircle(Circle circle);

        Color NextColor(bool randomAlpha = false);

        /// <summary>A random item of the list.</summary>
        T Pick<T>(IList<T> items);

        /// <summary>A random item of the list, where items with a higher weight are more likely.</summary>
        T PickWeighted<T>(IList<T> items, Func<T, float> getWeight);

        /// <summary>Shuffles the list in place (Fisher-Yates).</summary>
        void Shuffle<T>(IList<T> items);
    }
}
