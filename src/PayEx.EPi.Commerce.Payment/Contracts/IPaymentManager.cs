using Mediachase.Commerce.Orders;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Contracts
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
        DeliveryAddressResult GetApprovedDeliveryAddress(string orderRef);
        FinalizeTransactionResult FinalizeTransaction(string orderRef, long amount, long vatAmount,
            string clientIpAddress);
        LegalAddressResult GetAddressByPaymentMethod(string paymentMethod, string ssn, string zipcode,
            string countryCode, string ipAddress);
        PurchaseInvoiceSaleResult PurchaseFinancingInvoice(string orderRef, string paymentMethod, CustomerDetails customerDetails);
        InvoiceLinkResult GetInvoiceLinkForFinancingInvoicePurchase(int transactionNumber);
    }
}
