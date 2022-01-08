using System;

namespace MonoGame.GameManager.GameMath
{
    public static class RandomGenerator
    {
        private static readonly Random random = new Random();

        public static int Random(int minValue, int maxValue) => random.Next(minValue, maxValue + 1);
        public static float Random(float minValue, float maxValue, int decimalNumbers = 2)
        {
            var minValueCalc = minValue;
            var maxValueCalc = maxValue;
            for (var i = 0; i < decimalNumbers; i++)
            {
                minValue *= 10;
                maxValue *= 10;
            }

            float result = Random((int)minValue, (int)maxValue);

            for (var i = 0; i < decimalNumbers; i++)
            {
                result /= 10;
            }
            
            return result;
        }
    }
}
