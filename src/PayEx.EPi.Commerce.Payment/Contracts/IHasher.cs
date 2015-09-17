
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IHasher
    {
        string Create(long accountNumber, PaymentInformation payment, string encryptionKey);
        string Create(long accountNumber, OrderLine orderLine, string encryptionKey);
        string Create(long accountNumber, PayExAddress address, string encryptionKey);
        string Create(long accountNumber, string orderRef, string encryptionKey);
        string Create(long accountNumber, int transactionNumber, string encryptionKey);

        string Create(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount,
            string additionalValues, string encryptionKey);

        string Create(long accountNumber, int transactionNumber, string itemNumber, string orderId, string encryptionKey);
        string Create(long accountNumber, string socialSecurityNumber, string countryCode, string encryptionKey);
        string Create(long accountNumber, string orderRef, CustomerDetails customerDetails, string encryptionKey);
        string Create(long accountNumber, string orderRef, string paymentMethod, CustomerDetails customerDetails, string encryptionKey);

        string Create(long accountNumber, string orderRef, long amount, long vatAmount, string clientIpAddress, string encryptionKey);

        string Create(long accountNumber, string paymentMethod, string ssn, string zipcode, string countryCode, string ipAddress, string encryptionKey);
    }
}
