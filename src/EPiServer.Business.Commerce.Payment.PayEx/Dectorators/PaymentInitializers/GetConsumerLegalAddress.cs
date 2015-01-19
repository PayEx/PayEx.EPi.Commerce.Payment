using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers
{
    internal class GetConsumerLegalAddress : IPaymentInitializer
    {
        private readonly IPaymentInitializer _paymentInitializer;
        private readonly IVerificationManager _verificationManager;
        private readonly IPaymentActions _paymentActions;

        public GetConsumerLegalAddress(IPaymentInitializer paymentInitializer, IVerificationManager verificationManager, IPaymentActions paymentActions)
        {
            _paymentInitializer = paymentInitializer;
            _verificationManager = verificationManager;
            _paymentActions = paymentActions;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            CustomerDetails customerDetails = CreateModel(currentPayment);
            if (customerDetails == null)
                throw new Exception("Payment class must be ExtendedPayExPayment when using this payment method");

            ConsumerLegalAddressResult result = _verificationManager.GetConsumerLegalAddress(customerDetails.SocialSecurityNumber, customerDetails.CountryCode);
            if (!result.Status.Success)
                return new PaymentInitializeResult { ErrorMessage = result.Status.Description };

            _paymentActions.UpdateConsumerInformation(currentPayment, result);

            return _paymentInitializer.Initialize(currentPayment, orderNumber, returnUrl, orderRef);
        }

        private CustomerDetails CreateModel(PaymentMethod currentPayment)
        {
            if (!(currentPayment.Payment is ExtendedPayExPayment))
                return null;

            ExtendedPayExPayment payment = currentPayment.Payment as ExtendedPayExPayment;
            return new CustomerDetails
            {
                SocialSecurityNumber = payment.SocialSecurityNumber,
                CountryCode = payment.CountryCode,
            };
        }
    }
}
