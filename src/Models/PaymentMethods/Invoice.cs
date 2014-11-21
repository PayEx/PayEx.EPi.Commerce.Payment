
namespace Epinova.PayExProvider.Models.PaymentMethods
{
    public class Invoice : PaymentMethod
    {
        public Invoice()
        {
        }

        public Invoice(Mediachase.Commerce.Orders.Payment payment)
            : base(payment)
        {
        }

        public override PaymentInitializeResult Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            throw new System.NotImplementedException();
        }

        public override bool Capture()
        {
            throw new System.NotImplementedException();
        }
    }
}
