using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using EPiServer.Business.Commerce.Payment.PayEx.Price;
using log4net;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCreditors
{
    internal class CreditPayment : IPaymentCreditor
    {
        private readonly IPaymentCreditor _paymentCreditor;
        private readonly IParameterReader _parameterReader;
        private readonly IPaymentManager _paymentManager;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public CreditPayment(IPaymentCreditor paymentCreditor, IPaymentManager paymentManager, IParameterReader parameterReader)
        {
            _paymentCreditor = paymentCreditor;
            _parameterReader = parameterReader;
            _paymentManager = paymentManager;
        }

        public bool Credit(PaymentMethod currentPayment)
        {
            Mediachase.Commerce.Orders.Payment payment = (Mediachase.Commerce.Orders.Payment)currentPayment.Payment;

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.ErrorFormat("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id);
                return false;
            }

            long amount = payment.Amount.RoundToLong();
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            CreditResult result = _paymentManager.Credit(transactionId, amount,
                currentPayment.PurchaseOrder.TrackingNumber, vat, string.Empty);

            bool success = false;
            if (result.Success && !string.IsNullOrWhiteSpace(result.TransactionNumber))
            {
                payment.TransactionID = result.TransactionNumber;
                payment.AcceptChanges();
                success = true;
            }

            if (_paymentCreditor != null)
                return _paymentCreditor.Credit(currentPayment) && success;
            return success;
        }
    }
}
