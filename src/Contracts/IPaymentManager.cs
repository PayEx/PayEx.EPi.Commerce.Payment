
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentManager
    {
        string Initialize(PaymentInformation payment);
        string Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
    }
}
