using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Common math helpers for games.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>The tolerance used by the approximate comparisons.</summary>
        public const float Epsilon = 1e-5f;

        public static float Lerp(float from, float to, float amount) => from + (to - from) * amount;

        /// <summary>The amount (usually from 0 to 1) that produces <paramref name="value"/> between from and to.</summary>
        public static float InverseLerp(float from, float to, float value)
            => Math.Abs(to - from) < Epsilon ? 0f : (value - from) / (to - from);

        /// <summary>Maps a value from one range to another.</summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
            => Lerp(toMin, toMax, InverseLerp(fromMin, fromMax, value));

        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
                Swap(ref min, ref max);
            return value < min ? min : value > max ? max : value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
                Swap(ref min, ref max);
            return value < min ? min : value > max ? max : value;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
                Swap(ref min, ref max);
            return value < min ? min : value > max ? max : value;
        }

        public static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

        /// <summary>Wraps an integer into [min, maxExclusive), eg: for positions on a looping board.</summary>
        public static int Wrap(int value, int min, int maxExclusive)
        {
            var range = maxExclusive - min;
            if (range <= 0)
                return min;
            var result = (value - min) % range;
            return (result < 0 ? result + range : result) + min;
        }

        /// <summary>Wraps a value into [min, max).</summary>
        public static float Wrap(float value, float min, float max)
        {
            var range = max - min;
            if (range <= 0f)
                return min;
            var result = (value - min) % range;
            return (result < 0f ? result + range : result) + min;
        }

        /// <summary>Wraps an angle into [-π, π].</summary>
        public static float WrapAngle(float radians) => MathHelper.WrapAngle(radians);

        /// <summary>The shortest difference between two angles, in radians.</summary>
        public static float DeltaAngle(float fromRadians, float toRadians) => MathHelper.WrapAngle(toRadians - fromRadians);

        /// <summary>Interpolates two angles through the shortest path.</summary>
        public static float LerpAngle(float fromRadians, float toRadians, float amount) => fromRadians + DeltaAngle(fromRadians, toRadians) * amount;

        /// <summary>The angle of the direction from one point to another, in radians.</summary>
        public static float AngleBetween(Vector2 from, Vector2 to) => (float)Math.Atan2(to.Y - from.Y, to.X - from.X);

        /// <summary>A vector with the given angle (in radians) and length.</summary>
        public static Vector2 AngleToVector(float radians, float length = 1f)
            => new Vector2((float)Math.Cos(radians) * length, (float)Math.Sin(radians) * length);

        public static bool Approximately(float a, float b, float epsilon = Epsilon) => Math.Abs(a - b) <= epsilon;

        /// <summary>Hermite interpolation between 0 and 1 when x goes from edge0 to edge1.</summary>
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            var t = Clamp01(InverseLerp(edge0, edge1, x));
            return t * t * (3f - 2f * t);
        }

        /// <summary>Moves a value towards a target without overshooting it.</summary>
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
                return target;
            return current + Math.Sign(target - current) * maxDelta;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }
    }
}
