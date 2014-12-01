using System;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;
using Epinova.PayExProvider.Models.Result;
using Epinova.PayExProvider.Price;
using EPiServer.Globalization;

namespace Epinova.PayExProvider.Dectorators.PaymentInitializers
{
    public class InitializePayment : IPaymentInitializer
    {
        private readonly IPaymentInitializer _paymentInitializer;
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ICartActions _cartActions;

        public InitializePayment(IPaymentInitializer paymentInitializer, IPaymentManager paymentManager, IParameterReader parameterReader, ICartActions cartActions)
        {
            _paymentInitializer = paymentInitializer;
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
                return _paymentInitializer.Initialize(currentPayment, orderNumber, result.RedirectUrl);

            return new PaymentInitializeResult {Success = true};
        }

        private PaymentInformation CreateModel(PaymentMethod currentPayment, string orderNumber)
        {
            string additionalValues = FormatAdditionalValues(currentPayment);
            string priceArgsList = _parameterReader.GetPriceArgsList(currentPayment.PaymentMethodDto);
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            string defaultView = _parameterReader.GetDefaultView(currentPayment.PaymentMethodDto);

            return new PaymentInformation(
               currentPayment.Cart.Total.RoundToLong(), priceArgsList, currentPayment.Cart.BillingCurrency, vat,
               orderNumber, currentPayment.Payment.ProductNumber, currentPayment.Payment.Description, currentPayment.Payment.ClientIpAddress,
               currentPayment.Payment.ClientUserAgent, additionalValues, currentPayment.Payment.ReturnUrl, defaultView, currentPayment.Payment.AgreementReference,
               currentPayment.Payment.CancelUrl, ContentLanguage.PreferredCulture.TextInfo.CultureName, currentPayment.Payment.PurchaseOperation);
        }

        private string FormatAdditionalValues(PaymentMethod currentPayment)
        {
            string additionalValues = _parameterReader.GetAdditionalValues(currentPayment.PaymentMethodDto);
            if (string.IsNullOrWhiteSpace(additionalValues))
                return string.Empty;

            additionalValues = string.Concat(additionalValues, string.Format("&INVOICE_CUSTOMERID={0}", currentPayment.Payment.CustomerId));

            DateTime sixDaysForward = currentPayment.Payment.Created.AddDays(6);
            additionalValues = string.Concat(additionalValues,
                string.Format("&INVOICE_DUEDATE={0}",
                    new DateTime(sixDaysForward.Year, sixDaysForward.Month, sixDaysForward.Day).ToString("yyyy-MM-dd")));
            return additionalValues;
        }
    }
}
