
using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Contracts
{
   public interface IPaymentCapturer
    {
       bool Capture(PaymentMethod currentPayment);
    }
}
