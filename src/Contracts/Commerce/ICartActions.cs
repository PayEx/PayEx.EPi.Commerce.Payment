using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    public interface ICartActions
    {
        void UpdateCartInstanceId(Cart cart);
    }
}
