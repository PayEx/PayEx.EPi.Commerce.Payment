
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    interface IResultParser
    {
        T Deserialize<T>(string xml) where T : class;
    }
}
