
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Models
{
    public class PaymentCompleteResult
    {
        public bool Success { get; set; }
        public TransactionErrorCode? ErrorCode { get; set; }
    }
}
