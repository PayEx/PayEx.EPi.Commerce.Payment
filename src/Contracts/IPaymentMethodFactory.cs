using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IPaymentMethodFactory
    {
        PaymentMethod Create(Mediachase.Commerce.Orders.Payment payment);
    }
}
