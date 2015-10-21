using System;
using log4net;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

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
            Log.Info($"Retrieving consumer legal address for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            var customerDetails = CreateModel(currentPayment);
            if (customerDetails == null)
                throw new Exception("Payment class must be ExtendedPayExPayment when using this payment method");

            var result = _verificationManager.GetConsumerLegalAddress(customerDetails.SocialSecurityNumber, customerDetails.CountryCode);
            if (!result.Status.Success)
                return new PaymentInitializeResult { ErrorMessage = result.Status.Description };

            _paymentActions.UpdateConsumerInformation(currentPayment, result);
            Log.Info($"Successfully retrieved consumer legal address for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");

            return _paymentInitializer.Initialize(currentPayment, orderNumber, returnUrl, orderRef);
        }

        private CustomerDetails CreateModel(PaymentMethod currentPayment)
        {
            if (!(currentPayment.Payment is ExtendedPayExPayment))
                return null;

            var payment = currentPayment.Payment as ExtendedPayExPayment;
            return new CustomerDetails
            {
                SocialSecurityNumber = payment.SocialSecurityNumber,
                CountryCode = payment.CountryCode,
            };
        }
    }
}
