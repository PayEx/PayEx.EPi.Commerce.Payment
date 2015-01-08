
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IResultParser
    {
        T Deserialize<T>(string xml) where T : class;
    }
}
