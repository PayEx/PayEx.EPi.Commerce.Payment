using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Contracts.Commerce
{
    internal interface IPaymentActions
    {
        void UpdatePaymentInformation(PaymentMethod paymentMethod, string authorizationCode, string paymentMethodCode);
        void UpdateConsumerInformation(PaymentMethod paymentMethod, ConsumerLegalAddressResult consumerLegalAddress);
        void SetPaymentProcessed(PaymentMethod currentPayment);
    }
}
