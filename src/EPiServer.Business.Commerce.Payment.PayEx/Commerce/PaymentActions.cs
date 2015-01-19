using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using Mediachase.Data.Provider;

namespace EPiServer.Business.Commerce.Payment.PayEx.Commerce
{
    public class PaymentActions : IPaymentActions
    {
        private readonly ILogger _logger;

        public PaymentActions(ILogger logger)
        {
            _logger = logger;
        }

        public void UpdatePaymentInformation(PaymentMethod paymentMethod, string authorizationCode, string paymentMethodCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentMethodCode))
                    paymentMethodCode = paymentMethod.PaymentMethodCode;

                using (TransactionScope scope = new TransactionScope())
                {
                    ((Mediachase.Commerce.Orders.Payment)paymentMethod.Payment).AuthorizationCode = authorizationCode;
                    ((Mediachase.Commerce.Orders.Payment)paymentMethod.Payment).AcceptChanges();
                    paymentMethod.OrderGroup.OrderForms[0]["PaymentMethodCode"] = paymentMethodCode;
                    paymentMethod.OrderGroup.AcceptChanges();
                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(string.Format("Could not update payment information for orderForm with ID:{0}. AuthorizationCode:{1}. PaymentMethodCode:{2}", 
                    paymentMethod.OrderGroup.OrderForms[0].Id, authorizationCode, paymentMethodCode), e);
            }
        }

        public void UpdateConsumerInformation(PaymentMethod paymentMethod, ConsumerLegalAddressResult consumerLegalAddress)
        {
            if (!(paymentMethod.Payment is ExtendedPayExPayment))
                return;

            ExtendedPayExPayment payment = paymentMethod.Payment as ExtendedPayExPayment;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    payment.FirstName = consumerLegalAddress.FirstName;
                    payment.LastName = consumerLegalAddress.LastName;
                    payment.StreetAddress = consumerLegalAddress.Address;
                    payment.PostNumber = consumerLegalAddress.PostNumber;
                    payment.City = consumerLegalAddress.City;
                    payment.CountryCode = consumerLegalAddress.Country;
                    payment.AcceptChanges();
                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(string.Format("Could not update consumer information for payment with ID:{0}. ConsumerLegalAddressResult:{1}.",
                    payment.Id, consumerLegalAddress), e);
            }
        }
    }
}
