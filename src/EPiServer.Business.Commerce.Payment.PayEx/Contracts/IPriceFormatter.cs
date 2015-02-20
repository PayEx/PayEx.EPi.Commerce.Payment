
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IPriceFormatter
    {
        long RoundToLong(decimal price);
    }
}
