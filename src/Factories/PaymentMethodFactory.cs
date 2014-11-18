using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Factories
{
    public class PaymentMethodFactory : IPaymentMethodFactory
    {
        public PaymentMethod Create(Mediachase.Commerce.Orders.Payment payment)
        {
            if (!(payment is PayExPayment))
                return null;

            return new CreditCard(payment);
        }
    }
}
