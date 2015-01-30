using System.Web;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.ServiceLocation;
using log4net;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using PaymentMethod = EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods.PaymentMethod;

namespace EPiServer.Business.Commerce.Payment.PayEx
{
    public class PayExPaymentGateway : AbstractPaymentGateway
    {
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);
        private readonly IPaymentMethodFactory _paymentMethodFactory;

        public PayExPaymentGateway()
        {
            _paymentMethodFactory = ServiceLocator.Current.GetInstance<IPaymentMethodFactory>();
        }

        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            if (HttpContext.Current == null)
            {
                Log.Warn("HttpContent.Current is null");
                return false;
            }

            PaymentMethod currentPayment = _paymentMethodFactory.Create(payment);
            if (currentPayment == null)
            {
                Log.Warn("Could not get PayEx payment method for current payment");
                return false;
            }

            if (currentPayment.IsPurchaseOrder)
            {
                // when user click complete order in commerce manager the transaction type will be Capture
                if (currentPayment.IsCapture)
                {
                    Log.DebugFormat("Begin CapturePayment for purchaseOrder with ID:{0}", currentPayment.PurchaseOrder.Id);
                    return currentPayment.Capture();
                }

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (currentPayment.IsCredit)
                {
                    Log.DebugFormat("Begin CreditPayment for purchaseOrder with ID:{0}", currentPayment.PurchaseOrder.Id);
                    return currentPayment.Credit();
                }

                return false;
            }

            // When "Complete" or "Refund" shipment in Commerce Manager, this method will be run again with the TransactionType is Capture/Credit
            // PayEx will always return true to bypass the payment process again.
            if (!currentPayment.IsAuthorization)
                return true;

            if (!currentPayment.IsCart)
                return false;

            PaymentInitializeResult result = currentPayment.Initialize();
            message = result.ErrorMessage ?? string.Empty;
            return result.Success;
        }

        public bool ProcessSuccessfulTransaction(PayExPayment payExPayment, string orderNumber, string orderRef, Cart cart, out string transactionErrorCode)
        {
            transactionErrorCode = null;

            PaymentMethod currentPayment = _paymentMethodFactory.Create(payExPayment);
            if (currentPayment == null)
            {
                Log.Warn("Could not get PayEx payment method for current payment");
                return false;
            }

            PaymentCompleteResult result = currentPayment.Complete(orderRef);
            transactionErrorCode = result.TransactionErrorCode;
            return result.Success;
        }
    }
}
