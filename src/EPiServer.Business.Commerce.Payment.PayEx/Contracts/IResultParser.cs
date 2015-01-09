
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IResultParser
    {
        T Deserialize<T>(string xml) where T : class;
    }
}
