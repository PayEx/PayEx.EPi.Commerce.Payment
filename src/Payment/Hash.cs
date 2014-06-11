using System.Security.Cryptography;
using System.Text;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Payment
{
    public class Hash : IHasher
    {
        public string Create(PaymentInformation payment)
        {
            var stringToHash = string.Concat(payment.AccountNumber, payment.PurchaseOperation, payment.Price, payment.PriceArgList, payment.Currency, payment.Vat, payment.OrderId, payment.OrderId,
                                payment.Description, payment.ClientIpAddress, payment.ClientIdentifier, payment.AdditionalValues, payment.ReturnUrl, payment.View,
                                payment.AgreementRef, payment.CancelUrl, payment.ClientLanguage, payment.EncryptionKey);

            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string orderRef, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderRef, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, encryptionKey);
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
