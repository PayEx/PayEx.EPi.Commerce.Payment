
using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentCreditor
    {
        bool Credit(PaymentMethod currentPayment);
    }
}
