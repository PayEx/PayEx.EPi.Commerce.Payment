using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    public interface IOrderNumberGenerator
    {
        string Generate(Cart cart);
    }
}
