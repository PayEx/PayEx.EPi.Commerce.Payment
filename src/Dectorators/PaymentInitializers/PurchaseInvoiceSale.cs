using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers
{
    public class PurchaseInvoiceSale : IPaymentInitializer
    {
        private readonly IPaymentManager _paymentManager;

        public PurchaseInvoiceSale(IPaymentManager paymentManager)
        {
            _paymentManager = paymentManager;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            CustomerDetails customerDetails = new CustomerDetails(); //TODO
            _paymentManager.PurchaseInvoiceSale(orderRef, customerDetails); // Check result
            return new PaymentInitializeResult {Success = true};
        }
    }
}
