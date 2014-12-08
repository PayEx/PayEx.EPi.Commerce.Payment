
namespace EPiServer.Business.Commerce.Payment.PayEx.Models
{
    internal class PaymentCompleteResult
    {
        public bool Success { get; set; }
        public string TransactionErrorCode { get; set; }
    }
}
