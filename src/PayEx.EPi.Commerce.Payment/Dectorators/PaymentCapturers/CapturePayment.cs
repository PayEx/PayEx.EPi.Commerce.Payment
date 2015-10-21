using log4net;
using Mediachase.Commerce.Orders.Managers;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Formatters;
using PaymentMethod = PayEx.EPi.Commerce.Payment.Models.PaymentMethods.PaymentMethod;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentCapturers
{
    internal class CapturePayment : IPaymentCapturer
    {
        private readonly IPaymentCapturer _paymentCapturer;
        private readonly IPaymentManager _paymentManager;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public CapturePayment(IPaymentCapturer paymentCapturer, IPaymentManager paymentManager)
        {
            _paymentCapturer = paymentCapturer;
            _paymentManager = paymentManager;
        }

        public bool Capture(PaymentMethod currentPayment)
        {
            return Capture(currentPayment, string.Empty);
        }

        public bool Capture(PaymentMethod currentPayment, string additionalValues)
        {
            var payment = (Mediachase.Commerce.Orders.Payment) currentPayment.Payment;
            Log.Info($"Capturing payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.Error($"Could not get PayEx transaction ID from payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");
                return false;
            }
            Log.Info($"PayEx transaction ID is {transactionId} on payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");

            var amount = payment.Amount.RoundToLong();
            var orderNumber = OrderNumberFormatter.MakeNumeric(currentPayment.PurchaseOrder.TrackingNumber);
            var result = _paymentManager.Capture(transactionId, amount, orderNumber, currentPayment.Payment.Vat, additionalValues);

            var success = false;
            if (result.Success && !string.IsNullOrWhiteSpace(result.TransactionNumber))
            {
                Log.Info($"Setting PayEx transaction number to {result.TransactionNumber} on payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId} during capture");
                payment.ValidationCode = result.TransactionNumber;    
                PaymentStatusManager.ProcessPayment(payment);                
                payment.AcceptChanges();
                success = true;
                Log.Info($"Successfully captured payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            }

            if (_paymentCapturer != null)
                return _paymentCapturer.Capture(currentPayment) && success;
            return success;
        }
    }
}
