using System;

namespace PayEx.EPi.Commerce.Payment.Formatters
{
    internal static class OrderNumberFormatter
    {
        public static string MakeNumeric(string text)
        {
            string numericString = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                if (Char.IsDigit(text[i]))
                    numericString += text[i];
            }
            return numericString;
        }
    }
}
