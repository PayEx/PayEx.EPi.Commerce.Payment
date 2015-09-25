using PayEx.EPi.Commerce.Payment.Contracts.Commerce;

namespace PayEx.EPi.Commerce.Payment.Dectorators.AdditionalValuesFormatters
{
    internal class MasterPassAdditionalValuesFormatter : IAdditionalValuesFormatter
    {
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        private readonly bool _addShoppingCartXml;
        private readonly IMasterPassShoppingCartFormatter _masterPassShoppingCartFormatter;
        private const string ShoppingCartXmlParmeter = "SHOPPINGCARTXML={0}";

        public MasterPassAdditionalValuesFormatter(IAdditionalValuesFormatter additionalValuesFormatter, bool addShoppingCartXml, IMasterPassShoppingCartFormatter masterPassShoppingCartFormatter)
        {
            _additionalValuesFormatter = additionalValuesFormatter;
            _addShoppingCartXml = addShoppingCartXml;
            _masterPassShoppingCartFormatter = masterPassShoppingCartFormatter;
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

            if (_addShoppingCartXml)
                additionalValues += "&" + string.Format(ShoppingCartXmlParmeter, _masterPassShoppingCartFormatter.GenerateShoppingCartXmlString(payExPayment));

            return additionalValues;
        }
    }
}
