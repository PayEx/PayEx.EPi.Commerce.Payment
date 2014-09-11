using System.Collections.Generic;
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Models;
using EPiServer.Globalization;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Plugins.Payment;
using System;
using System.Web;
using Mediachase.Commerce.Security;
using Mediachase.Data.Provider;
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
        private readonly ILogger _logger;
        private readonly IPurchaseOrder _purchaseOrder;
        private readonly IOrderNote _orderNote;
        private readonly IPaymentManager _paymentManager;

        public const string PriceListArgsParameter = "PriceListArgs";
        public const string VatParameter = "Vat";
        public const string AdditionalValuesParameter = "AdditionalValues";
        public const string DefaultViewParameter = "DefaultView";

        public PayExPaymentGateway()
        {
            _logger = new Logger();
            _purchaseOrder = new Commerce.PurchaseOrder();
            _orderNote = new Commerce.OrderNote(_logger);
            _paymentManager = new PaymentManager();
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

            var cart = payment.Parent.Parent as Cart;
            if (cart == null && payment.Parent.Parent is PurchaseOrder)
            {
                // when user click complete order in commerce manager the transaction type will be Capture
                if (payment.TransactionType.Equals(TransactionType.Capture.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        //int? transactionId = _orderNote.FindTransactionIdByTitle(purchaseOrder.OrderNotes, PayExSettings.Instance.AuthorizationNoteTitle);
                        //if (transactionId == null)
                        //{
                        //    message = "Could not get PayEx Transaction Id from purchase order notes";
                        //    return false;
                        //}

                        //_logger.LogDebug(string.Format("Begin CapturePayment for purchaseOrder with ID:{0} and TransactionID:{1}", purchaseOrder.Id, transactionId.Value));
                        //bool captured = CapturePayment(purchaseOrder, transactionId.Value);
                        //message = transactionId.ToString();

                        //return captured;
                        return true;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Errror when completing order", e);
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

        public bool ProcessSuccessfulTransaction(string orderNumber, string orderRef, Cart cart)
        {
            string transactionNumber = _paymentManager.Complete(orderRef);
            if (string.IsNullOrWhiteSpace(transactionNumber))
                return false;

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
                        {
                            PaymentStatusManager.ProcessPayment(payment);
                        }
                    }
                }

                // Execute CheckOutWorkflow with parameter to ignore running process payment activity again.
                var isIgnoreProcessPayment = new Dictionary<string, object>();
                isIgnoreProcessPayment.Add("IsIgnoreProcessPayment", true);
                OrderGroupWorkflowManager.RunWorkflow(cart, OrderGroupWorkflowManager.CartCheckOutWorkflowName, true, isIgnoreProcessPayment);

                // Save changes
                cart.OrderNumberMethod = c => orderRef;
                PurchaseOrder purchaseOrder = cart.SaveAsPurchaseOrder(); 

                Mediachase.Commerce.Orders.OrderNote captureNote = _orderNote.Create(SecurityContext.Current.CurrentUserId, string.Concat(PayExSettings.Instance.AuthorizationNoteMessage, transactionNumber),
                 PayExSettings.Instance.AuthorizationNoteTitle, PayExSettings.Instance.AuthorizationNoteTitle);

                purchaseOrder.OrderNotes.Add(captureNote);

                cart.Delete();
                cart.AcceptChanges();
                purchaseOrder.AcceptChanges();
                scope.Complete();
            }
            return true;
        }

        private string GenerateOrderNumber(int orderGroupId)
        {
            string str = new Random().Next(1000, 9999).ToString();
            return string.Format("{0}{1}", orderGroupId, str);
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

        private bool InitializePayment(Cart cart, PayExPayment payment)
        {
            var orderNumber = GenerateOrderNumber(cart.OrderGroupId);
            cart.OrderNumberMethod = c => orderNumber;

            payment.OrderNumber = orderNumber;
            CartHelper.UpdateCartInstanceId(cart);

            payment.Description = string.Format(payment.Description, orderNumber);

            PaymentInformation paymentInformation = new PaymentInformation(
                cart.Total, PriceArgsList, cart.BillingCurrency, _vat,
                orderNumber, payment.ProductNumber, payment.Description, payment.ClientIpAddress,
                payment.ClientUserAgent, AdditionalValues, payment.ReturnUrl, DefaultView, payment.AgreementReference,
                payment.CancelUrl, ContentLanguage.PreferredCulture.TextInfo.CultureName);

            string redirectUrl = _paymentManager.Initialize(cart, paymentInformation);
            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                HttpContext.Current.Response.Redirect(redirectUrl, true);
                return true;
            }

            return false;
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
                Mediachase.Commerce.Orders.OrderNote captureNote = _orderNote.Create(new Guid(), string.Concat(PayExSettings.Instance.CaptureNoteMessage, transactionNumber),
                    PayExSettings.Instance.CaptureNoteTitle, PayExSettings.Instance.CaptureNoteTitle);

                purchaseOrder.OrderNotes.Add(captureNote);
                purchaseOrder.AcceptChanges();
                return true;
            }
            return false;
        }
    }

    ///// <summary>
    ///// KlarnaPaymentGateway class
    ///// </summary>
    //public class KlarnaPaymentGateway : AbstractPaymentGateway
    //{
    //    private readonly ILog _logger = log4net.LogManager.GetLogger(typeof(KlarnaPaymentGateway));
    //    public const string PclassId = "PclassId";
    //    private const KlarnaHelper.KlarnaPayment PaymentType = KlarnaHelper.KlarnaPayment.PartPayment;

    //    /// <summary>
    //    /// Processes the payment.
    //    /// </summary>
    //    /// <param name="payment">The payment.</param>
    //    /// <param name="message">The message.</param>
    //    /// <returns>True if success; otherwise false</returns>
    //    public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment orderPayment, ref string message)
    //    {
    //        KlarnaPayment payment = orderPayment as KlarnaPayment;
    //        // active invoice when order is complete
    //        // when user click complete order in commerce manager the transaction type will be Capture
    //        if (payment.TransactionType.Equals(TransactionType.Capture.ToString(), StringComparison.OrdinalIgnoreCase))
    //        {
    //            try
    //            {
    //                // create a Klarna client
    //                var klarnaProxy = KlarnaHelper.ConfigKlarna(new API(), PaymentType);
    //                var klarnaTransaction = payment.TransactionID;
    //                klarnaProxy.ActivateInvoice(klarnaTransaction);
    //            }
    //            catch (KlarnaException e)
    //            {
    //                _logger.Error("Errror when completing order", e);
    //                message = e.FaultString;
    //                return false;
    //            }

    //            return true;
    //        }

    //        // Refund processing
    //        // when user click complete return order in commerce manager the transaction type will be Credit
    //        if (payment.TransactionType.Equals(TransactionType.Credit.ToString(), StringComparison.OrdinalIgnoreCase))
    //        {
    //            try
    //            {
    //                // create a Klarna client
    //                var klarnaProxy = KlarnaHelper.ConfigKlarna(new API(), PaymentType);
    //                var returnAmount = payment.Amount;
    //                var klarnaTransaction = payment.TransactionID;

    //                string invoice = klarnaProxy.ReturnAmount(klarnaTransaction, (double)returnAmount, 0, API.Flag.IncVAT, string.Empty);
    //                payment.TransactionID = invoice;
    //            }
    //            catch (KlarnaException e)
    //            {
    //                _logger.Error("Errror when processing refund", e);
    //                message = e.FaultString;
    //                return false;
    //            }

    //            return true;
    //        }

    //        // When "Complete" or "Refund" shipment in Commerce Manager, this method will be run again with the TransactionType is Capture/Credit
    //        // Klarna will always return true to bypass the payment process again.
    //        if (!payment.TransactionType.Equals(TransactionType.Authorization.ToString(), StringComparison.OrdinalIgnoreCase))
    //        {
    //            return true;
    //        }

    //        // Get the billing address
    //        var billingAddress = payment.Parent.Parent.OrderAddresses.Cast<OrderAddress>().FirstOrDefault(address2 => address2.Name == payment.BillingAddressId);
    //        if (billingAddress == null)
    //        {
    //            throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "", "Billing address was not specified.");
    //        }

    //        // Initialize and configure Klarna object.
    //        var klarnaClient = KlarnaHelper.ConfigKlarna(new API(), PaymentType);

    //        // Add line items to pay by Klarna
    //        var shipments = payment.Parent.Shipments.Cast<Shipment>();
    //        foreach (LineItem lineItem in payment.Parent.LineItems)
    //        {
    //            // ShippingAddressId of lineItem is empty, we should get id from shipment.
    //            decimal taxRate = 0;
    //            var lineItemShipment = shipments.FirstOrDefault(s => Shipment.GetShipmentLineItems(s).Any(l => l.LineItemId == lineItem.Id));
    //            if (lineItemShipment == null)
    //            {
    //                // invalid shipment
    //                _logger.Error("Shipment not found for line item " + lineItem.CatalogEntryId);
    //            }
    //            else
    //            {
    //                OrderAddress shippingAddress = lineItemShipment.Parent.Parent.OrderAddresses.Cast<OrderAddress>().FirstOrDefault(address => string.Equals(address.Name, lineItemShipment.ShippingAddressId, StringComparison.Ordinal));
    //                // Calculate the tax for a line item
    //                taxRate = shippingAddress != null ? KlarnaHelper.GetItemTax(lineItem, shippingAddress) : 0;
    //            }

    //            var itemPriceExcTax = (lineItem.ListPrice);
    //            var itemTax = taxRate > 0 ? (itemPriceExcTax * taxRate / 100) : 0;

    //            // Calculate the discount for each product in percent
    //            var discount = lineItem.ListPrice == 0 ? 0.0d : (double)(((lineItem.ListPrice * lineItem.Quantity - lineItem.ExtendedPrice) / lineItem.Quantity) / lineItem.ListPrice) * 100;
    //            klarnaClient.AddArticle((int)lineItem.Quantity, lineItem.LineItemId.ToString(), lineItem.DisplayName, (double)(itemPriceExcTax + itemTax), (double)taxRate, discount, API.GoodsIs.IncVAT);
    //        }

    //        // Get the cart
    //        var cart = payment.Parent.Parent as Cart;
    //        if (cart == null)
    //        {
    //            return false;
    //        }

    //        var orderNumber = KlarnaHelper.GenerateOrderNumber(cart.OrderGroupId);
    //        cart.OrderNumberMethod = new Cart.CreateOrderNumber((c) => orderNumber);

    //        // Get the SocialSecurityNo from payment
    //        var socialSecurityNo = payment.SocialSecurityNo as String;
    //        if (string.IsNullOrEmpty(socialSecurityNo))
    //        {
    //            return false;
    //        }

    //        // Get the pclassId from payment
    //        if (string.IsNullOrEmpty(payment.PclassId))
    //        {
    //            return false;
    //        }
    //        var pclass = int.Parse(payment.PclassId.ToString());
    //        payment.PclassId = null;
    //        var status = true;

    //        string houseNumber = string.Empty;
    //        string houseExtension = string.Empty;
    //        try
    //        {
    //            switch (billingAddress.CountryCode)
    //            {
    //                case KlarnaHelper.Deu:
    //                    houseNumber = payment.GermanyHouseNumberText;
    //                    // House number is required
    //                    if (string.IsNullOrEmpty(houseNumber))
    //                    {
    //                        status = false;
    //                        break;
    //                    }
    //                    break;
    //                case KlarnaHelper.Nld:
    //                    houseNumber = payment.NetherlandsHouseNumberText;
    //                    houseExtension = payment.NetherlandsHouseExtensionText;
    //                    // House number is required
    //                    if (string.IsNullOrEmpty(houseNumber))
    //                    {
    //                        status = false;
    //                        break;
    //                    }
    //                    if (houseExtension == null)
    //                    {
    //                        houseExtension = string.Empty;
    //                    }

    //                    break;
    //            }

    //            // Add shipping fee and handling fee
    //            klarnaClient.AddArticle(1, string.Empty, "Shipment fee", (double)payment.Parent.ShippingTotal, 0, 0, API.GoodsIs.Shipping | API.GoodsIs.IncVAT);
    //            klarnaClient.AddArticle(1, string.Empty, "Handling fee", (double)payment.Parent.HandlingTotal, 0, 0, API.GoodsIs.Handling | API.GoodsIs.IncVAT);
    //            // Handles gift card and total amount of other active payment methods
    //            decimal otherActivePaymentAmount = payment.Parent.Total - payment.Amount;
    //            if (otherActivePaymentAmount != 0)
    //            {
    //                // Klarna doesn't support Gift Card directly so we add special item with negative amount to make the actual amount correct
    //                klarnaClient.AddArticle(1, string.Empty, "Gift Card", (double)-otherActivePaymentAmount, 0, 0, API.GoodsIs.Handling | API.GoodsIs.IncVAT);
    //            }

    //            KlarnaHelper.SetKlarnaAddress(klarnaClient, billingAddress, billingAddress, houseNumber, houseExtension);
    //            klarnaClient.OrderID1 = orderNumber;

    //            string[] result = klarnaClient.AddTransaction(socialSecurityNo, null, API.Flag.ReturnOCR, pclass);
    //            // save transaction id from Klarna server for later use
    //            payment.TransactionID = result[0];
    //        }
    //        catch (KlarnaException e)
    //        {
    //            _logger.Error("Error when processing Klarna part payment request", e);
    //            ErrorManager.GenerateError(e.FaultString);
    //            status = false;
    //        }
    //        return status;
    //    }

    // }
}
