using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using log4net;
using Mediachase.Commerce.Orders;
using PaymentMethod = EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods.PaymentMethod;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCreditors
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

            int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                Log.ErrorFormat("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id);
                return false;
            }

            CreditResult result = null;
            foreach (OrderForm orderForm in currentPayment.PurchaseOrder.OrderForms)
            {
                foreach (LineItem lineItem in orderForm.LineItems)
                {
                    result = _paymentManager.CreditOrderLine(transactionId, lineItem.CatalogEntryId, currentPayment.PurchaseOrder.TrackingNumber);
                }
            }

            bool success = false;
            if (result != null && !string.IsNullOrWhiteSpace(result.TransactionNumber))
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
