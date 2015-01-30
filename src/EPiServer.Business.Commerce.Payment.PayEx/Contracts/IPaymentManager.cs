using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IPaymentManager
    {
        CompleteResult Complete(string orderRef);
        TransactionResult GetTransactionDetails(int transactionNumber);
        InitializeResult Initialize(Cart cart, PaymentInformation payment, bool ignoreOrderLines = false, bool ignoreCustomerAddress = false);
        PurchaseInvoiceSaleResult PurchaseInvoiceSale(string orderRef, CustomerDetails customerDetails);
        PurchasePartPaymentSaleResult PurchasePartPaymentSale(string orderRef, CustomerDetails customerDetails);
        CreditResult CreditOrderLine(int transactionNumber, string itemNumber, string orderId);
        CreditResult Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
        CaptureResult Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues);
    }
}
