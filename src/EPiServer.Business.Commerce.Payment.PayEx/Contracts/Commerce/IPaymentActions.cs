
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    public interface IPaymentActions
    {
        void UpdatePaymentInformation(PaymentMethod paymentMethod, string authorizationCode, string paymentMethodCode);
    }
}
