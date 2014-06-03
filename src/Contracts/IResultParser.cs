using Epinova.PayExProvider.PayExResult;

namespace Epinova.PayExProvider.Contracts
{
    public interface IResultParser
    {
        InitializeResult ParseInitializeXml(string xml);
        TransactionResult ParseTransactionXml(string xml);
    }
}
