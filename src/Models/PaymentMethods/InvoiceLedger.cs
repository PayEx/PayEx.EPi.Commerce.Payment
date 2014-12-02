using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Dectorators.PaymentCapturers;
using Epinova.PayExProvider.Dectorators.PaymentCompleters;
using Epinova.PayExProvider.Dectorators.PaymentCreditors;
using Epinova.PayExProvider.Dectorators.PaymentInitializers;

namespace Epinova.PayExProvider.Models.PaymentMethods
{
    public class InvoiceLedger : PaymentMethod
    {
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ILogger _logger;
        private readonly ICartActions _cartActions;
        public InvoiceLedger() { } // Needed for unit testing

        public InvoiceLedger(Mediachase.Commerce.Orders.Payment payment, IPaymentManager paymentManager,
            IParameterReader parameterReader, ILogger logger, ICartActions cartActions)
            : base(payment)
        {
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _logger = logger;
            _cartActions = cartActions;
        }

        public override PaymentInitializeResult Initialize()
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                 new InitializePayment(
                 new RedirectUser(), _paymentManager, _parameterReader, _cartActions));
            return initializer.Initialize(this, null, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            IPaymentCompleter completer = new CompletePayment(
                new UpdateTransactionDetails(null, _paymentManager, _logger), _paymentManager, _logger);
            return completer.Complete(this, orderRef);
        }

        public override bool Capture()
        {
            IPaymentCapturer capturer = new CapturePayment(null, _logger, _paymentManager, _parameterReader);
            return capturer.Capture(this);
        }

        public override bool Credit()
        {
            IPaymentCreditor creditor = new CreditPaymentByOrderLines(null, _logger, _paymentManager);
            return creditor.Credit(this);
        }
    }
}
