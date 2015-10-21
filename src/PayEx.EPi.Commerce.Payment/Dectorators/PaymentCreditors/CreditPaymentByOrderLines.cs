using log4net;
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
            Log.Info($"Crediting payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}, by order lines");

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.Error($"Could not get PayEx Transaction Id from purchase order with ID: {currentPayment.PurchaseOrder.Id}");
                return false;
            }
            Log.Info($"PayEx transaction ID is {transactionId} on payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");

            CreditResult result = null;
            string orderNumber = OrderNumberFormatter.MakeNumeric(currentPayment.PurchaseOrder.TrackingNumber);
            foreach (OrderForm orderForm in currentPayment.PurchaseOrder.OrderForms)
            {
                foreach (LineItem lineItem in orderForm.LineItems)
                {
                    result = _paymentManager.CreditOrderLine(transactionId, lineItem.CatalogEntryId, orderNumber);
                }
            }

            bool success = false;
            if (result != null && !string.IsNullOrWhiteSpace(result.TransactionNumber))
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
