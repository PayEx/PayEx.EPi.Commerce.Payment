using PayEx.EPi.Commerce.Payment.Contracts.Commerce;

namespace PayEx.EPi.Commerce.Payment.Dectorators.AdditionalValuesFormatters
{
    internal class MasterPassAdditionalValuesFormatter : IAdditionalValuesFormatter
    {
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;

        public MasterPassAdditionalValuesFormatter(IAdditionalValuesFormatter additionalValuesFormatter)
        {
            _additionalValuesFormatter = additionalValuesFormatter;
        }

        public string Format(PayExPayment payExPayment)
        {
            var additionalValues = string.Empty;
            
            if (_additionalValuesFormatter != null)
                additionalValues = _additionalValuesFormatter.Format(payExPayment);

            Models.PaymentMethods.MasterPass.ValidateMasterPassAdditionalValues(additionalValues);

            if (!string.IsNullOrWhiteSpace(additionalValues))
                additionalValues += "&";

            additionalValues += "RESPONSIVE=1&USEMASTERPASS=1";
            return additionalValues;
        }
    }
}
