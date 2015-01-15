using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
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
    }
}
