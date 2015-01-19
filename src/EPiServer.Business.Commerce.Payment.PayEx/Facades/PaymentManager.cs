using System.Collections.Generic;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Facades
{
    public class PaymentManager : IPaymentManager
    {
        private readonly ILogger _logger;
        private readonly IOrderFacade _orderFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;
        private readonly IPayExSettings _payExSettings;

        public PaymentManager(ILogger logger, IOrderFacade orderFacade, IHasher hasher, IResultParser resultParser, IPayExSettings payExSettings)
        {
            _logger = logger;
            _orderFacade = orderFacade;
            _hasher = hasher;
            _resultParser = resultParser;
            _payExSettings = payExSettings;
        }

        public InitializeResult Initialize(Cart cart, PaymentInformation payment, bool ignoreOrderLines = false, bool ignoreCustomerAddress = false)
        {
            _logger.LogInfo(string.Format("Calling Initialize for cart with ID:{0}. PaymentInformation:{1}", cart.Id, payment));

            string hash = _hasher.Create(_payExSettings.AccountNumber, payment, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Initialize(_payExSettings.AccountNumber, payment, hash);

            InitializeResult result = _resultParser.Deserialize<InitializeResult>(xmlResult);
            if (!result.Status.Success)
            {
                _logger.LogError(string.Format("Error when calling Initialize for cart with ID:{0}. Result:{1}", cart.Id, xmlResult));
                return result;
            }

            _logger.LogInfo(string.Format("Successfully called Initialize for cart with ID:{0}. Result:{1}", cart.Id, xmlResult));

            if (!ignoreOrderLines && _payExSettings.IncludeOrderLines)
                AddOrderLineItems(cart, payment, result);

            if (!ignoreCustomerAddress && _payExSettings.IncludeCustomerAddress)
                AddOrderAddress(cart, payment, result);

            return result;
        }

        public CompleteResult Complete(string orderRef)
        {
            _logger.LogInfo(string.Format("Calling Complete for orderRef:{0}.", orderRef));

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Complete(_payExSettings.AccountNumber, orderRef, hash);

            CompleteResult result = _resultParser.Deserialize<CompleteResult>(xmlResult);
            if (result.Status.Success)
                _logger.LogInfo(string.Format("Successfully called Complete for orderRef:{0}. Result:{1}", orderRef, xmlResult));
            else
                _logger.LogError(string.Format("Error when calling Complete for orderRef:{0}. Result:{1}", orderRef, xmlResult));
            return result;
        }

        public CaptureResult Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            _logger.LogInfo(string.Format("Calling Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}",
                transactionNumber, amount, orderId, vatAmount, additionalValues));

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Capture(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount,
                additionalValues, hash);

            CaptureResult result = _resultParser.Deserialize<CaptureResult>(xmlResult);
            if (result.Success)
                _logger.LogInfo(string.Format("Successfully called Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
                    transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult));
            else
                _logger.LogError(string.Format("Error when calling Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
                   transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult));
            return result;
        }

        public CreditResult Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            _logger.LogInfo(string.Format("Calling Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}",
                transactionNumber, amount, orderId, vatAmount, additionalValues));

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.Credit(_payExSettings.AccountNumber, transactionNumber, amount, orderId, vatAmount,
              additionalValues, hash);

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                _logger.LogInfo(string.Format("Successfully called Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
              transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult));
            else
                _logger.LogError(string.Format("Error when calling Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
              transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult));
            return result;
        }

        public CreditResult CreditOrderLine(int transactionNumber, string itemNumber, string orderId)
        {
            _logger.LogInfo(string.Format("Calling CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
                transactionNumber, itemNumber, orderId));

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, itemNumber, orderId, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.CreditOrderLine(_payExSettings.AccountNumber, transactionNumber, itemNumber, orderId, hash);

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                _logger.LogInfo(string.Format("Successfully called CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
               transactionNumber, itemNumber, orderId));
            else
                _logger.LogError(string.Format("Error when calling CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
               transactionNumber, itemNumber, orderId));
            return result;
        }

        public TransactionResult GetTransactionDetails(int transactionNumber)
        {
            _logger.LogInfo(string.Format("Calling GetTransactionDetails for TransactionNumber:{0}.", transactionNumber));

            string hash = _hasher.Create(_payExSettings.AccountNumber, transactionNumber, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.GetTransactionDetails(_payExSettings.AccountNumber, transactionNumber, hash);

            TransactionResult result = _resultParser.Deserialize<TransactionResult>(xmlResult);
            if (result.Status.Success)
                _logger.LogInfo(string.Format("Successfully called GetTransactionDetails for TransactionNumber:{0}. Result:{1}", transactionNumber, xmlResult));
            else
                _logger.LogError(string.Format("Error when calling GetTransactionDetails for TransactionNumber:{0}. Result:{1}", transactionNumber, xmlResult));
            return result;
        }

        public PurchaseInvoiceSaleResult PurchaseInvoiceSale(string orderRef, CustomerDetails customerDetails)
        {
            _logger.LogInfo(string.Format("Calling PurchaseInvoiceSale for order with orderRef:{0}. CustomerDetails:{1}", orderRef, customerDetails));

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchaseInvoiceSale(_payExSettings.AccountNumber, orderRef, customerDetails, hash);

            PurchaseInvoiceSaleResult result = _resultParser.Deserialize<PurchaseInvoiceSaleResult>(xmlResult);
            if (result.Status.Success)
                _logger.LogInfo(string.Format("Successfully called PurchaseInvoiceSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult));
            else
                _logger.LogError(string.Format("Error when calling PurchaseInvoiceSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult));
            return result;
        }

        public PurchasePartPaymentSaleResult PurchasePartPaymentSale(string orderRef, CustomerDetails customerDetails)
        {
            _logger.LogInfo(string.Format("Calling PurchasePartPaymentSale for order with orderRef:{0}. CustomerDetails:{1}", orderRef, customerDetails));

            string hash = _hasher.Create(_payExSettings.AccountNumber, orderRef, customerDetails, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.PurchasePartPaymentSale(_payExSettings.AccountNumber, orderRef, customerDetails, hash);

            PurchasePartPaymentSaleResult result = _resultParser.Deserialize<PurchasePartPaymentSaleResult>(xmlResult);
            if (result.Status.Success)
                _logger.LogInfo(string.Format("Successfully called PurchasePartPaymentSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult));
            else
                _logger.LogError(string.Format("Error when calling PurchasePartPaymentSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult));
            return result;
        }

        private void AddOrderAddress(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            _logger.LogInfo(string.Format("Calling AddOrderAddress for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}", cart.Id, payment, initializeResult));

            PayExAddress address = CartHelper.OrderAddress(cart, payment, initializeResult);
            string hash = _hasher.Create(_payExSettings.AccountNumber, address, _payExSettings.EncryptionKey);
            string xmlResult = _orderFacade.AddOrderAddress(_payExSettings.AccountNumber, address, hash);

            _logger.LogInfo(string.Format("Finished calling AddOrderAddress for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}. Result:{3}",
                cart.Id, payment, initializeResult, xmlResult));
        }

        private void AddOrderLineItems(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            _logger.LogInfo(string.Format("Calling AddOrderLineItems for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}", cart.Id, payment, initializeResult));

            List<OrderLine> orderlines = CartHelper.OrderLines(cart, payment, initializeResult);
            foreach (OrderLine orderline in orderlines)
            {
                string hash = _hasher.Create(_payExSettings.AccountNumber, orderline, _payExSettings.EncryptionKey);
                string result = _orderFacade.AddOrderLine(_payExSettings.AccountNumber, orderline, hash);

                _logger.LogInfo(string.Format("Added OrderLineItem for cart with ID:{0}. OrderLine:{1}. Result:{2}",
                    cart.Id, orderline, result));
            }
        }
    }
}
