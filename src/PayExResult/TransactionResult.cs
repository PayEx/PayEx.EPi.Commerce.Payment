
namespace Epinova.PayExProvider.PayExResult
{
    public class TransactionResult : ResultBase
    {
        public string TransactionNumber { get; set; }
        public TransactionStatus TransactionStatus { get; set; }

        public TransactionResult(bool success) : base(success)
        {
        }
    }
}
