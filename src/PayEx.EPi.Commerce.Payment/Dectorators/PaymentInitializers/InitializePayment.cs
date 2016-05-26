using System.Text;
using EPiServer.Globalization;
using EPiServer.Logging.Compatibility;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers
{
    internal class InitializePayment : IPaymentInitializer
    {
        private readonly IPaymentInitializer _paymentInitializer;
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public InitializePayment(IPaymentInitializer paymentInitializer, IPaymentManager paymentManager, IParameterReader parameterReader, ICartActions cartActions,
            IAdditionalValuesFormatter additionalValuesFormatter)
        {
            _paymentInitializer = paymentInitializer;
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _cartActions = cartActions;
            _additionalValuesFormatter = additionalValuesFormatter;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            Log.InfoFormat("Initializing payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            PaymentInformation paymentInformation = CreateModel(currentPayment, orderNumber);

            Log.InfoFormat("Initializing payment with Price:{0}", paymentInformation.Price);

            InitializeResult result = _paymentManager.Initialize(currentPayment.Cart, paymentInformation, currentPayment.IsDirectModel, currentPayment.IsDirectModel);
            if (!result.Status.Success)
                return new PaymentInitializeResult { Success = false, ErrorMessage = result.Status.Description };

            Log.InfoFormat("Setting PayEx order reference to {0} on payment with ID:{1} belonging to order with ID: {2}", result.OrderRef, currentPayment.Payment.Id, currentPayment.OrderGroupId);
            currentPayment.Payment.PayExOrderRef = result.OrderRef.ToString();

            _cartActions.UpdateCartInstanceId(currentPayment.Cart); // Save all the changes that have been done to the cart

            if (_paymentInitializer != null)
                return _paymentInitializer.Initialize(currentPayment, orderNumber, result.RedirectUrl, result.OrderRef.ToString());

            return new PaymentInitializeResult { Success = true };
        }

        private PaymentInformation CreateModel(PaymentMethod currentPayment, string orderNumber)
        {
            string additionalValues = FormatAdditionalValues(currentPayment);
            string priceArgsList = _parameterReader.GetPriceArgsList(currentPayment.PaymentMethodDto);
            string purchaseOperation = currentPayment.PurchaseOperation.ToString();

            return new PaymentInformation(
               currentPayment.Cart.Total.RoundToLong(), priceArgsList, currentPayment.Cart.BillingCurrency, currentPayment.Payment.Vat,
               orderNumber, currentPayment.Payment.ProductNumber, currentPayment.Payment.Description, currentPayment.Payment.ClientIpAddress,
               additionalValues, currentPayment.Payment.ReturnUrl, currentPayment.DefaultView,
               currentPayment.Payment.CancelUrl, ContentLanguage.PreferredCulture.TextInfo.CultureName, purchaseOperation);
        }

        private string FormatAdditionalValues(PaymentMethod currentPayment)
        {
            string staticAdditionalValues = _parameterReader.GetAdditionalValues(currentPayment.PaymentMethodDto);
            StringBuilder stringBuilder = new StringBuilder(staticAdditionalValues);

            string dynamicAdditionalValues = _additionalValuesFormatter.Format(currentPayment.Payment as PayExPayment);
            if (!string.IsNullOrWhiteSpace(dynamicAdditionalValues))
            {
                if (!string.IsNullOrWhiteSpace(staticAdditionalValues))
                    stringBuilder.Append("&");

                stringBuilder.Append(dynamicAdditionalValues);
            }

            return stringBuilder.ToString();
        }
    }
}
