using Epinova.PayExProvider.Payment;
using Mediachase.Commerce.Orders;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentManager
    {
        InitializeResult Initialize(Cart cart, PaymentInformation payment);
        CompleteResult Complete(string orderRef);
        string Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
        TransactionResult GetTransactionDetails(int transactionNumber);
    }
}
