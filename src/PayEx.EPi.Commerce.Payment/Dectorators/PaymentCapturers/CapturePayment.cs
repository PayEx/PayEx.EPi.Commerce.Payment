using System;
using EPiServer.Logging.Compatibility;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models.Result;
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
            string orderNumber = OrderNumberFormatter.MakeNumeric(currentPayment.PurchaseOrder.TrackingNumber);
            CaptureResult result = _paymentManager.Capture(transactionId, amount, orderNumber, currentPayment.Payment.Vat, additionalValues);

            bool success = false;
            if (result.Success && !string.IsNullOrWhiteSpace(result.TransactionNumber))
            {
                Log.InfoFormat("Setting PayEx transaction number to {0} on payment with ID:{1} belonging to order with ID: {2} during capture", result.TransactionNumber, payment.Id, payment.OrderGroupId);
                payment.ValidationCode = result.TransactionNumber;    
                PaymentStatusManager.ProcessPayment(payment);                
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
