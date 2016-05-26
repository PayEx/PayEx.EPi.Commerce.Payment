using System;
using EPiServer.Logging.Compatibility;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers
{
    internal class GetConsumerLegalAddress : IPaymentInitializer
    {
        private readonly IPaymentInitializer _paymentInitializer;
        private readonly IVerificationManager _verificationManager;
        private readonly IPaymentActions _paymentActions;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public GetConsumerLegalAddress(IPaymentInitializer paymentInitializer, IVerificationManager verificationManager, IPaymentActions paymentActions)
        {
            _paymentInitializer = paymentInitializer;
            _verificationManager = verificationManager;
            _paymentActions = paymentActions;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            Log.InfoFormat("Retrieving consumer legal address for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            CustomerDetails customerDetails = CreateModel(currentPayment);
            if (customerDetails == null)
                throw new Exception("Payment class must be ExtendedPayExPayment when using this payment method");

            ConsumerLegalAddressResult result = _verificationManager.GetConsumerLegalAddress(customerDetails.SocialSecurityNumber, customerDetails.CountryCode);
            if (!result.Status.Success)
                return new PaymentInitializeResult { ErrorMessage = result.Status.Description };

            _paymentActions.UpdateConsumerInformation(currentPayment, result);
            Log.InfoFormat("Successfully retrieved consumer legal address for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);

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
