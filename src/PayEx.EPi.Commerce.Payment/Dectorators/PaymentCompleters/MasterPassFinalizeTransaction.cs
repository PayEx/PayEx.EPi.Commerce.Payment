using log4net;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentCompleters
{
    internal class MasterPassFinalizeTransaction : IPaymentCompleter
    {
        private readonly IPaymentCompleter _paymentCompleter;
        private readonly IPaymentManager _paymentManager;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public MasterPassFinalizeTransaction(IPaymentCompleter paymentCompleter, IPaymentManager paymentManager)
        {
            _paymentCompleter = paymentCompleter;
            _paymentManager = paymentManager;
        }

        public PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef)
        {
            var result = new PaymentCompleteResult() { Success = false }; 
            
            if (currentPayment.RequireAddressUpdate)
            {
                Log.Info("MasterPass is using best practice flow");
                Log.Info($"Finalizing transaction for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
                var total = currentPayment.Cart.Total.RoundToLong();
                var totalVat = currentPayment.Cart.TaxTotal.RoundToLong();
                var finalizeTransactionResult = _paymentManager.FinalizeTransaction(orderRef, total, totalVat,
                    currentPayment.Payment.ClientIpAddress);
                
                if (!finalizeTransactionResult.Success)
                {
                    Log.Info($"Finalize transaction failed for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}. Reason ErrorCode: {finalizeTransactionResult.Status.ErrorCode} Description: {finalizeTransactionResult.Status.Description}");
                    return result;
                }

                Log.Info($"Successfully called finalize transaction for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            }
            else
            {
                Log.Info("MasterPass is using redirect flow");
            }

            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);

            return result;
        }
    }
}
