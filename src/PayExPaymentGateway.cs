using System.Collections.Generic;
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Payment;
using Epinova.PayExProvider.Price;
using EPiServer.Globalization;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Plugins.Payment;
using System;
using System.Web;
using Mediachase.Data.Provider;
using PurchaseOrder = Mediachase.Commerce.Orders.PurchaseOrder;

namespace Epinova.PayExProvider
{
    public class PayExPaymentGateway : AbstractPaymentGateway
    {
        private PaymentMethodDto _payment;
        private string _priceListArgs;
        private string _additionalValues;
        private string _defaultView;
        private int _vat;
        private readonly ILogger _logger;
        private readonly IPaymentManager _paymentManager;
        private readonly PriceFormatter _priceFormatter;
        private readonly IPayExSettings _settings;
        private Guid _currentPaymentMethodId;

        public const string VatParameter = "Vat";
        public const string PriceListArgsParameter = "PriceListArgs";
        public const string AdditionalValuesParameter = "AdditionalValues";
        public const string DefaultViewParameter = "DefaultView";

        public PayExPaymentGateway()
        {
            _logger = new Logger();
            _paymentManager = new PaymentManager();
            _priceFormatter = new PriceFormatter();
            _settings = PayExSettings.Instance;
        }

        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            if (HttpContext.Current == null)
            {
                _logger.LogWarning("HttpContent.Current is null");
                return false;
            }

            if (!(payment is PayExPayment))
            {
                _logger.LogError("Only PayExPayments can be used with the PayExPaymentGateway");
                return false;
            }

            _currentPaymentMethodId = payment.PaymentMethodId;

            var cart = payment.Parent.Parent as Cart;
            if (cart == null && payment.Parent.Parent is PurchaseOrder)
            {
                // when user click complete order in commerce manager the transaction type will be Capture
                if (payment.TransactionType.Equals(TransactionType.Capture.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    PurchaseOrder purchaseOrder = payment.Parent.Parent as PurchaseOrder;
                    try
                    {
                        if (IsInvoicePayment(payment as PayExPayment))
                            return true;

                        _logger.LogDebug(string.Format("Begin CapturePayment for purchaseOrder with ID:{0}", purchaseOrder.Id));
                        return CapturePayment(purchaseOrder, payment as PayExPayment);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(string.Format("Error in CapturePayment for purchaseOrder with ID:{0}", purchaseOrder.Id), e);
                        return false;
                    }
                }

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (payment.TransactionType == TransactionType.Credit.ToString())
                {
                    return false; // Not implemented
                }

                return false;
            }

            // When "Complete" or "Refund" shipment in Commerce Manager, this method will be run again with the TransactionType is Capture/Credit
            // PayEx will always return true to bypass the payment process again.
            if (!payment.TransactionType.Equals(TransactionType.Authorization.ToString(), StringComparison.OrdinalIgnoreCase))
                return true;

            if (cart != null && cart.Status == CartStatus.PaymentComplete.ToString())
                return true; // return true because this shopping cart has been paid already on PayEx

            if (cart == null)
                return false;

            try
            {
                _logger.LogDebug(string.Format("Begin InitializePayment for cart with ID:{0}", cart.Id));
                return InitializePayment(cart, payment as PayExPayment);
            }
            catch (Exception e)
            {
                _logger.LogError("Error when initializing PayEx payment request", e);
                return false;
            }
        }

