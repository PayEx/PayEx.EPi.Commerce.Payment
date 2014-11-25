
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Models
{
    public class TransactionDetails
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PostNumber { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public TransactionDetails(TransactionResult result)
        {
        }
    }
}
