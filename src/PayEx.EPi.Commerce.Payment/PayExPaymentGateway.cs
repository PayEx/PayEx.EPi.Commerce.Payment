using System.Web;
using EPiServer.ServiceLocation;
using log4net;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using PayEx.EPi.Commerce.Payment.Contracts;

namespace PayEx.EPi.Commerce.Payment
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
            Log.Info($"Processing payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");

            if (HttpContext.Current == null)
            {
                Log.Error("HttpContent.Current is null");
                return false;
            }

            var currentPayment = _paymentMethodFactory.Create(payment);
            if (currentPayment == null)
            {
                Log.Error($"As the PaymentMethod for payment with ID:{payment.Id} could not be resolved, it cannot be processed by the PayEx Payment Provider!");
                return false;
            }
            Log.Info($"Successfully resolved the PaymentMethod for payment with ID:{payment.Id}. The PaymentMethodCode is {currentPayment.PaymentMethodCode}");

            if (currentPayment.IsPurchaseOrder)
            {
                Log.Info($"Payment with ID:{payment.Id} is a purchase order. It's transaction type is {payment.TransactionType}");

                // when user click complete order in commerce manager the transaction type will be Capture
                if (currentPayment.IsCapture)
                {
                    Log.Info($"Begin CapturePayment for payment with ID:{payment.Id}");
                    return currentPayment.Capture();
                }

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (currentPayment.IsCredit)
                {
                    Log.Info($"Begin CreditPayment for payment with ID:{payment.Id}");
                    return currentPayment.Credit();
                }

                Log.Error($"The transaction type for payment with ID:{payment.Id} is {payment.TransactionType}. The PayEx Payment Provider expected a Credit or Capture transaction type!");
                return false;
            }

            // When "Complete" or "Refund" shipment in Commerce Manager, this method will be run again with the TransactionType is Capture/Credit
            // PayEx will always return true to bypass the payment process again.
            if (!currentPayment.IsAuthorization)
            {
                Log.Info($"The transaction type for payment with ID:{payment.Id} is {payment.TransactionType}, meaning the payment process has already been run once.");
                return true;
            }

            if (!currentPayment.IsCart)
            {
                Log.Error($"Payment with ID:{payment.Id} is not a cart. That should not be possible at this stage!");
                return false;
            }

            Log.Info($"Initializing payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");
            var result = currentPayment.Initialize();
            message = result.ErrorMessage ?? string.Empty;

            if (!result.Success)
                Log.Error($"Could not initialize payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}. Message: {message}");
            else
                Log.Info($"Successfully initialized payment with ID:{payment.Id} belonging to order with ID: {payment.OrderGroupId}");

            return result.Success;
        }

        public bool ProcessSuccessfulTransaction(PayExPayment payExPayment, string orderNumber, string orderRef, Cart cart, out string transactionErrorCode)
        {
            transactionErrorCode = null;

            Log.Info($"Processing a transaction for payment with ID:{payExPayment.Id} belonging to order with ID: {payExPayment.OrderGroupId}. Order number: {orderNumber}. Order reference: {orderRef}");

            var currentPayment = _paymentMethodFactory.Create(payExPayment);
            if (currentPayment == null)
            {
                Log.Error($"As the PaymentMethod for payment with ID:{payExPayment.Id} could not be resolved, it cannot be processed by the PayEx Payment Provider!");
                return false;
            }

            Log.Info($"Completing payment with ID:{payExPayment.Id} belonging to order with ID: {payExPayment.OrderGroupId}");
            var result = currentPayment.Complete(orderRef);
            transactionErrorCode = result.TransactionErrorCode;

            if (!result.Success)
                Log.Error($"Could not complete payment with ID:{payExPayment.Id} belonging to order with ID: {payExPayment.OrderGroupId}. TransactionErrorCode: {transactionErrorCode}");
            else
                Log.Info($"Successfully completed payment with ID:{payExPayment.Id} belonging to order with ID: {payExPayment.OrderGroupId}");

            return result.Success;
        }
    }
}
