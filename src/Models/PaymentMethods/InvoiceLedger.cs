
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Dectorators.PaymentCompleters;
using Epinova.PayExProvider.Dectorators.PaymentCreditors;
using Epinova.PayExProvider.Dectorators.PaymentInitializers;
using Epinova.PayExProvider.Facades;

namespace Epinova.PayExProvider.Models.PaymentMethods
{
    public class InvoiceLedger : PaymentMethod
    {
        public InvoiceLedger() { } // Needed for unit testing

        public InvoiceLedger(Mediachase.Commerce.Orders.Payment payment)
            : base(payment)
        {
        }

        public override PaymentInitializeResult Initialize()
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                 new InitializePayment(
                 new RedirectUser(), new PaymentManager(), new ParameterReader(), new CartActions(new Logger())));
            return initializer.Initialize(this, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            PaymentManager paymentManager = new PaymentManager();
            Logger logger = new Logger();

            IPaymentCompleter completer = new CompletePayment(
                new UpdateTransactionDetails(null, paymentManager, logger), paymentManager, logger);
            return completer.Complete(this, orderRef);
        }

        public override bool Capture()
        {
            throw new System.NotImplementedException();
        }

        public override bool Credit()
        {
            IPaymentCreditor creditor = new CreditPaymentByOrderLines(null, new Logger(), new PaymentManager());
            return creditor.Credit(this);
        }
    }
}
