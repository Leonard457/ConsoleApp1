using System;

namespace ConsoleApp1.LogicGame
{
    /// <summary>
    /// Универсальный генератор случайных чисел для всей игры.
    /// Используйте статические методы для получения случайных значений.
    /// </summary>
    public static class RandomNumberGenerator
    {
        private static readonly Random randomInstance = new Random();

        /// <summary>
        /// Возвращает случайное целое число в диапазоне [minValue, maxValue).
        /// </summary>
        public static int Next(int minValue, int maxValue)
        {
            return randomInstance.Next(minValue, maxValue);
        }

        /// <summary>
        /// Возвращает случайное число с плавающей точкой в диапазоне [0.0, 1.0).
        /// </summary>
        public static double NextDouble()
        {
            return randomInstance.NextDouble();
        }

        /// <summary>
        /// Возвращает случайное целое число в диапазоне [0, maxValue).
        /// </summary>
        public static int Next(int maxValue)
        {
            return randomInstance.Next(maxValue);
        }
    }
}
