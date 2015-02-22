
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IPaymentCapturer
    {
       bool Capture(PaymentMethod currentPayment);
    }
}
