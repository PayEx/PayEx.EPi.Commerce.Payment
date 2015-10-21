namespace PayEx.EPi.Commerce.Payment.Formatters
{
    internal static class OrderNumberFormatter
    {
        public static string MakeNumeric(string text)
        {
            var numericString = string.Empty;
            for (var i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                    numericString += text[i];
            }
            return numericString;
        }
    }
}
