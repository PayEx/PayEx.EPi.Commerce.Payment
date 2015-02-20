using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IPaymentCompleter
    {
        PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef);
    }
}