        public bool ProcessSuccessfulTransaction(PayExPayment payExPayment, string orderNumber, string orderRef, Cart cart, out TransactionErrorCode? error)
        {
            CompleteResult completeResult = _paymentManager.Complete(orderRef);
            if (completeResult.Error || string.IsNullOrWhiteSpace(completeResult.TransactionNumber))
            {
                error = completeResult.ErrorCode;
                return false;
            }

            error = null;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    cart.Status = CartStatus.PaymentComplete.ToString();

                    // Change status of payments to processed. 
                    // It must be done before execute workflow to ensure payments which should mark as processed.
                    // To avoid get errors when executed workflow.
                    foreach (OrderForm orderForm in cart.OrderForms)
                    {
                        foreach (Mediachase.Commerce.Orders.Payment payment in orderForm.Payments)
                        {
                            if (payment != null)
                                PaymentStatusManager.ProcessPayment(payment);
                        }
                    }

                    // Execute CheckOutWorkflow with parameter to ignore running process payment activity again.
                    var isIgnoreProcessPayment = new Dictionary<string, object>();
                    isIgnoreProcessPayment.Add("IsIgnoreProcessPayment", true);
                    OrderGroupWorkflowManager.RunWorkflow(cart, OrderGroupWorkflowManager.CartCheckOutWorkflowName, true,
                        isIgnoreProcessPayment);

                    cart.OrderNumberMethod = c => orderNumber;
                    payExPayment.AuthorizationCode = completeResult.TransactionNumber;
                    payExPayment.AcceptChanges();
                    PurchaseOrder purchaseOrder = cart.SaveAsPurchaseOrder();

                    cart.Delete();
                    cart.AcceptChanges();

                    purchaseOrder.OrderForms[0]["PaymentMethodCode"] = completeResult.PaymentMethod;
                    purchaseOrder.AcceptChanges();

                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error in ProcessSuccessfulTransaction", e);
                return false;
            }
            return true;
        }

        private bool IsInvoicePayment(PayExPayment payment)
        {
            return
                Payment.PaymentMethod.FindByPaymentMethodId(payment.PaymentMethodId)
                    .SystemKeyword.Contains(_settings.InvoiceKeyword);
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
                    _payment = Mediachase.Commerce.Orders.Managers.PaymentManager.GetPaymentMethod(_currentPaymentMethodId);
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

        private string GenerateOrderNumber(int orderGroupId)
        {
            string str = new Random().Next(1000, 9999).ToString();
            return string.Format("{0}{1}", orderGroupId, str);
        }

        private bool InitializePayment(Cart cart, PayExPayment payment)
        {
            var orderNumber = GenerateOrderNumber(cart.OrderGroupId);

            payment.OrderNumber = orderNumber;
            payment.Description = string.Format(payment.Description, orderNumber);
            string additionalValues = FormatAdditionalValues(payment);

            PaymentInformation paymentInformation = new PaymentInformation(
                _priceFormatter.RoundToLong(cart.Total), PriceArgsList, cart.BillingCurrency, Vat,
                orderNumber, payment.ProductNumber, payment.Description, payment.ClientIpAddress,
                payment.ClientUserAgent, additionalValues, payment.ReturnUrl, DefaultView, payment.AgreementReference,
                payment.CancelUrl, ContentLanguage.PreferredCulture.TextInfo.CultureName);

            string orderRef;
            string redirectUrl = _paymentManager.Initialize(cart, paymentInformation, out orderRef);
            payment.PayExOrderRef = orderRef;

            CartHelper.UpdateCartInstanceId(cart);

            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                HttpContext.Current.Response.Redirect(redirectUrl, true);
                return true;
            }

            return false;
        }

        private bool CapturePayment(PurchaseOrder purchaseOrder, Mediachase.Commerce.Orders.Payment payExPayment)
        {
            int transactionId;
            if (!int.TryParse(payExPayment.AuthorizationCode, out transactionId))
            {
                _logger.LogWarning(string.Format("Could not get PayEx Transaction Id from purchase order with ID: {0}", purchaseOrder.Id));
                return false;
            }

            long amount = _priceFormatter.RoundToLong(payExPayment.Amount);
            string transactionNumber = _paymentManager.Capture(transactionId, amount, purchaseOrder.TrackingNumber, Vat, string.Empty);

            if (!string.IsNullOrWhiteSpace(transactionNumber))
            {
                payExPayment.ValidationCode = transactionNumber;
                payExPayment.AcceptChanges();
                return true;
            }
            return false;
        }

        private string FormatAdditionalValues(PayExPayment payment)
        {
            if (string.IsNullOrWhiteSpace(AdditionalValues))
                return string.Empty;

            string additional = AdditionalValues;
            additional = string.Concat(additional, string.Format("&INVOICE_CUSTOMERID={0}", payment.CustomerId));

            DateTime sixDaysForward = payment.Created.AddDays(6);
            additional = string.Concat(additional,
                string.Format("&INVOICE_DUEDATE={0}",
                    new DateTime(sixDaysForward.Year, sixDaysForward.Month, sixDaysForward.Day).ToString("yyyy-MM-dd")));
            return additional;
        }
    }
}
