using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Factories;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Payment;
using Mediachase.Commerce.Plugins.Payment;
using System.Web;
using PaymentMethod = Epinova.PayExProvider.Models.PaymentMethods.PaymentMethod;

namespace Epinova.PayExProvider
{
    public class NewPaymentGateway : AbstractPaymentGateway
    {
        private readonly ILogger _logger;
        private readonly IPaymentMethodFactory _paymentMethodFactory;

        public NewPaymentGateway()
        {
            _logger = new Logger();
            _paymentMethodFactory = new PaymentMethodFactory();
        }

        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            if (HttpContext.Current == null)
            {
                _logger.LogWarning("HttpContent.Current is null");
                return false;
            }

            PaymentMethod currentPayment = _paymentMethodFactory.Create(payment);
            if (currentPayment == null)
            {
                _logger.LogWarning("Could not get PayEx payment method for current payment");
                return false;
            }

            if (currentPayment.IsPurchaseOrder)
            {
                // when user click complete order in commerce manager the transaction type will be Capture
                if (currentPayment.IsCapture)
                {
                    _logger.LogDebug(string.Format("Begin CapturePayment for purchaseOrder with ID:{0}", currentPayment.PurchaseOrder.Id));
                    return currentPayment.Capture();
                }

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (currentPayment.IsCredit)
                {
                    _logger.LogDebug(string.Format("Begin CreditPayment for purchaseOrder with ID:{0}", currentPayment.PurchaseOrder.Id));
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

            if (currentPayment.Cart.Status == CartStatus.PaymentComplete.ToString())
                return true; // return true because this shopping cart has been paid already on PayEx

            PaymentInitializeResult result = currentPayment.Initialize();
            return result.Success;
        }

        public bool CompleteTransaction(PayExPayment payExPayment, string orderRef, out string transactionErrorCode)
        {
            PaymentMethod currentPayment = _paymentMethodFactory.Create(payExPayment);
            if (currentPayment == null)
            {
                _logger.LogWarning("Could not get PayEx payment method for current payment");
                transactionErrorCode = null;
                return false;
            }

            PaymentCompleteResult result = currentPayment.Complete(orderRef);
            transactionErrorCode = result.TransactionErrorCode;
            return result.Success;
        }
    }
}
