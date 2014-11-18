using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Factories;
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

            //_currentPaymentMethodId = payment.PaymentMethodId;

            if (currentPayment.IsPurchaseOrder)
            {
                // when user click complete order in commerce manager the transaction type will be Capture
                if (currentPayment.IsCapture)
                {
                    //try
                    //{
                    //    if (IsInvoicePayment(payment as PayExPayment))
                    //        return true;

                    //    _logger.LogDebug(string.Format("Begin CapturePayment for purchaseOrder with ID:{0}", currentPayment.PurchaseOrder.Id));
                    //    return CapturePayment(currentPayment.PurchaseOrder, payment as PayExPayment);
                    //}
                    //catch (Exception e)
                    //{
                    //    _logger.LogError(string.Format("Error in CapturePayment for purchaseOrder with ID:{0}", currentPayment.PurchaseOrder.Id), e);
                    //    return false;
                    //}
                }

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (currentPayment.IsCredit)
                {
                    return false; // Not implemented
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

            //try
            //{
            //    _logger.LogDebug(string.Format("Begin InitializePayment for cart with ID:{0}", cart.Id));
            //    return InitializePayment(cart, payment as PayExPayment);
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError("Error when initializing PayEx payment request", e);
            //    return false;
            //}
            return true;
        }
    }
}
