using EPiServer.Logging.Compatibility;
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
            PaymentCompleteResult result = new PaymentCompleteResult() { Success = false }; 
            
            if (currentPayment.RequireAddressUpdate)
            {
                Log.Info("MasterPass is using best practice flow");
                Log.InfoFormat("Finalizing transaction for payment with ID:{0} belonging to order with ID: {1}",
                    currentPayment.Payment.Id, currentPayment.OrderGroupId);
                var total = currentPayment.Cart.Total.RoundToLong();
                var totalVat = currentPayment.Cart.TaxTotal.RoundToLong();
                var finalizeTransactionResult = _paymentManager.FinalizeTransaction(orderRef, total, totalVat,
                    currentPayment.Payment.ClientIpAddress);
                
                if (!finalizeTransactionResult.Success)
                {
                    Log.InfoFormat(
                        "Finalize transaction failed for payment with ID:{0} belonging to order with ID: {1}. Reason ErrorCode: {2} Description: {3}",
                        currentPayment.Payment.Id, currentPayment.OrderGroupId,
                        finalizeTransactionResult.Status.ErrorCode,
                        finalizeTransactionResult.Status.Description);
                    return result;
                }

                Log.InfoFormat(
                    "Successfully called finalize transaction for payment with ID:{0} belonging to order with ID: {1}",
                    currentPayment.Payment.Id, currentPayment.OrderGroupId);
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
