using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Contracts
{
    public interface IResultParser
    {
        T Deserialize<T>(string xml) where T : class;
       // InitializeResult ParseInitializeXml(string xml);
        TransactionResult ParseTransactionXml(string xml);
    }
}
