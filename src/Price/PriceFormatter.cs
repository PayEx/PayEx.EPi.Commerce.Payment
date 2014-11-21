using System;

namespace Epinova.PayExProvider.Price
{
    public static class PriceFormatter
    {
        public static int RoundToInt(this decimal price)
        {
            return (int)(price.Round() * 100);
        }

        public static long RoundToLong(this decimal price)
        {
            return (long)(price.Round() * 100);
        }

        private static decimal Round(this decimal price)
        {
            return Math.Round(price, MidpointRounding.AwayFromZero);
        }
    }
}
