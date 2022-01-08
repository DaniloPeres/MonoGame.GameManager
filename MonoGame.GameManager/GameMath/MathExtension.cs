using System;

namespace MonoGame.GameManager.GameMath
{
    public static class MathExtension
    {
        public static int CapValue(int value, int minValue, int maxValue) => Math.Min(maxValue, Math.Max(minValue, value));
        public static float CapValue(float value, float minValue, float maxValue) => Math.Min(maxValue, Math.Max(minValue, value));
        public static double CapValue(double value, double minValue, double maxValue) => Math.Min(maxValue, Math.Max(minValue, value));
    }
}
