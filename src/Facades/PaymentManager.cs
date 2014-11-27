using System.Collections.Generic;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.Result;
using Epinova.PayExProvider.Payment;
using EPiServer.Data;
using Mediachase.Commerce.Orders;

namespace Epinova.PayExProvider.Facades
{
    public class PaymentManager : IPaymentManager
    {
        private readonly IOrderFacade _orderFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;

        public PaymentManager()
        {
            _orderFacade = new Order();
            _hasher = new Hash();
            _resultParser = new ResultParser();
        }

        public InitializeResult Initialize(Cart cart, PaymentInformation payment)
        {
            string hash = _hasher.Create(payment);
            string xmlResult = _orderFacade.Initialize(payment, hash);

            InitializeResult result = _resultParser.Deserialize<InitializeResult>(xmlResult);
            if (!result.Status.Success)
                return null;

            AddOrderLineItems(cart, payment, result);
            AddOrderAddress(cart, payment, result);
            return result;
        }

        public CompleteResult Complete(string orderRef)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, orderRef, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Complete(PayExSettings.Instance.AccountNumber, orderRef, hash);

            return _resultParser.Deserialize<CompleteResult>(xmlResult);
        }

        public CaptureResult Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Capture(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount,
                additionalValues, hash);

            CaptureResult result = _resultParser.Deserialize<CaptureResult>(xmlResult);
            if (result.Success)
                return result;
            return null;
        }

        public string Credit(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Credit(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount,
              additionalValues, hash);

            TransactionResult result = _resultParser.ParseTransactionXml(xmlResult);
            if (result.Success && result.TransactionStatus == TransactionStatus.Credit)
                return result.TransactionNumber;
            return null;
        }

        public string CreditOrderLine(int transactionNumber, string itemNumber, string orderId)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, itemNumber, orderId, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.CreditOrderLine(PayExSettings.Instance.AccountNumber, transactionNumber, itemNumber, orderId, hash);

            TransactionResult result = _resultParser.ParseTransactionXml(xmlResult);
            if (result.Success && result.TransactionStatus == TransactionStatus.Credit)
                return result.TransactionNumber;
            return null;
        }

        public TransactionResult GetTransactionDetails(int transactionNumber)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.GetTransactionDetails(PayExSettings.Instance.AccountNumber, transactionNumber, hash);
            TransactionResult result = _resultParser.ParseTransactionXml(xmlResult);
            if (result.Success)
                return result;
            return null;
        }

        private void AddOrderAddress(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            PayExAddress address = CartHelper.OrderAddress(cart, payment, initializeResult);
            string hash = _hasher.Create(address);
            string result = _orderFacade.AddOrderAddress(address, hash);
        }

        private void AddOrderLineItems(Cart cart, PaymentInformation payment, InitializeResult initializeResult)
        {
            List<OrderLine> orderlines = CartHelper.OrderLines(cart, payment, initializeResult);
            foreach (OrderLine orderline in orderlines)
            {
                string hash = _hasher.Create(orderline);
                string result = _orderFacade.AddOrderLine(orderline, hash);
            }
        }
    }
}
