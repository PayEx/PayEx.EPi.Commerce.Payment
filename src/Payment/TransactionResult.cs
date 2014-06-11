
namespace Epinova.PayExProvider.Payment
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
