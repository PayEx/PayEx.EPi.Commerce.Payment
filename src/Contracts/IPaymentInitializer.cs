
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentInitializer
    {
        PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef);
    }
}
