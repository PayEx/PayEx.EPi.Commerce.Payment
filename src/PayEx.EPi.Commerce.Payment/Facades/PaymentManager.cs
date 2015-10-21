using System.Collections.Generic;
using log4net;
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
            Log.Info($"Calling Initialize for cart with ID:{cart.Id}. PaymentInformation:{payment}");

            string hash = _hasher.Create(_payExSettings.AccountNumber, payment, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Initialize(_payExSettings.AccountNumber, payment, hash);

            InitializeResult result = _resultParser.Deserialize<InitializeResult>(xmlResult);
            if (!result.Status.Success)
            {
                Log.Error($"Error when calling Initialize for cart with ID:{cart.Id}. Result:{xmlResult}");
                return result;
            }

            Log.Info($"Successfully called Initialize for cart with ID:{cart.Id}. Result:{xmlResult}");

            if (!ignoreOrderLines && _payExSettings.IncludeOrderLines)
                AddOrderLineItems(cart, payment, result);

            if (!ignoreCustomerAddress && _payExSettings.IncludeCustomerAddress)
                AddOrderAddress(cart, payment, result);

            return result;
        }

        public CompleteResult Complete(string orderRef)
        {
            Log.Info($"Calling Complete for orderRef:{orderRef}.");

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Complete(_payExSettings.AccountNumber, orderRef, hash);

            CompleteResult result = _resultParser.Deserialize<CompleteResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called Complete for orderRef:{orderRef}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling Complete for orderRef:{orderRef}. Result:{xmlResult}");
            return result;
        }

        public CaptureResult Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            Log.Info($"Calling Capture for TransactionNumber:{transactionNumber}. Amount:{amount}. OrderId:{orderId}. VatAmount:{vatAmount}. AdditionalValues:{additionalValues}");

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Capture(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount,
                additionalValues, hash);

            CaptureResult result = _resultParser.Deserialize<CaptureResult>(xmlResult);
            if (result.Success)
                Log.Info($"Successfully called Capture for TransactionNumber:{transactionNumber}. Amount:{amount}. OrderId:{orderId}. VatAmount:{vatAmount}. AdditionalValues:{additionalValues}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling Capture for TransactionNumber:{transactionNumber}. Amount:{amount}. OrderId:{orderId}. VatAmount:{vatAmount}. AdditionalValues:{additionalValues}. Result:{xmlResult}");
            return result;
        }

        public CreditResult Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            Log.Info($"Calling Credit for TransactionNumber:{transactionNumber}. Amount:{amount}. OrderId:{orderId}. VatAmount:{vatAmount}. AdditionalValues:{additionalValues}");

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Credit(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount,
              additionalValues, hash);

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                Log.Info($"Successfully called Credit for TransactionNumber:{transactionNumber}. Amount:{amount}. OrderId:{orderId}. VatAmount:{vatAmount}. AdditionalValues:{additionalValues}. Result:{xmlResult}");
            else
                 Log.Error($"Error when calling Credit for TransactionNumber:{transactionNumber}. Amount:{amount}. OrderId:{orderId}. VatAmount:{vatAmount}. AdditionalValues:{additionalValues}. Result:{xmlResult}");
            return result;
        }

        public CreditResult CreditOrderLine(int transactionNumber, string itemNumber, string orderId)
        {
            Log.Info($"Calling CreditOrderLine for TransactionNumber:{transactionNumber}. ItemNumber:{itemNumber}. OrderId:{orderId}.");

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, itemNumber, orderId, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.CreditOrderLine(_payExSettings.AccountNumber, transactionNumber, itemNumber, orderId, hash);

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                Log.Info($"Successfully called CreditOrderLine for TransactionNumber:{transactionNumber}. ItemNumber:{itemNumber}. OrderId:{orderId}.");
            else
                 Log.Error($"Error when calling CreditOrderLine for TransactionNumber:{transactionNumber}. ItemNumber:{itemNumber}. OrderId:{orderId}.");
            return result;
        }

        public TransactionResult GetTransactionDetails(int transactionNumber)
        {
            Log.Info($"Calling GetTransactionDetails for TransactionNumber:{transactionNumber}.");

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.GetTransactionDetails(_payExSettings.AccountNumber, transactionNumber, hash);

            TransactionResult result = _resultParser.Deserialize<TransactionResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called GetTransactionDetails for TransactionNumber:{transactionNumber}. Result:{xmlResult}");
            else
                 Log.Error($"Error when calling GetTransactionDetails for TransactionNumber:{transactionNumber}. Result:{xmlResult}");
            return result;
        }

        public PurchaseInvoiceSaleResult PurchaseInvoiceSale(string orderRef, CustomerDetails customerDetails)
        {
            Log.Info($"Calling PurchaseInvoiceSale for order with orderRef:{orderRef}. CustomerDetails:{customerDetails}");

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchaseInvoiceSale(_payExSettings.AccountNumber, orderRef, customerDetails, hash);

            PurchaseInvoiceSaleResult result = _resultParser.Deserialize<PurchaseInvoiceSaleResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called PurchaseInvoiceSale for order with orderRef:{orderRef}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling PurchaseInvoiceSale for order with orderRef:{orderRef}. Result:{xmlResult}");
            return result;
        }

        public PurchasePartPaymentSaleResult PurchasePartPaymentSale(string orderRef, CustomerDetails customerDetails)
        {
            Log.Info($"Calling PurchasePartPaymentSale for order with orderRef:{orderRef}. CustomerDetails:{customerDetails}");

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchasePartPaymentSale(_payExSettings.AccountNumber, orderRef, customerDetails, hash);

            PurchasePartPaymentSaleResult result = _resultParser.Deserialize<PurchasePartPaymentSaleResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called PurchasePartPaymentSale for order with orderRef:{orderRef}. Result:{xmlResult}");
            else
                 Log.Error($"Error when calling PurchasePartPaymentSale for order with orderRef:{orderRef}. Result:{xmlResult}");
            return result;
        }

        private void AddOrderAddress(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            Log.Info($"Calling AddOrderAddress for cart with ID:{cart.Id}. PaymentInformation:{payment}. InitializeResult:{initializeResult}");

            PayExAddress address = CartHelper.OrderAddress(cart, payment, initializeResult);
            string hash = _hasher.Create(_payExSettings.AccountNumber, address, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.AddOrderAddress(_payExSettings.AccountNumber, address, hash);

            Log.Info($"Finished calling AddOrderAddress for cart with ID:{cart.Id}. PaymentInformation:{payment}. InitializeResult:{initializeResult}. Result:{xmlResult}");
        }

        private void AddOrderLineItems(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            Log.Info($"Calling AddOrderLineItems for cart with ID:{cart.Id}. PaymentInformation:{payment}. InitializeResult:{initializeResult}");

            List<OrderLine> orderlines = CartHelper.OrderLines(cart, payment, initializeResult);
            foreach (OrderLine orderline in orderlines)
            {
                string hash = _hasher.Create(_payExSettings.AccountNumber, orderline, _payExSettings.EncryptionKey);
                string result = _orderFacade.AddOrderLine(_payExSettings.AccountNumber, orderline, hash);

                Log.Info($"Added OrderLineItem for cart with ID:{cart.Id}. OrderLine:{orderline}. Result:{result}");
            }
        }

        public DeliveryAddressResult GetApprovedDeliveryAddress(string orderRef)
        {
            Log.Info($"Calling GetApprovedDeliveryAddress for orderRef:{orderRef}.");

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.GetApprovedDeliveryAddress(_payExSettings.AccountNumber, orderRef, hash);

            DeliveryAddressResult result = _resultParser.Deserialize<DeliveryAddressResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called GetApprovedDeliveryAddress for orderRef:{orderRef}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling GetApprovedDeliveryAddress for orderRef:{orderRef}. Result:{xmlResult}");
            return result;
        }

        public FinalizeTransactionResult FinalizeTransaction(string orderRef, long amount, long vatAmount, string clientIpAddress)
        {
            Log.Info($"Calling FinalizeTransaction for orderRef:{orderRef}.");

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, amount, vatAmount, clientIpAddress, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.FinalizeTransaction(_payExSettings.AccountNumber, orderRef, amount, vatAmount, clientIpAddress, hash);

            FinalizeTransactionResult result = _resultParser.Deserialize<FinalizeTransactionResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called FinalizeTransaction for orderRef:{orderRef}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling FinalizeTransaction for orderRef:{orderRef}. Result:{xmlResult}");
            return result;
        }

        public LegalAddressResult GetAddressByPaymentMethod(string paymentMethod, string ssn, string zipcode, string countryCode, string ipAddress)
        {
            Log.Info($"Calling GetAddressByPaymentMethod for paymentMethod:{paymentMethod}.");

            string hash = _hasher.Create(_payExSettings.AccountNumber, paymentMethod, ssn, zipcode, countryCode, ipAddress, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.GetAddressByPaymentMethod(_payExSettings.AccountNumber, paymentMethod, ssn, zipcode, countryCode, ipAddress, hash);

            LegalAddressResult result = _resultParser.Deserialize<LegalAddressResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called GetAddressByPaymentMethod for paymentMethod:{paymentMethod}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling GetApprovedDeliveryAddress for paymentMethod:{paymentMethod}. Result:{xmlResult}");
            return result;
        }

        public PurchaseInvoiceSaleResult PurchaseFinancingInvoice(string orderRef, string paymentMethod, CustomerDetails customerDetails)
        {
            Log.Info($"Calling PurchaseFinancingInvoice for order with orderRef:{orderRef}. CustomerDetails:{customerDetails}");

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, paymentMethod, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchaseFinancingInvoice(_payExSettings.AccountNumber, orderRef, paymentMethod, customerDetails, hash);

            PurchaseInvoiceSaleResult result = _resultParser.Deserialize<PurchaseInvoiceSaleResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called PurchaseFinancingInvoice for order with orderRef:{orderRef}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling PurchaseFinancingInvoice for order with orderRef:{orderRef}. Result:{xmlResult}");
            return result;
        }

        public InvoiceLinkResult GetInvoiceLinkForFinancingInvoicePurchase(int transactionNumber)
        {
            Log.Info($"Calling InvoiceLinkGet for order with transactionNumber:{transactionNumber}.");

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.InvoiceLinkGet(_payExSettings.AccountNumber, transactionNumber, hash);

            InvoiceLinkResult result = _resultParser.Deserialize<InvoiceLinkResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called InvoiceLinkGet for order with transactionNumber:{transactionNumber}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling InvoiceLinkGet for order with transactionNumber:{transactionNumber}. Result:{xmlResult}");
            return result;
        }
    }
}
