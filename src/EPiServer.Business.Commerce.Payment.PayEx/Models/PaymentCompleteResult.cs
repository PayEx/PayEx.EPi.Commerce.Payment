
namespace EPiServer.Business.Commerce.Payment.PayEx.Models
{
    public class PaymentCompleteResult
    {
        public bool Success { get; set; }
        public string TransactionErrorCode { get; set; }
    }
}
