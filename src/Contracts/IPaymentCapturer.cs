
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
   public interface IPaymentCapturer
    {
       bool Capture(PaymentMethod currentPayment);
    }
}
