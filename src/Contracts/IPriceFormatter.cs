
namespace Epinova.PayExProvider.Contracts
{
    public interface IPriceFormatter
    {
        long RoundToLong(decimal price);
    }
}
