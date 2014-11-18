using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentInitializerFactory
    {
        IPaymentInitializer Create(PaymentMethod payment);
    }
}
