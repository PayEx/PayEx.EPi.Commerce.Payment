
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentCompleter
    {
        PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef);
    }
}
