using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Dectorators.PaymentInitializers;
using Epinova.PayExProvider.Models.PaymentMethods;
using Epinova.PayExProvider.Price;

namespace Epinova.PayExProvider.Factories
{
    public class PaymentInitializerFactory : IPaymentInitializerFactory
    {
        public IPaymentInitializer Create(PaymentMethod payment)
        {
            if (payment is CreditCard)
            {
                return new GenerateOrderNumber(
                    new InitializePayment(
                    new RedirectUser(), new PriceFormatter(), new PaymentManager(), new ParameterReader(), new CartActions(new Logger())));
            }
            return null;
        }
    }
}
