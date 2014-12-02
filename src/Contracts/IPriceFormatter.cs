
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IPriceFormatter
    {
        long RoundToLong(decimal price);
    }
}
