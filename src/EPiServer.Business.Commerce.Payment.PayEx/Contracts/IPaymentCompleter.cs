using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    interface IPaymentCompleter
    {
        PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef);
    }
}
