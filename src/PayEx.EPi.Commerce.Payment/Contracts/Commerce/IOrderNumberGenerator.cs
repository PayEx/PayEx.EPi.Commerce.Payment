using Mediachase.Commerce.Orders;

namespace PayEx.EPi.Commerce.Payment.Contracts.Commerce
{
    public interface IOrderNumberGenerator
    {
        /// <summary>
        /// Returns a generated ordernumber for the given cart
        /// </summary>
        string Generate(Cart cart);
    }
}
