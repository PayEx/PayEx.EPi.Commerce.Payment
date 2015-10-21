using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCapturers;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCreditors;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Models.PaymentMethods
{
    internal class Invoice : PaymentMethod
    {
        private readonly IVerificationManager _verificationManager;
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        private readonly IPaymentActions _paymentActions;
        public Invoice() { }

        public Invoice(Mediachase.Commerce.Orders.Payment payment, IVerificationManager verificationManager, IPaymentManager paymentManager, IParameterReader parameterReader,   
            ICartActions cartActions, IOrderNumberGenerator orderNumberGenerator, IAdditionalValuesFormatter additionalValuesFormatter, IPaymentActions paymentActions)
            : base(payment)
        {
            _verificationManager = verificationManager;
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _cartActions = cartActions;
            _orderNumberGenerator = orderNumberGenerator;
            _additionalValuesFormatter = additionalValuesFormatter;
            _paymentActions = paymentActions;
        }

        public override string PaymentMethodCode => "INVOICESALE";

        public override string DefaultView => "FACTORING";

        public override bool RequireAddressUpdate => false;

        public override bool IsDirectModel => true;

        public override PurchaseOperation PurchaseOperation => PurchaseOperation.AUTHORIZATION;

        public override PaymentInitializeResult Initialize()
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                new GetConsumerLegalAddress(
                    new InitializePayment(
                        new PurchaseInvoiceSale(_paymentManager, _paymentActions), _paymentManager, _parameterReader, _cartActions, _additionalValuesFormatter),
                        _verificationManager, _paymentActions), _orderNumberGenerator);
            return initializer.Initialize(this, null, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            return new PaymentCompleteResult { Success = true };
        }

        public override bool Capture()
        {
            IPaymentCapturer capturer = new CapturePayment(null, _paymentManager);
            return capturer.Capture(this);
        }

        public override bool Credit()
        {
            IPaymentCreditor creditor = new CreditPayment(null, _paymentManager);
            return creditor.Credit(this);
        }

        public override Address GetAddressFromPayEx(TransactionResult transactionResult)
        {
            return null;
        }
    }
}
