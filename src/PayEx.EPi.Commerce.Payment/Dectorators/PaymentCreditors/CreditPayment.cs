using log4net;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentCreditors
{
    internal class CreditPayment : IPaymentCreditor
    {
        private readonly IPaymentCreditor _paymentCreditor;
        private readonly IPaymentManager _paymentManager;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public CreditPayment(IPaymentCreditor paymentCreditor, IPaymentManager paymentManager)
        {
            _paymentCreditor = paymentCreditor;
            _paymentManager = paymentManager;
        }

        public bool Credit(PaymentMethod currentPayment)
        {
            var payment = (Mediachase.Commerce.Orders.Payment)currentPayment.Payment;
            Log.Info($"Crediting payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.Error($"Could not get PayEx Transaction Id from purchase order with ID: {currentPayment.PurchaseOrder.Id}");
                return false;
            }
            Log.Info($"PayEx transaction ID is {transactionId} on payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");

            var amount = payment.Amount.RoundToLong();
            var orderNumber = OrderNumberFormatter.MakeNumeric(currentPayment.PurchaseOrder.TrackingNumber);
            var result = _paymentManager.Credit(transactionId, amount,
                orderNumber, currentPayment.Payment.Vat, string.Empty);

            var success = false;
            if (result.Success && !string.IsNullOrWhiteSpace(result.TransactionNumber))
            {
                Log.Info($"Setting PayEx transaction number to {result.TransactionNumber} on payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId} during credit");
                payment.TransactionID = result.TransactionNumber;
                payment.AcceptChanges();
                success = true;
                Log.Info($"Successfully credited payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            }

            if (_paymentCreditor != null)
                return _paymentCreditor.Credit(currentPayment) && success;
            return success;
        }
    }
}
