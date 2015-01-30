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
            Log.InfoFormat("Processing payment with ID:{0} belonging to order with ID: {1}", payment.Id, payment.OrderGroupId);

            if (HttpContext.Current == null)
            {
                Log.ErrorFormat("HttpContent.Current is null");
                return false;
            }

            PaymentMethod currentPayment = _paymentMethodFactory.Create(payment);
            if (currentPayment == null)
            {
                Log.ErrorFormat("As the PaymentMethod for payment with ID:{0} could not be resolved, it cannot be processed by the PayEx Payment Provider!", payment.Id);
                return false;
            }
            Log.InfoFormat("Successfully resolved the PaymentMethod for payment with ID:{0}. The PaymentMethodCode is {1}", payment.Id, currentPayment.PaymentMethodCode);

            if (currentPayment.IsPurchaseOrder)
            {
                Log.InfoFormat("Payment with ID:{0} is a purchase order. It's transaction type is {1}", payment.Id, payment.TransactionType);

                // when user click complete order in commerce manager the transaction type will be Capture
                if (currentPayment.IsCapture)
                {
                    Log.InfoFormat("Begin CapturePayment for payment with ID:{0}", payment.Id);
                    return currentPayment.Capture();
                }

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (currentPayment.IsCredit)
                {
                    Log.InfoFormat("Begin CreditPayment for payment with ID:{0}", payment.Id);
                    return currentPayment.Credit();
                }

                Log.ErrorFormat("The transaction type for payment with ID:{0} is {1}. The PayEx Payment Provider expected a Credit or Capture transaction type!", payment.Id, payment.TransactionType);
                return false;
            }

            // When "Complete" or "Refund" shipment in Commerce Manager, this method will be run again with the TransactionType is Capture/Credit
            // PayEx will always return true to bypass the payment process again.
            if (!currentPayment.IsAuthorization)
            {
                Log.InfoFormat("The transaction type for payment with ID:{0} is {1}, meaning the payment process has already been run once.", payment.Id, payment.TransactionType);
                return true;
            }

            if (!currentPayment.IsCart)
            {
                Log.ErrorFormat("Payment with ID:{0} is not a cart. That should not be possible at this stage!", payment.Id);
                return false;
            }

            Log.InfoFormat("Initializing payment with ID:{0} belonging to order with ID: {1}", payment.Id, payment.OrderGroupId);
            PaymentInitializeResult result = currentPayment.Initialize();
            message = result.ErrorMessage ?? string.Empty;

            if (!result.Success)
                Log.ErrorFormat("Could not initialize payment with ID:{0} belonging to order with ID: {1}. Message: {2}", payment.Id, payment.OrderGroupId, message);
            else
                Log.InfoFormat("Successfully initialized payment with ID:{0} belonging to order with ID: {1}", payment.Id, payment.OrderGroupId);

            return result.Success;
        }

        public bool ProcessSuccessfulTransaction(PayExPayment payExPayment, string orderNumber, string orderRef, Cart cart, out string transactionErrorCode)
        {
            transactionErrorCode = null;

            Log.InfoFormat("Processing a transaction for payment with ID:{0} belonging to order with ID: {1}. Order number: {2}. Order reference: {3}", 
                payExPayment.Id, payExPayment.OrderGroupId, orderNumber, orderRef);

            PaymentMethod currentPayment = _paymentMethodFactory.Create(payExPayment);
            if (currentPayment == null)
            {
                Log.ErrorFormat("As the PaymentMethod for payment with ID:{0} could not be resolved, it cannot be processed by the PayEx Payment Provider!", payExPayment.Id);
                return false;
            }

            Log.InfoFormat("Completing payment with ID:{0} belonging to order with ID: {1}", payExPayment.Id, payExPayment.OrderGroupId);
            PaymentCompleteResult result = currentPayment.Complete(orderRef);
            transactionErrorCode = result.TransactionErrorCode;

            if (!result.Success)
                Log.ErrorFormat("Could not complete payment with ID:{0} belonging to order with ID: {1}. TransactionErrorCode: {2}", payExPayment.Id, payExPayment.OrderGroupId, transactionErrorCode);
            else
                Log.InfoFormat("Successfully completed payment with ID:{0} belonging to order with ID: {1}", payExPayment.Id, payExPayment.OrderGroupId);

            return result.Success;
        }
    }
}
