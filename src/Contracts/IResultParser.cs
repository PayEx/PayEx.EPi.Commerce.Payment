using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Contracts
{
    public interface IResultParser
    {
        InitializeResult ParseInitializeXml(string xml);
        TransactionResult ParseTransactionXml(string xml);
    }
}
