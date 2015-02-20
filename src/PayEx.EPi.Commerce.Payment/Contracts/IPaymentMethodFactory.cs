using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IPaymentMethodFactory
    {
        PaymentMethod Create(Mediachase.Commerce.Orders.Payment payment);
    }
}
