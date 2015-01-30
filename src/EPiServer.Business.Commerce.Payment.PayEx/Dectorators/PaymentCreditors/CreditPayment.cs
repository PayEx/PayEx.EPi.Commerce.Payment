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
            Log.InfoFormat("Crediting payment with ID:{0} belonging to order with ID: {1}", payment.Id, payment.OrderGroupId);

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.ErrorFormat("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id);
                return false;
            }
            Log.InfoFormat("PayEx transaction ID is {0} on payment with ID:{1} belonging to order with ID: {2}", transactionId, payment.Id, payment.OrderGroupId);

            long amount = payment.Amount.RoundToLong();
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            CreditResult result = _paymentManager.Credit(transactionId, amount,
                currentPayment.PurchaseOrder.TrackingNumber, vat, string.Empty);

            bool success = false;
            if (result.Success && !string.IsNullOrWhiteSpace(result.TransactionNumber))
            {
                Log.InfoFormat("Setting PayEx transaction number to {0} on payment with ID:{1} belonging to order with ID: {2} during credit", result.TransactionNumber, payment.Id, payment.OrderGroupId);
                payment.TransactionID = result.TransactionNumber;
                payment.AcceptChanges();
                success = true;
                Log.InfoFormat("Successfully credited payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            }

            if (_paymentCreditor != null)
                return _paymentCreditor.Credit(currentPayment) && success;
            return success;
        }
    }
}
