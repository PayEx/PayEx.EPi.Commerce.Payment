using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using Mediachase.Data.Provider;
using PaymentMethod = EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods.PaymentMethod;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCompleters
{
    internal class CompletePayment : IPaymentCompleter
    {
        private IPaymentCompleter _paymentCompleter;
        private readonly IPaymentManager _paymentManager;
        private readonly ILogger _logger;

        public CompletePayment(IPaymentCompleter paymentCompleter, IPaymentManager paymentManager, ILogger logger)
        {
            _paymentCompleter = paymentCompleter;
            _paymentManager = paymentManager;
            _logger = logger;
        }

        public PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef)
        {
            CompleteResult completeResult = _paymentManager.Complete(orderRef);
            if (!completeResult.Success || string.IsNullOrWhiteSpace(completeResult.TransactionNumber))
                return new PaymentCompleteResult { TransactionErrorCode = completeResult.ErrorDetails != null ? completeResult.ErrorDetails.TransactionErrorCode : string.Empty };

            if (completeResult.GetTransactionDetails)
            {
                if (_paymentCompleter == null)
                    _paymentCompleter = new UpdateTransactionDetails(null, _paymentManager, _logger);
                _paymentCompleter = new UpdateTransactionDetails(_paymentCompleter, _paymentManager, _logger);
            }

            bool paymentInformationSet = SetPaymentInformation(currentPayment, completeResult);

            PaymentCompleteResult result = new PaymentCompleteResult { Success = true };
            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);
            result.Success = paymentInformationSet && result.Success;
            return result;
        }

        private bool SetPaymentInformation(PaymentMethod currentPayment, CompleteResult completeResult)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    ((Mediachase.Commerce.Orders.Payment)currentPayment.Payment).AuthorizationCode =
                        completeResult.TransactionNumber;
                    ((Mediachase.Commerce.Orders.Payment)currentPayment.Payment).AcceptChanges();
                    currentPayment.OrderGroup.OrderForms[0]["PaymentMethodCode"] = completeResult.PaymentMethod;
                    currentPayment.OrderGroup.AcceptChanges();
                    scope.Complete();
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Could not set payment information", e);
                return false;
            }
        }
    }
}
