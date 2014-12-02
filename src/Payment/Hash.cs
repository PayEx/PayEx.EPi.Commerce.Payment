using System.Security.Cryptography;
using System.Text;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;

namespace EPiServer.Business.Commerce.Payment.PayEx.Payment
{
    public class Hash : IHasher
    {
        public string Create(PaymentInformation payment)
        {
            var stringToHash = string.Concat(payment.AccountNumber, payment.PurchaseOperation, payment.Price, payment.PriceArgList, payment.Currency, payment.Vat, payment.OrderId, payment.ProductNumber,
                                payment.Description, payment.ClientIpAddress, payment.ClientIdentifier, payment.AdditionalValues, payment.ReturnUrl, payment.View,
                                payment.AgreementRef, payment.CancelUrl, payment.ClientLanguage, payment.EncryptionKey);

            return CreateHash(stringToHash);
        }

        public string Create(OrderLine orderLine)
        {
            var stringToHash = string.Concat(orderLine.AccountNumber, orderLine.OrderRef, orderLine.ItemNumber, orderLine.Description, orderLine.Description2, orderLine.Description3,
                orderLine.Description4, orderLine.Description5, orderLine.Quantity, orderLine.Amount, orderLine.VatAmount, orderLine.VatPercentage, orderLine.EncryptionKey);

            return CreateHash(stringToHash);
        }

        public string Create(PayExAddress address)
        {
            var stringToHash = string.Concat(address.AccountNumber, address.OrderRef, address.BillingAddress.FirstName,
                address.BillingAddress.LastName, address.BillingAddress.Line1,
                address.BillingAddress.Line2, address.BillingAddress.Line3, address.BillingAddress.PostCode,
                address.BillingAddress.City, address.BillingAddress.State, address.BillingAddress.Country,
                address.BillingAddress.CountryCode, address.BillingAddress.Email, address.BillingAddress.Phone,
                address.BillingAddress.Mobile, address.ShippingAddress.FirstName, address.ShippingAddress.LastName,
                address.ShippingAddress.Line1, address.ShippingAddress.Line2, address.ShippingAddress.Line3,
                address.ShippingAddress.PostCode, address.ShippingAddress.City, address.ShippingAddress.State,
                address.ShippingAddress.Country, address.ShippingAddress.CountryCode, address.ShippingAddress.Email,
                address.ShippingAddress.Phone, address.ShippingAddress.Mobile, address.EncryptionKey);

            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, int transactionNumber, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, transactionNumber, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string orderRef, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderRef, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string socialSecurityNumber, string countryCode, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, countryCode, socialSecurityNumber, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string orderRef, CustomerDetails customerDetails, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderRef, customerDetails.SocialSecurityNumber, customerDetails.FirstName, customerDetails.LastName, customerDetails.StreetAddress, customerDetails.CoAddress,
                customerDetails.PostNumber, customerDetails.City, customerDetails.CountryCode, customerDetails.Email, customerDetails.MobilePhone, customerDetails.IpAddress, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, int transactionNumber, string itemNumber, string orderId, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, transactionNumber, itemNumber, orderId, encryptionKey);
            return CreateHash(stringToHash);
        }

        private string CreateHash(string stringToHash)
        {
            var hash = new MD5CryptoServiceProvider();
            var data = hash.ComputeHash(Encoding.Default.GetBytes(stringToHash));
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}
