
using Newtonsoft.Json;

namespace PayEx.EPi.Commerce.Payment.Models
{
    public class PaymentInformation
    {
        public string PurchaseOperation { get; set; }
        public long Price { get; private set; }
        public string PriceArgList { get; set; }
        public string Currency { get; private set; }
        public int Vat { get; private set; }
        public string OrderId { get; private set; }
        public string ProductNumber { get; private set; }
        public string Description { get; private set; }
        public string ClientIpAddress { get; private set; }
        public string ClientIdentifier { get; private set; }
        public string AdditionalValues { get; private set; }
        public string ReturnUrl { get; private set; }
        public string View { get; private set; }
        public string AgreementRef { get; private set; }
        public string CancelUrl { get; private set; }
        public string ClientLanguage { get; private set; }

        public PaymentInformation(long lowestUnitPrice, string priceArgList, string currency, int vat, string orderId, string productNumber, string description, string clientIpAddress,
            string additionalValues, string returnUrl, string view, string cancelUrl, string clientLanguage, string purchaseOperation)
        {
            if (!string.IsNullOrWhiteSpace(priceArgList))
            {
                Price = 0;
                PriceArgList = string.Format(priceArgList, lowestUnitPrice);
            }
            else
            {
                Price = lowestUnitPrice;
                PriceArgList = string.Empty;
            }

            Currency = currency;
            Vat = vat;
            OrderId = orderId;
            ProductNumber = productNumber;
            Description = description;
            ClientIpAddress = clientIpAddress;
            ClientIdentifier = string.Empty; // The information in this field is only used if you are implementing Credit Card in the direct model. And the direct model is not supported by this provider
            AdditionalValues = additionalValues;
            ReturnUrl = string.Format(returnUrl, orderId);
            View = view;
            AgreementRef = string.Empty; // The provider does not support recurring payments
            CancelUrl = cancelUrl;
            ClientLanguage = clientLanguage;
            PurchaseOperation = purchaseOperation;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
