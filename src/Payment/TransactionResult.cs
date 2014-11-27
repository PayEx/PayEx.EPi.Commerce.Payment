
namespace Epinova.PayExProvider.Payment
{
    public class TransactionResult : ResultBase
    {
        public string TransactionNumber { get; set; }
        public string PaymentMethod { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
      //  public TransactionErrorCode TransactionErrorCode { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PostNumber { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public TransactionResult(bool success) : base(success)
        {
        }
    }
}
