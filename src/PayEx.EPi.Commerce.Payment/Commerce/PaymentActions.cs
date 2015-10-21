using System;
using log4net;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Data.Provider;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Commerce
{
    internal class PaymentActions : IPaymentActions
    {
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public void UpdatePaymentInformation(PaymentMethod paymentMethod, string authorizationCode, string paymentMethodCode)
        {
            try
            {
                Log.Info($"Updating payment information for payment with ID:{paymentMethod.Payment.Id} belonging to order with ID: {paymentMethod.OrderGroupId}");
                if (string.IsNullOrWhiteSpace(paymentMethodCode))
                {
                    paymentMethodCode = paymentMethod.PaymentMethodCode;
                }

                using (var scope = new TransactionScope())
                {
                    Log.Info($"Setting authorization code:{authorizationCode} for payment with ID:{paymentMethod.Payment.Id} belonging to order with ID: {paymentMethod.OrderGroupId}");
                    Log.Info($"Setting payment method code:{paymentMethodCode} for payment with ID:{paymentMethod.Payment.Id} belonging to order with ID: {paymentMethod.OrderGroupId}");
                    ((Mediachase.Commerce.Orders.Payment)paymentMethod.Payment).AuthorizationCode = authorizationCode;
                    ((Mediachase.Commerce.Orders.Payment)paymentMethod.Payment).AcceptChanges();
                    paymentMethod.OrderGroup.OrderForms[0]["PaymentMethodCode"] = paymentMethodCode;
                    paymentMethod.OrderGroup.AcceptChanges();
                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not update payment information. See next log item for more information.", e);
                Log.Error($"Could not update payment information for orderForm with ID:{paymentMethod.OrderGroup.OrderForms[0].Id}. AuthorizationCode:{authorizationCode}. PaymentMethodCode:{paymentMethodCode}");
            }
        }

        public void UpdateConsumerInformation(PaymentMethod paymentMethod, ConsumerLegalAddressResult consumerLegalAddress)
        {
            Log.Info($"Updating consumer information for payment with ID:{paymentMethod.Payment.Id} belonging to order with ID: {paymentMethod.OrderGroupId}");
            if (!(paymentMethod.Payment is ExtendedPayExPayment))
            {
                Log.Error($"Payment with ID:{paymentMethod.Payment.Id} belonging to order with ID: {paymentMethod.OrderGroupId} is not an ExtendedPayExPayment, cannot update consumer information");
                return;
            }

            var payment = paymentMethod.Payment as ExtendedPayExPayment;
            try
            {
                using (var scope = new TransactionScope())
                {
                    payment.FirstName = consumerLegalAddress.FirstName;
                    payment.LastName = consumerLegalAddress.LastName;
                    payment.StreetAddress = consumerLegalAddress.Address;
                    payment.PostNumber = consumerLegalAddress.PostNumber;
                    payment.City = consumerLegalAddress.City;
                    payment.CountryCode = consumerLegalAddress.Country;
                    payment.AcceptChanges();
                    scope.Complete();
                    Log.Info($"Successfully updated consumer information for payment with ID:{paymentMethod.Payment.Id} belonging to order with ID: {paymentMethod.OrderGroupId}");
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not update consumer information. See next log item for more information", e);
                Log.Error($"Could not update consumer information for payment with ID:{payment.Id}. ConsumerLegalAddressResult:{consumerLegalAddress}.");
            }
        }

        public void SetPaymentProcessed(PaymentMethod paymentMethod)
        {
            var payment = (Mediachase.Commerce.Orders.Payment) paymentMethod.Payment;
            PaymentStatusManager.ProcessPayment(payment);
            payment.AcceptChanges();
            Log.Info($"Successfully set payment status to pros for payment with ID:{paymentMethod.Payment.Id} belonging to order with ID: {paymentMethod.OrderGroupId}");
        }
    }
}
