using System.Linq;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCapturers;
using EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCompleters;
using EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCreditors;
using EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods
{
    internal class InvoiceLedger : PaymentMethod
    {
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        private readonly IPaymentActions _paymentActions;
        public InvoiceLedger() { } // Needed for unit testing

        public InvoiceLedger(Mediachase.Commerce.Orders.Payment payment, IPaymentManager paymentManager,
            IParameterReader parameterReader, ICartActions cartActions, IOrderNumberGenerator orderNumberGenerator, 
            IAdditionalValuesFormatter additionalValuesFormatter, IPaymentActions paymentActions)
            : base(payment)
        {
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _cartActions = cartActions;
            _orderNumberGenerator = orderNumberGenerator;
            _additionalValuesFormatter = additionalValuesFormatter;
            _paymentActions = paymentActions;
        }

        public override string PaymentMethodCode
        {
            get { return "INVOICE"; }
        }

        public override string DefaultView
        {
            get { return "INVOICE"; }
        }

        public override bool RequireAddressUpdate
        {
            get { return true; }
        }

        public override bool IsDirectModel
        {
            get { return false; }
        }

        public override PurchaseOperation PurchaseOperation
        {
            get { return PurchaseOperation.AUTHORIZATION; }
        }

        public override PaymentInitializeResult Initialize()
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                 new InitializePayment(
                 new RedirectUser(), _paymentManager, _parameterReader, _cartActions, _additionalValuesFormatter), _orderNumberGenerator);
            return initializer.Initialize(this, null, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            IPaymentCompleter completer = new CompletePayment(
                new UpdateTransactionDetails(null, _paymentManager), _paymentManager, _paymentActions);
            return completer.Complete(this, orderRef);
        }

        public override bool Capture()
        {
            IPaymentCapturer capturer = new CapturePayment(null, _paymentManager, _parameterReader);
            return capturer.Capture(this);
        }

        public override bool Credit()
        {
            IPaymentCreditor creditor = new CreditPaymentByOrderLines(null, _paymentManager);
            return creditor.Credit(this);
        }

        public override Address GetAddressFromPayEx(TransactionResult transactionResult)
        {
            if (transactionResult.Invoice == null || string.IsNullOrWhiteSpace(transactionResult.CustomerName))
                return null;

            string lastName = string.Empty;
            string[] names = transactionResult.Invoice.CustomerName.Split(' ');
            string firstName = names[0];
            if (names.Length > 1)
                lastName = string.Join(" ", names.Skip(1));

            return new Address
            {
                FirstName = firstName,
                LastName = lastName,
                Line1 = transactionResult.Invoice.CustomerStreetAddress,
                PostCode = transactionResult.Invoice.CustomerPostNumber,
                City = transactionResult.Invoice.CustomerCity,
                Email = transactionResult.Invoice.CustomerEmail
            };
        }
    }
}
