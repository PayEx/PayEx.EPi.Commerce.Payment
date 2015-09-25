using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    public interface IPayExService
    {
        DeliveryAddressResult GetDeliveryAddress(string orderRef);
        InvoiceLinkResult GetInvoiceLinkForFinancingInvoicePurchase(int transactionNumber);
    }
}
