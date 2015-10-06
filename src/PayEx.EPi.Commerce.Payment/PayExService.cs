using EPiServer.ServiceLocation;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment
{
    public class PayExService : IPayExService
    {
        private readonly IPaymentManager _paymentManager;

        public PayExService()
        {
            _paymentManager = ServiceLocator.Current.GetInstance<IPaymentManager>();
        }

        public DeliveryAddressResult GetDeliveryAddress(string orderRef)
        {
            return _paymentManager.GetApprovedDeliveryAddress(orderRef);
        }

        public InvoiceLinkResult GetInvoiceLinkForFinancingInvoicePurchase(int transactionNumber)
        {
            return _paymentManager.GetInvoiceLinkForFinancingInvoicePurchase(transactionNumber);
        }

        public CompleteResult Complete(string orderRef)
        {
            return _paymentManager.Complete(orderRef);
        }
    }
}
