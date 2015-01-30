
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    interface IPaymentActions
    {
        void UpdatePaymentInformation(PaymentMethod paymentMethod, string authorizationCode, string paymentMethodCode);
        void UpdateConsumerInformation(PaymentMethod paymentMethod, ConsumerLegalAddressResult consumerLegalAddress);
    }
}
