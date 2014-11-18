using System;
using Epinova.PayExProvider.Contracts;

namespace Epinova.PayExProvider.Price
{
    public class PriceFormatter : IPriceFormatter
    {
        public int RoundToInt(decimal price)
        {
            return (int)(Round(price) * 100);
        }

        public long RoundToLong(decimal price)
        {
            return (long)(Round(price) * 100);
        }

        private decimal Round(decimal price)
        {
            return Math.Round(price, MidpointRounding.AwayFromZero);
        }
    }
}
