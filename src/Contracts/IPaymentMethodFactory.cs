using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentMethodFactory
    {
        PaymentMethod Create(Mediachase.Commerce.Orders.Payment payment);
    }
}
