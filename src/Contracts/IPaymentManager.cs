using Epinova.PayExProvider.Models.Result;
using Mediachase.Commerce.Orders;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentManager
    {
        CompleteResult Complete(string orderRef);
        TransactionResult GetTransactionDetails(int transactionNumber);
        InitializeResult Initialize(Cart cart, PaymentInformation payment);
        void PurchaseInvoiceSale(string orderRef, InvoiceData invoiceData);
        CreditResult CreditOrderLine(int transactionNumber, string itemNumber, string orderId);
        CreditResult Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
        CaptureResult Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
    }
}
