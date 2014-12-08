
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    internal interface IPurchaseOrder
    {
        Mediachase.Commerce.Orders.PurchaseOrder Get(Mediachase.Commerce.Orders.Payment payment);
    }
}
