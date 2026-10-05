using System;

namespace MonoGame.GameManager.GameMath
{
    [Obsolete("Use MathUtils instead.")]
    public static class MathExtension
    {
        [Obsolete("Use MathUtils.Clamp instead.")]
        public static int CapValue(int value, int minValue, int maxValue) => MathUtils.Clamp(value, minValue, maxValue);

        [Obsolete("Use MathUtils.Clamp instead.")]
        public static float CapValue(float value, float minValue, float maxValue) => MathUtils.Clamp(value, minValue, maxValue);

        [Obsolete("Use MathUtils.Clamp instead.")]
        public static double CapValue(double value, double minValue, double maxValue) => MathUtils.Clamp(value, minValue, maxValue);
    }
}
