using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models.PaymentMethods;
using Epinova.PayExProvider.Price;

namespace Epinova.PayExProvider.Dectorators.PaymentCapturers
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
            int transactionId;
            Mediachase.Commerce.Orders.Payment payment = (Mediachase.Commerce.Orders.Payment) currentPayment.Payment;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                _logger.LogWarning(string.Format("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id));
                return false;
            }

            long amount = payment.Amount.RoundToLong();
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            string transactionNumber = _paymentManager.Capture(transactionId, amount, currentPayment.PurchaseOrder.TrackingNumber, vat, string.Empty);

            if (!string.IsNullOrWhiteSpace(transactionNumber))
            {
                payment.ValidationCode = transactionNumber;
                payment.AcceptChanges();
                return true;
            }
            return false;
        }
    }
}
