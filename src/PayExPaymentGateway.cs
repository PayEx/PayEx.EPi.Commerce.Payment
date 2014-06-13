using System.Web;
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Models;
using EPiServer.Globalization;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Plugins.Payment;
using StructureMap;
using System;
using PurchaseOrder = Mediachase.Commerce.Orders.PurchaseOrder;

namespace Epinova.PayExProvider
{
    public class PayExPaymentGateway : AbstractPaymentGateway
    {
        private PaymentMethodDto _payment;
        private string _priceListArgs;
        private int _vat;
        private string _additionalValues;
        private string _defaultView;
        private readonly HttpContextBase _httpContext;
        private readonly IPurchaseOrder _purchaseOrder;
        private readonly IOrderNote _orderNote;
        private readonly IPaymentManager _paymentManager;
        private readonly ISettings _settings;

        public const string PriceListArgsParameter = "PriceListArgs";
        public const string VatParameter = "Vat";
        public const string AdditionalValuesParameter = "AdditionalValues";
        public const string DefaultViewParameter = "DefaultView";

        public PayExPaymentGateway()
            : this(ObjectFactory.GetInstance<HttpContextBase>(), ObjectFactory.GetInstance<ISettings>(), ObjectFactory.GetInstance<IPurchaseOrder>(), ObjectFactory.GetInstance<IOrderNote>(), ObjectFactory.GetInstance<IPaymentManager>())
        { }

        public PayExPaymentGateway(HttpContextBase httpContext, ISettings settings, IPurchaseOrder purchaseOrder, IOrderNote orderNote, IPaymentManager paymentManager)
        {
            _httpContext = httpContext;
            _purchaseOrder = purchaseOrder;
            _orderNote = orderNote;
            _paymentManager = paymentManager;
            _settings = settings;
        }

        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            if (HttpContext.Current == null)
                return false;

            if (!(payment is PayExPayment))
                return false;

            PurchaseOrder purchaseOrder = _purchaseOrder.Get(payment);
            if (purchaseOrder != null)
            {
                if (payment.TransactionType == TransactionType.Authorization.ToString())
                {
                    bool captured = InitializePayment(purchaseOrder, payment as PayExPayment);
                    return captured;
                }

                if (payment.TransactionType == TransactionType.Capture.ToString())
                {
                    int? transactionId = _orderNote.FindTransactionIdByTitle(purchaseOrder.OrderNotes, _settings.AuthorizationNoteTitle);
                    if (transactionId == null)
                    {
                        message = "Could not get PayEx Transaction Id from purchase order notes";
                        return false;
                    }

                    bool captured = CapturePayment(purchaseOrder, transactionId.Value);
                    message = transactionId.ToString();

                    return captured;
                }
                message = "The current payment method does not support this order type.";
                return false;
            }

            var cart = payment.Parent.Parent as Cart;
            if (cart != null && cart.Status == CartStatus.PaymentComplete.ToString())
            {
                //return true because this shopping cart has been paid already on PayEx
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the payment.
        /// </summary>
        /// <value>The payment.</value>
        public PaymentMethodDto Payment
        {
            get
            {
                if (_payment == null)
                {
                    _payment = Mediachase.Commerce.Orders.Managers.PaymentManager.GetPaymentMethodBySystemName("PayEx", SiteContext.Current.LanguageName);
                }
                return _payment;
            }
        }

        public string PriceArgsList
        {
            get
            {
                if (_priceListArgs == null)
                {
                    _priceListArgs = GetParameterByName(PriceListArgsParameter).Value;
                }
                return _priceListArgs;
            }
        }

        public int Vat
        {
            get
            {
                if (_vat <= 0)
                {
                    int.TryParse(GetParameterByName(VatParameter).Value, out _vat);
                }
                return _vat;
            }
        }

        public string AdditionalValues
        {
            get
            {
                if (_additionalValues == null)
                {
                    _additionalValues = GetParameterByName(AdditionalValuesParameter).Value;
                }
                return _additionalValues;
            }
        }

        public string DefaultView
        {
            get
            {
                if (_defaultView == null)
                {
                    _defaultView = GetParameterByName(DefaultViewParameter).Value;
                }
                return _defaultView;
            }
        }

        internal PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(string name)
        {
            PaymentMethodDto.PaymentMethodParameterRow[] rowArray = (PaymentMethodDto.PaymentMethodParameterRow[])Payment.PaymentMethodParameter.Select(string.Format("Parameter = '{0}'", name));
            if (rowArray.Length > 0)
                return rowArray[0];
            throw new ArgumentNullException("Parameter named " + name + " for PayEx payment cannot be null");
        }

        private bool InitializePayment(PurchaseOrder purchaseOrder, PayExPayment payment)
        {
            long price = (long)(purchaseOrder.Total * 100);

            PaymentInformation paymentInformation = new PaymentInformation(
                price, PriceArgsList, purchaseOrder.BillingCurrency, _vat,
                purchaseOrder.Id.ToString(), payment.ProductNumber, payment.Description, payment.ClientIpAddress,
                FormatUserAgent(payment), AdditionalValues, payment.ReturnUrl, DefaultView, payment.AgreementReference, 
                payment.CancelUrl, ContentLanguage.PreferredCulture.TextInfo.CultureName);

            string redirectUrl = _paymentManager.Initialize(paymentInformation);
            if (!string.IsNullOrWhiteSpace(redirectUrl))
                _httpContext.Response.Redirect(redirectUrl);

            return true;
        }

        private bool CapturePayment(PurchaseOrder purchaseOrder, int transactionId)
        {
            long amount = (long)(purchaseOrder.Total * 100);
            string orderId = purchaseOrder.TrackingNumber.Replace("PO", "");
            var useDefaultVat = purchaseOrder.MarketId == MarketId.Default;
            var vat = 0;//useDefaultVat ? PayExSettings.Instance.VAT : 0;

            string transactionNumber = _paymentManager.Capture(transactionId, amount, orderId, vat, string.Empty);

            if (!string.IsNullOrWhiteSpace(transactionNumber))
            {
                Mediachase.Commerce.Orders.OrderNote captureNote = _orderNote.Create(new Guid(), string.Concat(_settings.CaptureNoteMessage, transactionNumber),
                    _settings.CaptureNoteTitle, _settings.CaptureNoteTitle);

                purchaseOrder.OrderNotes.Add(captureNote);
                purchaseOrder.AcceptChanges();
                return true;
            }
            return false;
        }

        private string FormatUserAgent(PayExPayment payment)
        {
            return string.Concat("USERAGENT=", payment.ClientUserAgent);
        }
    }
}
