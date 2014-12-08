
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;

namespace EPiServer.Business.Commerce.Payment.PayEx.Commerce
{
    internal class PurchaseOrder : IPurchaseOrder
    {
        public Mediachase.Commerce.Orders.PurchaseOrder Get(Mediachase.Commerce.Orders.Payment payment)
        {
            if (payment.Parent.Parent is Mediachase.Commerce.Orders.PurchaseOrder)
                return payment.Parent.Parent as Mediachase.Commerce.Orders.PurchaseOrder;
            return null;
        }
    }
}
