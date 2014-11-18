using Mediachase.Commerce.Orders;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentManager
    {
        string Initialize(Cart cart, PaymentInformation payment, out string orderRef);
        CompleteResult Complete(string orderRef);
        string Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
    }
}
