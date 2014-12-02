
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IPaymentCreditor
    {
        bool Credit(PaymentMethod currentPayment);
    }
}
