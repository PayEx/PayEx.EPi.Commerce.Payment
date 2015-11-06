using System;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Dectorators.AdditionalValuesFormatters;
using PayEx.EPi.Commerce.Payment.Dectorators.ParameterReaders;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCapturers;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCreditors;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Models.PaymentMethods
{
    internal class FinancingInvoice : PaymentMethod
    {
        private readonly IPaymentManager _paymentManager;
        private readonly FinancingInvoiceParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        private readonly IPaymentActions _paymentActions;
        private readonly string _paymentMethodCode;
        private readonly IFinancialInvoicingOrderLineFormatter _financialInvoicingOrderLineFormatter;
        private readonly IUpdateAddressHandler _updateAddressHandler;

        public FinancingInvoice()
        {
        } // Needed for unit testing

        public FinancingInvoice(Mediachase.Commerce.Orders.Payment payment, IPaymentManager paymentManager,
            IParameterReader parameterReader,
            ICartActions cartActions, IOrderNumberGenerator orderNumberGenerator,
            IAdditionalValuesFormatter additionalValuesFormatter,
            IFinancialInvoicingOrderLineFormatter financialInvoicingOrderLineFormatter, IPaymentActions paymentActions,
            string paymentMethodCode, IUpdateAddressHandler updateAddressHandler)
            : base(payment)
        {
            _paymentManager = paymentManager;
            _parameterReader = new FinancingInvoiceParameterReader(parameterReader);
            _cartActions = cartActions;
            _orderNumberGenerator = orderNumberGenerator;

            _financialInvoicingOrderLineFormatter = financialInvoicingOrderLineFormatter;
            financialInvoicingOrderLineFormatter.IncludeOrderLines =
                _parameterReader.UseOnePhaseTransaction(this.PaymentMethodDto);
            _additionalValuesFormatter = new FinancingInvoiceAdditionalValuesFormatter(additionalValuesFormatter,
                financialInvoicingOrderLineFormatter);
            _paymentActions = paymentActions;
            _paymentMethodCode = paymentMethodCode;
            _updateAddressHandler = updateAddressHandler;
        }

        public override string PaymentMethodCode
        {
            get
            {
                return _paymentMethodCode;
            }
        }

        public override string DefaultView
        {
            get { return "FINANCING"; }
        }

        public override bool RequireAddressUpdate
        {
            get {
                return _parameterReader.GetLegalAddress(this.PaymentMethodDto); 
            }
        }

        public override bool IsDirectModel
        {
            get { return true; }
        }

        public override PurchaseOperation PurchaseOperation
        {
            get
            {
                return UseOnePhaseTransaction ? PurchaseOperation.SALE : PurchaseOperation.AUTHORIZATION;
            }
        }

        private bool UseOnePhaseTransaction
        {
            get
            {
                return _parameterReader.UseOnePhaseTransaction(this.PaymentMethodDto);
            }
        }

        public override PaymentInitializeResult Initialize(Action<string> redirectAction)
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                new GetConsumerLegalAddressForFinancingInvoice(
                    new InitializePayment(
                        new PurchaseFinancingInvoice(_paymentManager, _paymentActions), _paymentManager, _parameterReader, _cartActions, _additionalValuesFormatter)
                        , _paymentActions, _paymentManager, _updateAddressHandler), _orderNumberGenerator);
            return initializer.Initialize(this, null, null, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            return new PaymentCompleteResult { Success = true };
        }

        public override bool Capture()
        {
            if (UseOnePhaseTransaction)
                return true; 

            IPaymentCapturer capturer = new CapturePayment(null, _paymentManager);

            var financialInvoicingOrderLineFormatter = _financialInvoicingOrderLineFormatter;
            financialInvoicingOrderLineFormatter.IncludeOrderLines = true;
            var financingInvoiceAdditionalValuesFormatter = new FinancingInvoiceAdditionalValuesFormatter(null,
                financialInvoicingOrderLineFormatter);
            return capturer.Capture(this, financingInvoiceAdditionalValuesFormatter.Format(this.Payment as PayExPayment));
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
