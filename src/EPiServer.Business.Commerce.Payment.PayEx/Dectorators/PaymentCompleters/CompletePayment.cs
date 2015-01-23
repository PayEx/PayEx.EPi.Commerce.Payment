using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using PaymentMethod = EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods.PaymentMethod;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCompleters
{
    internal class CompletePayment : IPaymentCompleter
    {
        private IPaymentCompleter _paymentCompleter;
        private readonly IPaymentManager _paymentManager;
        private readonly IPaymentActions _paymentActions;
        private readonly ILogger _logger;

        public CompletePayment(IPaymentCompleter paymentCompleter, IPaymentManager paymentManager, IPaymentActions paymentActions, ILogger logger)
        {
            _paymentCompleter = paymentCompleter;
            _paymentManager = paymentManager;
            _paymentActions = paymentActions;
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

            _paymentActions.UpdatePaymentInformation(currentPayment, completeResult.TransactionNumber, completeResult.PaymentMethod);

            PaymentCompleteResult result = new PaymentCompleteResult { Success = true };
            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);
            return result;
        }
    }
}
