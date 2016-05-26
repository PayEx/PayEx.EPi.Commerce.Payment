using System;
using System.Linq;
using EPiServer.Logging.Compatibility;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers
{
    internal class GetConsumerLegalAddressForFinancingInvoice : IPaymentInitializer
    {
        private readonly IPaymentInitializer _paymentInitializer;
        private readonly IPaymentManager _paymentManager;
        private readonly IPaymentActions _paymentActions;
        private readonly IUpdateAddressHandler _updateAddressHandler;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public GetConsumerLegalAddressForFinancingInvoice(IPaymentInitializer paymentInitializer, IPaymentActions paymentActions, IPaymentManager paymentManager, IUpdateAddressHandler updateAddressHandler)
        {
            _paymentInitializer = paymentInitializer;
            _paymentActions = paymentActions;
            _paymentManager = paymentManager;
            _updateAddressHandler = updateAddressHandler;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            if (currentPayment.RequireAddressUpdate)
            {
                Log.InfoFormat(
                    "Retrieving consumer legal address for payment with ID:{0} belonging to order with ID: {1}",
                    currentPayment.Payment.Id, currentPayment.OrderGroupId);
                CustomerDetails customerDetails = CreateModel(currentPayment);
                if (customerDetails == null)
                    throw new Exception("Payment class must be ExtendedPayExPayment when using this payment method");

                var result = _paymentManager.GetAddressByPaymentMethod(currentPayment.PaymentMethodCode,
                    customerDetails.SocialSecurityNumber, customerDetails.PostNumber, customerDetails.CountryCode,
                    customerDetails.IpAddress);
                if (!result.Status.Success)
                    return new PaymentInitializeResult {ErrorMessage = result.Status.Description};

                var convertToConsumerAddress = ConvertToConsumerAddress(result);
                _paymentActions.UpdateConsumerInformation(currentPayment, convertToConsumerAddress);

                var extendedPayment = currentPayment.Payment as ExtendedPayExPayment;
                _updateAddressHandler.UpdateAddress(currentPayment.Cart, extendedPayment);

                Log.InfoFormat(
                    "Successfully retrieved consumer legal address for payment with ID:{0} belonging to order with ID: {1}",
                    currentPayment.Payment.Id, currentPayment.OrderGroupId);
            }
            else
            {
                Log.InfoFormat(
                    "Payment method is configured to not use consumer legal address for payment with ID:{0} belonging to order with ID: {1}",
                    currentPayment.Payment.Id, currentPayment.OrderGroupId);                
            }

            return _paymentInitializer.Initialize(currentPayment, orderNumber, returnUrl, orderRef);
        }

        private static ConsumerLegalAddressResult ConvertToConsumerAddress(LegalAddressResult result)
        {
            string lastName = string.Empty;
            string[] names = result.Name.Split(' ');
            string firstName = names[0];
            if (names.Length > 1)
                lastName = string.Join(" ", names.Skip(1));


            ConsumerLegalAddressResult consumerLegalAddressResult = new ConsumerLegalAddressResult()
            {
                Status = result.Status,
                Address = result.StreetAddress,
                City = result.City,
                Country = result.CountryCode,
                FirstName = firstName,
                LastName = lastName,
                PostNumber = result.ZipCode
            };
            return consumerLegalAddressResult;
        }

        private CustomerDetails CreateModel(PaymentMethod currentPayment)
        {
            if (!(currentPayment.Payment is ExtendedPayExPayment))
                return null;

            ExtendedPayExPayment payment = currentPayment.Payment as ExtendedPayExPayment;
            return new CustomerDetails
            {
                SocialSecurityNumber = payment.SocialSecurityNumber,
                PostNumber = payment.PostNumber,
                IpAddress = payment.ClientIpAddress,
                CountryCode = payment.CountryCode,
            };
        }
    }
}
