using System;

namespace PayEx.EPi.Commerce.Payment.Formatters
{
    internal static class PriceFormatter
    {
        public static int RoundToInt(this decimal price)
        {
            return (int) ((price*100).Round());
        }

        public static long RoundToLong(this decimal price)
        {
            return (long) ((price*100).Round());
        }

        private static decimal Round(this decimal price)
        {
            return Math.Round(price, MidpointRounding.AwayFromZero);
        }

        public static decimal RoundToTwoDecimal(this decimal price)
        {
            return ((price * 100).Round()) / 100;
        }

    }
}