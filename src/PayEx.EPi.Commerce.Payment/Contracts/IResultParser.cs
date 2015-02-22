
namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IResultParser
    {
        T Deserialize<T>(string xml) where T : class;
    }
}
