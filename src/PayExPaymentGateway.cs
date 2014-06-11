using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using StructureMap;
using System;
using PurchaseOrder = Mediachase.Commerce.Orders.PurchaseOrder;

namespace Epinova.PayExProvider
{
    public class PayExPaymentGateway : AbstractPaymentGateway
    {
        private readonly IPurchaseOrder _purchaseOrder;
        private readonly IOrderNote _orderNote;
        private readonly IPaymentManager _paymentManager;
        private readonly ISettings _settings;

        public PayExPaymentGateway()
            : this(ObjectFactory.GetInstance<ISettings>(), ObjectFactory.GetInstance<IPurchaseOrder>(), ObjectFactory.GetInstance<IOrderNote>(), ObjectFactory.GetInstance<IPaymentManager>())
        {}

        public PayExPaymentGateway(ISettings settings, IPurchaseOrder purchaseOrder, IOrderNote orderNote, IPaymentManager paymentManager)
        {
            _purchaseOrder = purchaseOrder;
            _orderNote = orderNote;
            _paymentManager = paymentManager;
            _settings = settings;
        }

        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            PurchaseOrder purchaseOrder = _purchaseOrder.Get(payment);
            if (purchaseOrder != null)
            {
                if (payment.TransactionType == TransactionType.Authorization.ToString())
                    return true;

                if (payment.TransactionType == TransactionType.Capture.ToString())
                {
                    int? transactionId = _orderNote.FindTransactionIdByTitle(purchaseOrder.OrderNotes, _settings.AuthorizationNoteTitle);
                    if (transactionId == null)
                    {
                        message = "Could not get PayEx Transaction Id from purchase order notes";
                        return false;
                    }

                    bool captured = CapturePayment(purchaseOrder, transactionId.Value);
                    message = transactionId.ToString();

                    return captured;
                }
                message = "The current payment method does not support this order type.";
                return false;
            }

            var cart = payment.Parent.Parent as Cart;
            if (cart != null && cart.Status == CartStatus.PaymentComplete.ToString())
            {
                //return true because this shopping cart has been paid already on PayEx
                return true;
            }
            return false;
        }

        private bool CapturePayment(PurchaseOrder purchaseOrder, int transactionId)
        {
            long amount = (long)(purchaseOrder.Total * 100);
            string orderId = purchaseOrder.TrackingNumber.Replace("PO", "");
            var useDefaultVat = purchaseOrder.MarketId == MarketId.Default;
            var vat = 0;//useDefaultVat ? PayExSettings.Instance.VAT : 0;

            string transactionNumber = _paymentManager.Capture(transactionId, amount, orderId, vat, string.Empty);

            if (!string.IsNullOrWhiteSpace(transactionNumber))
            {
                Mediachase.Commerce.Orders.OrderNote captureNote = _orderNote.Create(new Guid(), string.Concat(_settings.CaptureNoteMessage, transactionNumber),
                    _settings.CaptureNoteTitle, _settings.CaptureNoteTitle);

                purchaseOrder.OrderNotes.Add(captureNote);
                purchaseOrder.AcceptChanges();
                return true;
            }
            return false;
        }
    }
}
