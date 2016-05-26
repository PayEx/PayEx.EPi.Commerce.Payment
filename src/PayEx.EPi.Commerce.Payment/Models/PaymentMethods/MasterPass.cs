using System;
using EPiServer.Logging.Compatibility;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Dectorators.AdditionalValuesFormatters;
using PayEx.EPi.Commerce.Payment.Dectorators.ParameterReaders;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCapturers;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCompleters;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentCreditors;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Models.PaymentMethods
{
    internal class MasterPass : PaymentMethod
    {
        private const string UseMasterPassParameterNotAllowedErrorMessage =
            "AdditionalValues for MasterPass configured in Commerce Manager cannot set parameter USEMASTERPASS as it needs to be set by explicitly by the payment method";

        private const string ResponsiveParameterNotAllowedErrorMessage =
            "AdditionalValues for MasterPass configured in Commerce Manager cannot set parameter RESPONSIVE as it needs to be set by explicitly by the payment method";

        private readonly IPaymentManager _paymentManager;
        private readonly MasterPassParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        private readonly IPaymentActions _paymentActions;
        private readonly IRedirectUser _redirectUser;

        public MasterPass()
        {
        } // Needed for unit testing

        public MasterPass(Mediachase.Commerce.Orders.Payment payment, IPaymentManager paymentManager,
            IParameterReader parameterReader, ICartActions cartActions, IOrderNumberGenerator orderNumberGenerator,
            IAdditionalValuesFormatter additionalValuesFormatter, IPaymentActions paymentActions,
            IMasterPassShoppingCartFormatter masterPassShoppingCartFormatter, IRedirectUser redirectUser)
            : base(payment)
        {
            _paymentManager = paymentManager;
            _parameterReader = new MasterPassParameterReader(parameterReader);
            _cartActions = cartActions;
            _orderNumberGenerator = orderNumberGenerator;
            _additionalValuesFormatter = new MasterPassAdditionalValuesFormatter(additionalValuesFormatter,
                _parameterReader.AddShoppingCartXml(this.PaymentMethodDto), masterPassShoppingCartFormatter);
            _paymentActions = paymentActions;
            _redirectUser = redirectUser;
        }

        public override string PaymentMethodCode
        {
            get { return "MASTERPASS"; }
        }

        public override string DefaultView
        {
            get { return "CREDITCARD"; }
        }

        public override bool RequireAddressUpdate
        {
            get
            {
                return _parameterReader.UseBestPracticeFlow(this.PaymentMethodDto); 
            }
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
                _redirectUser, _paymentManager, _parameterReader, _cartActions, _additionalValuesFormatter),
                _orderNumberGenerator);

            return initializer.Initialize(this, null, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            IPaymentCompleter completer =
                new MasterPassFinalizeTransaction(new CompletePayment(null, _paymentManager, _paymentActions),
                    _paymentManager);
            return completer.Complete(this, orderRef);
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

        public static void ValidateMasterPassAdditionalValues(string additionalValues)
        {
            if (additionalValues.IndexOf("RESPONSIVE=", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                LogManager.GetLogger(Constants.Logging.DefaultLoggerName).Error(ResponsiveParameterNotAllowedErrorMessage);
                throw new InvalidOperationException(ResponsiveParameterNotAllowedErrorMessage);
            }

            if (additionalValues.IndexOf("USEMASTERPASS=", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                LogManager.GetLogger(Constants.Logging.DefaultLoggerName).Error(UseMasterPassParameterNotAllowedErrorMessage);
                throw new InvalidOperationException(UseMasterPassParameterNotAllowedErrorMessage);

            }
        }
    }
}
