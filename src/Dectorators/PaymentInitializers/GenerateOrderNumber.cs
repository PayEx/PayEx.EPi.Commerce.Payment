using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers
{
    internal class GenerateOrderNumber : IPaymentInitializer
    {
        private readonly IPaymentInitializer _initializer;
        private readonly IOrderNumberGenerator _orderNumberGenerator;

        public GenerateOrderNumber(IPaymentInitializer initializer, IOrderNumberGenerator orderNumberGenerator)
        {
            _initializer = initializer;
            _orderNumberGenerator = orderNumberGenerator;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                orderNumber = _orderNumberGenerator.Generate(currentPayment.Cart);

            currentPayment.Payment.OrderNumber = orderNumber;

            if (!string.IsNullOrWhiteSpace(currentPayment.Payment.Description))
                currentPayment.Payment.Description = string.Format(currentPayment.Payment.Description, orderNumber);

            return _initializer.Initialize(currentPayment, orderNumber, returnUrl, orderRef);
        }
    }
}
