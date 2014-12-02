using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IPaymentMethodFactory
    {
        PaymentMethod Create(Mediachase.Commerce.Orders.Payment payment);
    }
}
