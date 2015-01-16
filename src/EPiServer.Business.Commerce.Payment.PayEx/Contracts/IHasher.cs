
using EPiServer.Business.Commerce.Payment.PayEx.Models;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IHasher
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
    }
}
