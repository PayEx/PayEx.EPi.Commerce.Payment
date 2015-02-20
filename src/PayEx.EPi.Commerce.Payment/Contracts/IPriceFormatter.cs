
namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IPriceFormatter
    {
        long RoundToLong(decimal price);
    }
}
