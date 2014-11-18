using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;
using Epinova.PayExProvider.Payment;
using EPiServer.Globalization;

namespace Epinova.PayExProvider.Dectorators.PaymentInitializers
{
    public class InitializePayment : IPaymentInitializer
    {
        private readonly IPaymentInitializer _paymentInitializer;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;

        public InitializePayment(IPaymentInitializer paymentInitializer, IPriceFormatter priceFormatter, IPaymentManager paymentManager, IParameterReader parameterReader, ICartActions cartActions)
        {
            _paymentInitializer = paymentInitializer;
            _priceFormatter = priceFormatter;
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _cartActions = cartActions;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl)
        {
            PaymentInformation paymentInformation = CreateModel(currentPayment, orderNumber);

            InitializeResult result = _paymentManager.Initialize(currentPayment.Cart, paymentInformation);
            currentPayment.Payment.PayExOrderRef = result.OrderRef.ToString();

            _cartActions.UpdateCartInstanceId(currentPayment.Cart); // Save all the changes that have been done to the cart

            if (_paymentInitializer != null)
                return _paymentInitializer.Initialize(currentPayment, orderNumber, result.ReturnUrl);

            return new PaymentInitializeResult {Success = true};
        }

        private PaymentInformation CreateModel(PaymentMethod currentPayment, string orderNumber)
        {
            string additionalValues = string.Empty;
            string priceArgsList = _parameterReader.GetPriceArgsList(currentPayment.PaymentMethodDto);
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            string defaultView = _parameterReader.GetDefaultView(currentPayment.PaymentMethodDto);

            return new PaymentInformation(
               _priceFormatter.RoundToLong(currentPayment.Cart.Total), priceArgsList, currentPayment.Cart.BillingCurrency, vat,
               orderNumber, currentPayment.Payment.ProductNumber, currentPayment.Payment.Description, currentPayment.Payment.ClientIpAddress,
               currentPayment.Payment.ClientUserAgent, additionalValues, currentPayment.Payment.ReturnUrl, defaultView, currentPayment.Payment.AgreementReference,
               currentPayment.Payment.CancelUrl, ContentLanguage.PreferredCulture.TextInfo.CultureName);
        }

        //private string FormatAdditionalValues(PayExPayment payment)
        //{
        //    if (string.IsNullOrWhiteSpace(AdditionalValues))
        //        return string.Empty;

        //    string additional = AdditionalValues;
        //    additional = string.Concat(additional, string.Format("&INVOICE_CUSTOMERID={0}", payment.CustomerId));

        //    DateTime sixDaysForward = payment.Created.AddDays(6);
        //    additional = string.Concat(additional,
        //        string.Format("&INVOICE_DUEDATE={0}",
        //            new DateTime(sixDaysForward.Year, sixDaysForward.Month, sixDaysForward.Day).ToString("yyyy-MM-dd")));
        //    return additional;
        //}
    }
}
