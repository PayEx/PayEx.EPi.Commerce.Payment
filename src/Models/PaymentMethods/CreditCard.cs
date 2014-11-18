
namespace Epinova.PayExProvider.Models.PaymentMethods
{
    public class CreditCard : PaymentMethod
    {
        public CreditCard(Mediachase.Commerce.Orders.Payment payment) : base(payment)
        {
        }
    }
}
