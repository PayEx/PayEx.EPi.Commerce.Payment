
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IHasher
    {
        string Create(PaymentInformation payment);
        string Create(long accountNumber, string orderRef, string encryptionKey);

        string Create(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount,
            string additionalValues, string encryptionKey);
    }
}
