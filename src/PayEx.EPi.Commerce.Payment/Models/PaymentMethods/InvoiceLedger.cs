using System.Linq;
using EPiServer.Logging.Compatibility;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCapturers;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCompleters;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCreditors;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Models.PaymentMethods
{
    internal class InvoiceLedger : PaymentMethod
    {
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        private readonly IPaymentActions _paymentActions;
        private readonly IRedirectUser _redirectUser;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public InvoiceLedger()
        {
        } // Needed for unit testing

        public InvoiceLedger(Mediachase.Commerce.Orders.Payment payment, IPaymentManager paymentManager,
            IParameterReader parameterReader, ICartActions cartActions, IOrderNumberGenerator orderNumberGenerator,
            IAdditionalValuesFormatter additionalValuesFormatter, IPaymentActions paymentActions, IRedirectUser redirectUser)
            : base(payment)
        {
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _cartActions = cartActions;
            _orderNumberGenerator = orderNumberGenerator;
            _additionalValuesFormatter = additionalValuesFormatter;
            _paymentActions = paymentActions;
            _redirectUser = redirectUser;
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
                 _redirectUser, _paymentManager, _parameterReader, _cartActions, _additionalValuesFormatter), _orderNumberGenerator);
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
            IPaymentCapturer capturer = new CapturePayment(null, _paymentManager);
            return capturer.Capture(this);
        }

        public override bool Credit()
        {
            IPaymentCreditor creditor = new CreditPaymentByOrderLines(null, _paymentManager);
            return creditor.Credit(this);
        }

        public override Address GetAddressFromPayEx(TransactionResult transactionResult)
        {
            Log.InfoFormat("Attempting to retrieve address for PayEx transaction result: {0} for Invoice Ledger payment with ID:{1} belonging to order with ID: {2}", transactionResult, Payment.Id, OrderGroupId);
            if (transactionResult.Invoice == null || string.IsNullOrWhiteSpace(transactionResult.CustomerName))
            {
                Log.ErrorFormat("TransactionResult must contain both an invoice element and a customer name in order to retrieve address for PayEx transaction result. Payment with ID:{1} belonging to order with ID: {2}", Payment.Id, OrderGroupId);
                return null;
            }

            string lastName = string.Empty;
            string[] names = transactionResult.Invoice.CustomerName.Split(' ');
            string firstName = names[0];
            if (names.Length > 1)
                lastName = string.Join(" ", names.Skip(1));

            Address address = new Address
            {
                FirstName = firstName,
                LastName = lastName,
                Line1 = transactionResult.Invoice.CustomerStreetAddress,
                PostCode = transactionResult.Invoice.CustomerPostNumber,
                City = transactionResult.Invoice.CustomerCity,
                Email = transactionResult.Invoice.CustomerEmail
            };
            Log.InfoFormat("Successfully retrieved address:{0} from PayEx transaction result: {1} for Invoice Ledger payment with ID:{2} belonging to order with ID: {3}",
                address, transactionResult, Payment.Id, OrderGroupId);
            return address;
        }
    }
}
