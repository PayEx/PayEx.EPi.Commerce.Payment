using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using EPiServer.Business.Commerce.Payment.PayEx.Price;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCapturers
{
    public class CapturePayment : IPaymentCapturer
    {
        private readonly IPaymentCapturer _paymentCapturer;
        private readonly ILogger _logger;
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;

        public CapturePayment(IPaymentCapturer paymentCapturer, ILogger logger, IPaymentManager paymentManager, IParameterReader parameterReader)
        {
            _paymentCapturer = paymentCapturer;
            _logger = logger;
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
        }

        public bool Capture(PaymentMethod currentPayment)
        {
            Mediachase.Commerce.Orders.Payment payment = (Mediachase.Commerce.Orders.Payment) currentPayment.Payment;

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                _logger.LogError(string.Format("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id));
                return false;
            }

            long amount = payment.Amount.RoundToLong();
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            CaptureResult result = _paymentManager.Capture(transactionId, amount, currentPayment.PurchaseOrder.TrackingNumber, vat, string.Empty);

            bool success = false;
            if (result != null && !string.IsNullOrWhiteSpace(result.TransactionNumber))
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
