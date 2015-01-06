using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods
{
    internal class Invoice : PaymentMethod
    {
        private readonly IVerificationManager _verificationManager;
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        public Invoice() { }

        public Invoice(Mediachase.Commerce.Orders.Payment payment, IVerificationManager verificationManager, IPaymentManager paymentManager, IParameterReader parameterReader, 
            ICartActions cartActions, IOrderNumberGenerator orderNumberGenerator)
            : base(payment)
        {
            _verificationManager = verificationManager;
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _cartActions = cartActions;
            _orderNumberGenerator = orderNumberGenerator;
        }

        public override string PaymentMethodCode
        {
            get { return "INVOICESALE"; }
        }

        public override string DefaultView
        {
            get { return "FACTORING"; }
        }

        public override PurchaseOperation PurchaseOperation
        {
            get { return PurchaseOperation.AUTHORIZATION; }
        }

        public override PaymentInitializeResult Initialize()
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                new GetConsumerLegalAddress(
                    new InitializePayment(
                        new PurchaseInvoiceSale(_paymentManager), _paymentManager, _parameterReader, _cartActions), _verificationManager), _orderNumberGenerator);
            return initializer.Initialize(this, null, null, null);
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
