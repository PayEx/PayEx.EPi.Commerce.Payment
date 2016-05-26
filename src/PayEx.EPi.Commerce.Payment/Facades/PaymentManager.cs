using System.Collections.Generic;
using EPiServer.Logging.Compatibility;
using Mediachase.Commerce.Orders;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Facades
{
    internal class PaymentManager : IPaymentManager
    {
        private readonly IOrderFacade _orderFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;
        private readonly IPayExSettings _payExSettings;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public PaymentManager(IOrderFacade orderFacade, IHasher hasher, IResultParser resultParser, IPayExSettings payExSettings)
        {
            _orderFacade = orderFacade;
            _hasher = hasher;
            _resultParser = resultParser;
            _payExSettings = payExSettings;
        }

        public InitializeResult Initialize(Cart cart, PaymentInformation payment, bool ignoreOrderLines = false, bool ignoreCustomerAddress = false)
        {
            Log.InfoFormat("Calling Initialize for cart with ID:{0}. PaymentInformation:{1}", cart.Id, payment);

            string hash = _hasher.Create(_payExSettings.AccountNumber, payment, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Initialize(_payExSettings.AccountNumber, payment, hash);

            InitializeResult result = _resultParser.Deserialize<InitializeResult>(xmlResult);
            if (!result.Status.Success)
            {
                Log.ErrorFormat("Error when calling Initialize for cart with ID:{0}. Result:{1}", cart.Id, xmlResult);
                return result;
            }

            Log.InfoFormat("Successfully called Initialize for cart with ID:{0}. Result:{1}", cart.Id, xmlResult);

            if (!ignoreOrderLines && _payExSettings.IncludeOrderLines)
                AddOrderLineItems(cart, payment, result);

            if (!ignoreCustomerAddress && _payExSettings.IncludeCustomerAddress)
                AddOrderAddress(cart, payment, result);

            return result;
        }

        public CompleteResult Complete(string orderRef)
        {
            Log.InfoFormat("Calling Complete for orderRef:{0}.", orderRef);

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Complete(_payExSettings.AccountNumber, orderRef, hash);

            CompleteResult result = _resultParser.Deserialize<CompleteResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called Complete for orderRef:{0}. Result:{1}", orderRef, xmlResult);
            else
                Log.ErrorFormat("Error when calling Complete for orderRef:{0}. Result:{1}", orderRef, xmlResult);
            return result;
        }

        public CaptureResult Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            Log.InfoFormat("Calling Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}",
                transactionNumber, amount, orderId, vatAmount, additionalValues);

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Capture(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount,
                additionalValues, hash);

            CaptureResult result = _resultParser.Deserialize<CaptureResult>(xmlResult);
            if (result.Success)
                Log.InfoFormat("Successfully called Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
                    transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult);
            else
                Log.ErrorFormat("Error when calling Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
                   transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult);
            return result;
        }

        public CreditResult Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            Log.InfoFormat("Calling Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}",
                transactionNumber, amount, orderId, vatAmount, additionalValues);

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Credit(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount,
              additionalValues, hash);

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                Log.InfoFormat("Successfully called Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
              transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult);
            else
                 Log.ErrorFormat("Error when calling Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
              transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult);
            return result;
        }

        public CreditResult CreditOrderLine(int transactionNumber, string itemNumber, string orderId)
        {
            Log.InfoFormat("Calling CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
                transactionNumber, itemNumber, orderId);

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, itemNumber, orderId, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.CreditOrderLine(_payExSettings.AccountNumber, transactionNumber, itemNumber, orderId, hash);

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                Log.InfoFormat("Successfully called CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
               transactionNumber, itemNumber, orderId);
            else
                 Log.ErrorFormat("Error when calling CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
               transactionNumber, itemNumber, orderId);
            return result;
        }

        public TransactionResult GetTransactionDetails(int transactionNumber)
        {
            Log.InfoFormat("Calling GetTransactionDetails for TransactionNumber:{0}.", transactionNumber);

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.GetTransactionDetails(_payExSettings.AccountNumber, transactionNumber, hash);

            TransactionResult result = _resultParser.Deserialize<TransactionResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called GetTransactionDetails for TransactionNumber:{0}. Result:{1}", transactionNumber, xmlResult);
            else
                 Log.ErrorFormat("Error when calling GetTransactionDetails for TransactionNumber:{0}. Result:{1}", transactionNumber, xmlResult);
            return result;
        }

        public PurchaseInvoiceSaleResult PurchaseInvoiceSale(string orderRef, CustomerDetails customerDetails)
        {
            Log.InfoFormat("Calling PurchaseInvoiceSale for order with orderRef:{0}. CustomerDetails:{1}", orderRef, customerDetails);

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchaseInvoiceSale(_payExSettings.AccountNumber, orderRef, customerDetails, hash);

            PurchaseInvoiceSaleResult result = _resultParser.Deserialize<PurchaseInvoiceSaleResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called PurchaseInvoiceSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            else
                Log.ErrorFormat("Error when calling PurchaseInvoiceSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            return result;
        }

        public PurchasePartPaymentSaleResult PurchasePartPaymentSale(string orderRef, CustomerDetails customerDetails)
        {
            Log.InfoFormat("Calling PurchasePartPaymentSale for order with orderRef:{0}. CustomerDetails:{1}", orderRef, customerDetails);

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchasePartPaymentSale(_payExSettings.AccountNumber, orderRef, customerDetails, hash);

            PurchasePartPaymentSaleResult result = _resultParser.Deserialize<PurchasePartPaymentSaleResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called PurchasePartPaymentSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            else
                 Log.ErrorFormat("Error when calling PurchasePartPaymentSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            return result;
        }

        private void AddOrderAddress(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            Log.InfoFormat("Calling AddOrderAddress for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}", cart.Id, payment, initializeResult);

            PayExAddress address = CartHelper.OrderAddress(cart, payment, initializeResult);
            string hash = _hasher.Create(_payExSettings.AccountNumber, address, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.AddOrderAddress(_payExSettings.AccountNumber, address, hash);

            Log.InfoFormat("Finished calling AddOrderAddress for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}. Result:{3}",
                cart.Id, payment, initializeResult, xmlResult);
        }

        private void AddOrderLineItems(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            Log.InfoFormat("Calling AddOrderLineItems for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}", cart.Id, payment, initializeResult);

            List<OrderLine> orderlines = CartHelper.OrderLines(cart, payment, initializeResult);
            foreach (OrderLine orderline in orderlines)
            {
                string hash = _hasher.Create(_payExSettings.AccountNumber, orderline, _payExSettings.EncryptionKey);
                string result = _orderFacade.AddOrderLine(_payExSettings.AccountNumber, orderline, hash);

                Log.InfoFormat("Added OrderLineItem for cart with ID:{0}. OrderLine:{1}. Result:{2}",
                    cart.Id, orderline, result);
            }
        }

        public DeliveryAddressResult GetApprovedDeliveryAddress(string orderRef)
        {
            Log.InfoFormat("Calling GetApprovedDeliveryAddress for orderRef:{0}.", orderRef);

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.GetApprovedDeliveryAddress(_payExSettings.AccountNumber, orderRef, hash);

            DeliveryAddressResult result = _resultParser.Deserialize<DeliveryAddressResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called GetApprovedDeliveryAddress for orderRef:{0}. Result:{1}", orderRef, xmlResult);
            else
                Log.ErrorFormat("Error when calling GetApprovedDeliveryAddress for orderRef:{0}. Result:{1}", orderRef, xmlResult);
            return result;
        }

        public FinalizeTransactionResult FinalizeTransaction(string orderRef, long amount, long vatAmount, string clientIpAddress)
        {
            Log.InfoFormat("Calling FinalizeTransaction for orderRef:{0}.", orderRef);

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, amount, vatAmount, clientIpAddress, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.FinalizeTransaction(_payExSettings.AccountNumber, orderRef, amount, vatAmount, clientIpAddress, hash);

            FinalizeTransactionResult result = _resultParser.Deserialize<FinalizeTransactionResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called FinalizeTransaction for orderRef:{0}. Result:{1}", orderRef, xmlResult);
            else
                Log.ErrorFormat("Error when calling FinalizeTransaction for orderRef:{0}. Result:{1}", orderRef, xmlResult);
            return result;
        }

        public LegalAddressResult GetAddressByPaymentMethod(string paymentMethod, string ssn, string zipcode, string countryCode, string ipAddress)
        {
            Log.InfoFormat("Calling GetAddressByPaymentMethod for paymentMethod:{0}.", paymentMethod);

            string hash = _hasher.Create(_payExSettings.AccountNumber, paymentMethod, ssn, zipcode, countryCode, ipAddress, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.GetAddressByPaymentMethod(_payExSettings.AccountNumber, paymentMethod, ssn, zipcode, countryCode, ipAddress, hash);

            LegalAddressResult result = _resultParser.Deserialize<LegalAddressResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called GetAddressByPaymentMethod for paymentMethod:{0}. Result:{1}", paymentMethod, xmlResult);
            else
                Log.ErrorFormat("Error when calling GetApprovedDeliveryAddress for paymentMethod:{0}. Result:{1}", paymentMethod, xmlResult);
            return result;
        }

        public PurchaseInvoiceSaleResult PurchaseFinancingInvoice(string orderRef, string paymentMethod, CustomerDetails customerDetails)
        {
            Log.InfoFormat("Calling PurchaseFinancingInvoice for order with orderRef:{0}. CustomerDetails:{1}", orderRef, customerDetails);

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, paymentMethod, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchaseFinancingInvoice(_payExSettings.AccountNumber, orderRef, paymentMethod, customerDetails, hash);

            PurchaseInvoiceSaleResult result = _resultParser.Deserialize<PurchaseInvoiceSaleResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called PurchaseFinancingInvoice for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            else
                Log.ErrorFormat("Error when calling PurchaseFinancingInvoice for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            return result;
        }

        public InvoiceLinkResult GetInvoiceLinkForFinancingInvoicePurchase(int transactionNumber)
        {
            Log.InfoFormat("Calling InvoiceLinkGet for order with transactionNumber:{0}.", transactionNumber);

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.InvoiceLinkGet(_payExSettings.AccountNumber, transactionNumber, hash);

            InvoiceLinkResult result = _resultParser.Deserialize<InvoiceLinkResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called InvoiceLinkGet for order with transactionNumber:{0}. Result:{1}", transactionNumber, xmlResult);
            else
                Log.ErrorFormat("Error when calling InvoiceLinkGet for order with transactionNumber:{0}. Result:{1}", transactionNumber, xmlResult);
            return result;
        }

        public PurchaseSwishResult PurchaseSwish(string orderRef, string paymentMethod, CustomerDetails customerDetails)
        {
            Log.InfoFormat("Calling PurchaseSwish for order with orderRef:{0}. CustomerDetails:{1}", orderRef, customerDetails);

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, paymentMethod, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PreparePurchaseSwish(_payExSettings.AccountNumber, orderRef, customerDetails.MobilePhone, customerDetails.IpAddress, hash);

            PurchaseSwishResult result = _resultParser.Deserialize<PurchaseSwishResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called PurchaseSwishResult for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            else
                Log.ErrorFormat("Error when calling PurchaseSwishResult for order with orderRef:{0}. Result:{1}", orderRef, xmlResult);
            return result;
        }
    }
}
