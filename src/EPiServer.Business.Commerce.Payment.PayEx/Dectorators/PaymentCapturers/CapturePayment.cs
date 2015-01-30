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
        private readonly IParameterReader _parameterReader;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public CapturePayment(IPaymentCapturer paymentCapturer, IPaymentManager paymentManager, IParameterReader parameterReader)
        {
            _paymentCapturer = paymentCapturer;
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
        }

        public bool Capture(PaymentMethod currentPayment)
        {
            Mediachase.Commerce.Orders.Payment payment = (Mediachase.Commerce.Orders.Payment) currentPayment.Payment;

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.ErrorFormat("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id);
                return false;
            }

            long amount = payment.Amount.RoundToLong();
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            CaptureResult result = _paymentManager.Capture(transactionId, amount, currentPayment.PurchaseOrder.TrackingNumber, vat, string.Empty);

            bool success = false;
            if (result.Success && !string.IsNullOrWhiteSpace(result.TransactionNumber))
            {
                payment.ValidationCode = result.TransactionNumber;
                payment.AcceptChanges();
                success = true;
            }

            if (_paymentCapturer != null)
                return _paymentCapturer.Capture(currentPayment) && success;
            return success;
        }
    }
}
