using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Data.Provider;
using System;

namespace EPiServer.Business.Commerce.Payment.PayEx.Commerce
{
    public class PurchaseOrderCreator : IPurchaseOrderCreator
    {
        private readonly ILogger _logger;
        private readonly IOrderNumberGenerator _orderNumberGenerator;

        public PurchaseOrderCreator(ILogger logger, IOrderNumberGenerator orderNumberGenerator)
        {
            _logger = logger;
            _orderNumberGenerator = orderNumberGenerator;
        }

        public bool CreatePurchaseOrder(Cart cart, Mediachase.Commerce.Orders.Payment payment)
        {
            var orderNumber = _orderNumberGenerator.GenerateOrderNumber(cart);

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    PaymentStatusManager.ProcessPayment(payment);

                    cart.OrderNumberMethod = c => orderNumber;
                    Mediachase.Commerce.Orders.PurchaseOrder purchaseOrder = cart.SaveAsPurchaseOrder();

                    cart.Delete();
                    cart.AcceptChanges();

                    purchaseOrder.AcceptChanges();
                    scope.Complete();
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Error in CreatePurchaseOrder", e);
                return false;
            }
        }
    }
}
