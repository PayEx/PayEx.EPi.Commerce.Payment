
using EPiServer.Business.Commerce.Payment.PayEx.Models;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IOrderFacade
    {
        string Initialize(PaymentInformation payment, string hash);
        string AddOrderLine(OrderLine orderLine, string hash);
        string AddOrderAddress(PayExAddress address, string hash);
        string Complete(long accountNumber, string orderRef, string hash);

        string Capture(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount,
            string additionalValues, string hash);

        string GetTransactionDetails(long accountNumber, int transactionNumber, string hash);

        string Credit(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount,
            string additionalValues, string hash);

        string CreditOrderLine(long accountNumber, int transactionNumber, string itemNumber, string orderId, string hash);

        string PurchaseInvoiceSale(long accountNumber, string orderRef, CustomerDetails customerDetails, string hash);

        string PurchasePartPaymentSale(long accountNumber, string orderRef, CustomerDetails customerDetails,
            string hash);
    }
}
