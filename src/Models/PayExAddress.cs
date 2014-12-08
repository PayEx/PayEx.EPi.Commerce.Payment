
namespace EPiServer.Business.Commerce.Payment.PayEx.Models
{
    internal class PayExAddress
    {
        public long AccountNumber { get; private set; }
        public string OrderRef { get; private set; }
        public string EncryptionKey { get; private set; }
    
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }

        public PayExAddress(long accountNumber, string orderRef, string encryptionKey)
        {
            AccountNumber = accountNumber;
            OrderRef = orderRef;
            EncryptionKey = encryptionKey;

            BillingAddress = new Address();
            ShippingAddress = new Address();
        }
    }
}
