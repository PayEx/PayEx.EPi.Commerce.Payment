using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using EPiServer.Business.Commerce.Payment.PayEx.Price;
using log4net;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCapturers
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
            Mediachase.Commerce.Orders.Payment payment = (Mediachase.Commerce.Orders.Payment) currentPayment.Payment;
            Log.InfoFormat("Capturing payment with ID:{0} belonging to order with ID: {1}", payment.Id, payment.OrderGroupId);

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.ErrorFormat("Could not get PayEx transaction ID from payment with ID:{0} belonging to order with ID: {1}", payment.Id, payment.OrderGroupId);
                return false;
            }
            Log.InfoFormat("PayEx transaction ID is {0} on payment with ID:{1} belonging to order with ID: {2}", transactionId, payment.Id, payment.OrderGroupId);

            long amount = payment.Amount.RoundToLong();
            CaptureResult result = _paymentManager.Capture(transactionId, amount, currentPayment.PurchaseOrder.TrackingNumber, 0, string.Empty);

            bool success = false;
            if (result.Success && !string.IsNullOrWhiteSpace(result.TransactionNumber))
            {
                Log.InfoFormat("Setting PayEx transaction number to {0} on payment with ID:{1} belonging to order with ID: {2} during capture", result.TransactionNumber, payment.Id, payment.OrderGroupId);
                payment.ValidationCode = result.TransactionNumber;
                payment.AcceptChanges();
                success = true;
                Log.InfoFormat("Successfully captured payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            }

            if (_paymentCapturer != null)
                return _paymentCapturer.Capture(currentPayment) && success;
            return success;
        }
    }
}
