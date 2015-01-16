using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers
{
    internal class PurchasePartPaymentSale : IPaymentInitializer
    {
        private readonly IPaymentManager _paymentManager;
        private readonly IPaymentActions _paymentActions;

        public PurchasePartPaymentSale(IPaymentManager paymentManager, IPaymentActions paymentActions)
        {
            _paymentManager = paymentManager;
            _paymentActions = paymentActions;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            CustomerDetails customerDetails = CreateModel(currentPayment);
            if (customerDetails == null)
                throw new Exception("Payment class must be ExtendedPayExPayment when using this payment method");

            PurchasePartPaymentSaleResult result = _paymentManager.PurchasePartPaymentSale(orderRef, customerDetails);
            if (result == null || !result.Status.Success)
                return new PaymentInitializeResult { ErrorMessage = result != null ? result.Status.Description : string.Empty };

            _paymentActions.UpdatePaymentInformation(currentPayment, result.TransactionNumber, result.PaymentMethod);
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
