using Epinova.PayExProvider.Models.Result;
using Epinova.PayExProvider.Payment;
using Mediachase.Commerce.Orders;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentManager
    {
        Models.Result.CompleteResult Complete(string orderRef);
        TransactionResult GetTransactionDetails(int transactionNumber);
        InitializeResult Initialize(Cart cart, PaymentInformation payment);
        string CreditOrderLine(int transactionNumber, string itemNumber, string orderId);
        string Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
        string Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
    }
}
