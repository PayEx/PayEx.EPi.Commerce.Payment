using System;
using EPiServer.Logging.Compatibility;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers
{
    internal class PurchaseInvoiceSale : IPaymentInitializer
    {
        private readonly IPaymentManager _paymentManager;
        private readonly IPaymentActions _paymentActions;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public PurchaseInvoiceSale(IPaymentManager paymentManager, IPaymentActions paymentActions)
        {
            _paymentManager = paymentManager;
            _paymentActions = paymentActions;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            Log.InfoFormat("Calling PurchaseInvoiceSale for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            CustomerDetails customerDetails = CreateModel(currentPayment);
            if (customerDetails == null)
                throw new Exception("Payment class must be ExtendedPayExPayment when using this payment method");

            PurchaseInvoiceSaleResult result = _paymentManager.PurchaseInvoiceSale(orderRef, customerDetails);
            if (!result.Status.Success)
                return new PaymentInitializeResult { ErrorMessage = result.Status.Description };

            _paymentActions.UpdatePaymentInformation(currentPayment, result.TransactionNumber, result.PaymentMethod);

            Log.InfoFormat("Successfully called PurchaseInvoiceSale for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            return new PaymentInitializeResult { Success = true };
        }

        private CustomerDetails CreateModel(PaymentMethod currentPayment)
        {
            if (!(currentPayment.Payment is ExtendedPayExPayment))
                return null;

            ExtendedPayExPayment payment = currentPayment.Payment as ExtendedPayExPayment;

            return new CustomerDetails
            {
                SocialSecurityNumber = payment.SocialSecurityNumber,
                FirstName = payment.FirstName,
                LastName = payment.LastName,
                StreetAddress = payment.StreetAddress,
                City = payment.City,
                CoAddress = payment.CoAddress,
                CountryCode = payment.CountryCode,
                Email = payment.Email,
                IpAddress = currentPayment.Payment.ClientIpAddress,
                MobilePhone = payment.MobilePhone,
                PostNumber = payment.PostNumber,
            };
        }
    }
}
