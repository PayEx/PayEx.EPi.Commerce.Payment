
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IOrderFacade
    {
        string Initialize(long accountNumber, PaymentInformation payment, string hash);
        string AddOrderLine(long accountNumber, OrderLine orderLine, string hash);
        string AddOrderAddress(long accountNumber, PayExAddress address, string hash);
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

        string GetApprovedDeliveryAddress(long accountNumber, string orderRef, string hash);

        string FinalizeTransaction(long accountNumber, string orderRef, long amount, long vatAmount,
            string clientIpAddress, string hash);

        string GetAddressByPaymentMethod(long accountNumber, string paymentMethod, string ssn, string zipcode,
            string countryCode, string ipAddress, string hash);

        string PurchaseFinancingInvoice(long accountNumber, string orderRef, string paymentMethod,
            CustomerDetails customerDetails, string hash);

        string InvoiceLinkGet(long accountNumber, int transactionNumber, string hash);

        string PreparePurchaseSwish(long accountNumber, string orderRef, string msisdn, string ipAddress, string hash);
    }
}
