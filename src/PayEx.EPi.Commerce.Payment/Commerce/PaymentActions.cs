using System;
using EPiServer.Logging.Compatibility;
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
                Log.InfoFormat("Updating payment information for payment with ID:{0} belonging to order with ID: {1}", paymentMethod.Payment.Id, paymentMethod.OrderGroupId);
                if (string.IsNullOrWhiteSpace(paymentMethodCode))
                {
                    paymentMethodCode = paymentMethod.PaymentMethodCode;
                }

                using (TransactionScope scope = new TransactionScope())
                {
                    Log.InfoFormat("Setting authorization code:{0} for payment with ID:{1} belonging to order with ID: {2}", authorizationCode, paymentMethod.Payment.Id, paymentMethod.OrderGroupId);
                    Log.InfoFormat("Setting payment method code:{0} for payment with ID:{1} belonging to order with ID: {2}", paymentMethodCode, paymentMethod.Payment.Id, paymentMethod.OrderGroupId);
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
                Log.ErrorFormat("Could not update payment information for orderForm with ID:{0}. AuthorizationCode:{1}. PaymentMethodCode:{2}",
                    paymentMethod.OrderGroup.OrderForms[0].Id, authorizationCode, paymentMethodCode);
            }
        }

        public void UpdateConsumerInformation(PaymentMethod paymentMethod, ConsumerLegalAddressResult consumerLegalAddress)
        {
            Log.InfoFormat("Updating consumer information for payment with ID:{0} belonging to order with ID: {1}", paymentMethod.Payment.Id, paymentMethod.OrderGroupId);
            if (!(paymentMethod.Payment is ExtendedPayExPayment))
            {
                Log.ErrorFormat("Payment with ID:{0} belonging to order with ID: {1} is not an ExtendedPayExPayment, cannot update consumer information", paymentMethod.Payment.Id, paymentMethod.OrderGroupId);
                return;
            }

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
                    Log.InfoFormat("Successfully updated consumer information for payment with ID:{0} belonging to order with ID: {1}", paymentMethod.Payment.Id, paymentMethod.OrderGroupId);
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not update consumer information. See next log item for more information", e);
                Log.ErrorFormat(
                    "Could not update consumer information for payment with ID:{0}. ConsumerLegalAddressResult:{1}.",
                    payment.Id, consumerLegalAddress);
            }
        }

        public void SetPaymentProcessed(PaymentMethod paymentMethod)
        {
            var payment = (Mediachase.Commerce.Orders.Payment) paymentMethod.Payment;
            PaymentStatusManager.ProcessPayment(payment);
            payment.AcceptChanges();
            Log.InfoFormat("Successfully set payment status to pros for payment with ID:{0} belonging to order with ID: {1}", paymentMethod.Payment.Id, paymentMethod.OrderGroupId);
        }
    }
}
