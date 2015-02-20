using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    internal interface ICartActions
    {
        void UpdateCartInstanceId(Cart cart);
    }
}
