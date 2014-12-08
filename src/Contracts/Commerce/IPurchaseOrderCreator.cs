
using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    public interface IPurchaseOrderCreator
    {
        bool CreatePurchaseOrder(Cart cart, Mediachase.Commerce.Orders.Payment payment);
    }
}
