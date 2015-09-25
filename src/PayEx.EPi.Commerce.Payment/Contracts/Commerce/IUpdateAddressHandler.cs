using Mediachase.Commerce.Orders;

namespace PayEx.EPi.Commerce.Payment.Contracts.Commerce
{
    public interface IUpdateAddressHandler
    {
        void UpdateAddress(Cart cart, ExtendedPayExPayment payment);
    }
}
