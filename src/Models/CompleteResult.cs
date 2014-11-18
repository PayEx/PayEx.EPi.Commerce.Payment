
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Models
{
    public class CompleteResult
    {
        public bool Error { get { return ErrorCode.HasValue; } }

        public string TransactionNumber { get; private set; }
        public string PaymentMethod { get; private set; }
        public TransactionErrorCode? ErrorCode { get; private set; }

        public CompleteResult(string transactionNumber, string paymentMethod)
        {
            TransactionNumber = transactionNumber;
            PaymentMethod = paymentMethod;
        }

        public CompleteResult(string transactionNumber, string paymentMethod, TransactionErrorCode errorCode)
        {
            TransactionNumber = transactionNumber;
            PaymentMethod = paymentMethod;
            ErrorCode = errorCode;
        }
    }
}
