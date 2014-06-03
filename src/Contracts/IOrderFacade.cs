
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IOrderFacade
    {
        string Initialize(Payment payment, string hash);

        string Complete(long accountNumber, string orderRef, string hash);

        string Capture(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount,
            string additionalValues, string hash);
    }
}
