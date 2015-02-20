
namespace EPiServer.Business.Commerce.Payment.PayEx.Models
{
    public class PayExAddress
    {
        public string OrderRef { get; private set; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }

        public PayExAddress(string orderRef)
        {
            OrderRef = orderRef;

            BillingAddress = new Address();
            ShippingAddress = new Address();
        }
    }
}
