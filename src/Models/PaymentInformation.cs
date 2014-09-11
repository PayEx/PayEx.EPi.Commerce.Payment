
using System;
using Epinova.PayExProvider.Contracts;

namespace Epinova.PayExProvider.Models
{
    public class PaymentInformation
    {
        public long AccountNumber { get; set; }
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
        public string EncryptionKey { get; private set; }

        public PaymentInformation(decimal price, string priceArgList, string currency, int vat, string orderId, string productNumber, string description, string clientIpAddress, string clientIdentifier,
            string additionalValues, string returnUrl, string view, string agreementRef, string cancelUrl, string clientLanguage)
        {
            long lowestUnitPrice = FormatPrice(price);
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
            ClientIdentifier = clientIdentifier;
            AdditionalValues = additionalValues;
            ReturnUrl = string.Format(returnUrl, orderId);
            View = view;
            AgreementRef = agreementRef;
            CancelUrl = cancelUrl;
            ClientLanguage = clientLanguage;
        }

        public void AddSettings(IPayExSettings settings)
        {
            AccountNumber = settings.AccountNumber;
            PurchaseOperation = settings.PurchaseOperation;
            EncryptionKey = settings.EncryptionKey;
        }

        private long FormatPrice(decimal price)
        {
            decimal rounded = Math.Round(price, MidpointRounding.AwayFromZero);
            return (long)(rounded * 100);
        }
    }
}
