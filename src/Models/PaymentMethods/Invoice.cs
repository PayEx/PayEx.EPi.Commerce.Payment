
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Dectorators.PaymentInitializers;

namespace Epinova.PayExProvider.Models.PaymentMethods
{
    public class Invoice : PaymentMethod
    {
        private readonly IVerificationManager _verificationManager;
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        public Invoice() { }

        public Invoice(Mediachase.Commerce.Orders.Payment payment, IVerificationManager verificationManager, IPaymentManager paymentManager, IParameterReader parameterReader, 
            ICartActions cartActions)
            : base(payment)
        {
            _verificationManager = verificationManager;
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _cartActions = cartActions;
        }

        public override PaymentInitializeResult Initialize()
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                 new InitializePayment(
                     new GetConsumerLegalAddress(null, _verificationManager), _paymentManager, _parameterReader, _cartActions));
            return initializer.Initialize(this, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            throw new System.NotImplementedException();
        }

        public override bool Capture()
        {
            throw new System.NotImplementedException();
        }

        public override bool Credit()
        {
            throw new System.NotImplementedException();
        }
    }
}
