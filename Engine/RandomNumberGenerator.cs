using System;
using System.Security.Cryptography;

namespace Engine
{
    public static class RandomNumberGenerator
    {
        private static readonly RNGCryptoServiceProvider generator 
                                = new RNGCryptoServiceProvider();

        public static int NumberBetween(int minimumValue, int maximumValue)
        {
            byte[] randomNumber = new byte[1];

            generator.GetBytes(randomNumber);

            double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);

            // Используем Math.Max и вычитаем 0.00000000001, чтобы убедиться,
            // что наш множитель всегда будет между 0.0 и .99999999999
            // Иначе, из-за единицы в округлении могут возникуть проблемы
            double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);

            // Необходимо прибавить единицу, чтобы позволить округление, используя Math.Floor
            int range = maximumValue - minimumValue + 1;

            double randomValueInRange = Math.Floor(multiplier * range);

            return (int)(minimumValue + randomValueInRange);
        }
    }
}