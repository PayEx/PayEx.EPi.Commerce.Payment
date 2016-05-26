using EPiServer.Logging.Compatibility;
using Mediachase.Commerce.Orders;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models.Result;
using PaymentMethod = PayEx.EPi.Commerce.Payment.Models.PaymentMethods.PaymentMethod;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentCreditors
{
    internal class CreditPaymentByOrderLines : IPaymentCreditor
    {
        private readonly IPaymentCreditor _paymentCreditor;
        private readonly IPaymentManager _paymentManager;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public CreditPaymentByOrderLines(IPaymentCreditor paymentCreditor, IPaymentManager paymentManager)
        {
            _paymentCreditor = paymentCreditor;
            _paymentManager = paymentManager;
        }

        public bool Credit(PaymentMethod currentPayment)
        {
            Mediachase.Commerce.Orders.Payment payment = (Mediachase.Commerce.Orders.Payment)currentPayment.Payment;
            Log.InfoFormat("Crediting payment with ID:{0} belonging to order with ID: {1}, by order lines", payment.Id, payment.OrderGroupId);

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.ErrorFormat("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id);
                return false;
            }
            Log.InfoFormat("PayEx transaction ID is {0} on payment with ID:{1} belonging to order with ID: {2}", transactionId, payment.Id, payment.OrderGroupId);

            CreditResult result = null;
            string orderNumber = OrderNumberFormatter.MakeNumeric(currentPayment.PurchaseOrder.TrackingNumber);
            foreach (OrderForm orderForm in currentPayment.PurchaseOrder.OrderForms)
            {
                foreach (LineItem lineItem in orderForm.LineItems)
                {
                    result = _paymentManager.CreditOrderLine(transactionId, lineItem.Code, orderNumber);
                }
            }

            bool success = false;
            if (result != null && !string.IsNullOrWhiteSpace(result.TransactionNumber))
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
