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

        public PaymentManager(ILogger logger, IOrderFacade orderFacade, IHasher hasher, IResultParser resultParser)
        {
            _logger = logger;
            _orderFacade = orderFacade;
            _hasher = hasher;
            _resultParser = resultParser;
        }

        public InitializeResult Initialize(Cart cart, PaymentInformation payment, bool ignoreOrderLines = false, bool ignoreCustomerAddress = false)
        {
            _logger.LogInfo(string.Format("Calling Initialize for cart with ID:{0}. PaymentInformation:{1}", cart.Id, payment));

            string hash = _hasher.Create(payment);
            string xmlResult = _orderFacade.Initialize(payment, hash);

            _logger.LogInfo(string.Format("Finished calling Initialize for cart with ID:{0}. Result:{1}", cart.Id, xmlResult));

            InitializeResult result = _resultParser.Deserialize<InitializeResult>(xmlResult);
            if (!result.Status.Success)
                return null;

            if (!ignoreOrderLines && PayExSettings.Instance.IncludeOrderLines)
                AddOrderLineItems(cart, payment, result);

            if (!ignoreCustomerAddress && PayExSettings.Instance.IncludeCustomerAddress)
                AddOrderAddress(cart, payment, result);

            return result;
        }

        public CompleteResult Complete(string orderRef)
        {
            _logger.LogInfo(string.Format("Calling Complete for orderRef:{0}.", orderRef));

            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, orderRef, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Complete(PayExSettings.Instance.AccountNumber, orderRef, hash);

            _logger.LogInfo(string.Format("Finished calling Complete for orderRef:{0}. Result:{1}", orderRef, xmlResult));

            return _resultParser.Deserialize<CompleteResult>(xmlResult);
        }

        public CaptureResult Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            _logger.LogInfo(string.Format("Calling Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}",
                transactionNumber, amount, orderId, vatAmount, additionalValues));

            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Capture(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount,
                additionalValues, hash);

            _logger.LogInfo(string.Format("Finished calling Capture for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
               transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult));

            CaptureResult result = _resultParser.Deserialize<CaptureResult>(xmlResult);
            if (result.Success)
                return result;
            return null;
        }

        public CreditResult Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            _logger.LogInfo(string.Format("Calling Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}",
                transactionNumber, amount, orderId, vatAmount, additionalValues));

            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Credit(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount,
              additionalValues, hash);

            _logger.LogInfo(string.Format("Finished calling Credit for TransactionNumber:{0}. Amount:{1}. OrderId:{2}. VatAmount:{3}. AdditionalValues:{4}. Result:{5}",
                transactionNumber, amount, orderId, vatAmount, additionalValues, xmlResult));

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                return result;
            return null;
        }

        public CreditResult CreditOrderLine(int transactionNumber, string itemNumber, string orderId)
        {
            _logger.LogInfo(string.Format("Calling CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
                transactionNumber, itemNumber, orderId));

            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, itemNumber, orderId, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.CreditOrderLine(PayExSettings.Instance.AccountNumber, transactionNumber, itemNumber, orderId, hash);

            _logger.LogInfo(string.Format("Finished calling CreditOrderLine for TransactionNumber:{0}. ItemNumber:{1}. OrderId:{2}.",
                transactionNumber, itemNumber, orderId));

            CreditResult result = _resultParser.Deserialize<CreditResult>(xmlResult);
            if (result.Success)
                return result;
            return null;
        }

        public TransactionResult GetTransactionDetails(int transactionNumber)
        {
            _logger.LogInfo(string.Format("Calling GetTransactionDetails for TransactionNumber:{0}.", transactionNumber));

            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.GetTransactionDetails(PayExSettings.Instance.AccountNumber, transactionNumber, hash);

            _logger.LogInfo(string.Format("Finished calling GetTransactionDetails for TransactionNumber:{0}.", transactionNumber));

            TransactionResult result = _resultParser.Deserialize<TransactionResult>(xmlResult);
            if (result.Status.Success)
                return result;
            return null;
        }

        public void PurchaseInvoiceSale(string orderRef, CustomerDetails customerDetails)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, orderRef, customerDetails, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.PurchaseInvoiceSale(PayExSettings.Instance.AccountNumber, orderRef, customerDetails, hash);
            _logger.LogDebug(xmlResult);
            // TODO
        }

        public PurchasePartPaymentSaleResult PurchasePartPaymentSale(string orderRef, CustomerDetails customerDetails)
        {
            _logger.LogInfo(string.Format("Calling PurchasePartPaymentSale for order with orderRef:{0}. CustomerDetails:{1}", orderRef, customerDetails));

            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, orderRef, customerDetails, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.PurchasePartPaymentSale(PayExSettings.Instance.AccountNumber, orderRef, customerDetails, hash);

            _logger.LogInfo(string.Format("Finished calling PurchasePartPaymentSale for order with orderRef:{0}. Result:{1}", orderRef, xmlResult));
            return _resultParser.Deserialize<PurchasePartPaymentSaleResult>(xmlResult);
        }

        private void AddOrderAddress(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            _logger.LogInfo(string.Format("Calling AddOrderAddress for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}", cart.Id, payment, initializeResult));

            PayExAddress address = CartHelper.OrderAddress(cart, payment, initializeResult);
            string hash = _hasher.Create(address);
            string xmlResult = _orderFacade.AddOrderAddress(address, hash);

            _logger.LogInfo(string.Format("Finished calling AddOrderAddress for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}. Result:{3}",
                cart.Id, payment, initializeResult, xmlResult));
        }

        private void AddOrderLineItems(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            _logger.LogInfo(string.Format("Calling AddOrderLineItems for cart with ID:{0}. PaymentInformation:{1}. InitializeResult:{2}", cart.Id, payment, initializeResult));

            List<OrderLine> orderlines = CartHelper.OrderLines(cart, payment, initializeResult);
            foreach (OrderLine orderline in orderlines)
            {
                string hash = _hasher.Create(orderline);
                string result = _orderFacade.AddOrderLine(orderline, hash);

                _logger.LogInfo(string.Format("Added OrderLineItem for cart with ID:{0}. OrderLine:{1}. Result:{2}",
                    cart.Id, orderline, result));
            }
        }
    }
}
