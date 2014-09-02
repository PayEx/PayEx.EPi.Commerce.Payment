using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Facades;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider
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

        public string Initialize(PaymentInformation payment)
        {
            payment.AddSettings(PayExSettings.Instance);

            string hash = _hasher.Create(payment);
            string xmlResult = _orderFacade.Initialize(payment, hash);

            InitializeResult result = _resultParser.ParseInitializeXml(xmlResult);

            if (result.Success)
                return result.ReturnUrl;
            return null;
        }

        public string CompleteOrder(string orderRef)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, orderRef, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Complete(PayExSettings.Instance.AccountNumber, orderRef, hash);

            TransactionResult result = _resultParser.ParseTransactionXml(xmlResult);
            if (result.Success && result.TransactionStatus == TransactionStatus.Authorize)
                return result.TransactionNumber;
            return null;
        }

        public string Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, PayExSettings.Instance.EncryptionKey);
            string xmlResult = _orderFacade.Capture(PayExSettings.Instance.AccountNumber, transactionNumber, amount, orderId, vatAmount,
                additionalValues, hash);

            TransactionResult result = _resultParser.ParseTransactionXml(xmlResult);
            if (result.Success && result.TransactionStatus == TransactionStatus.Capture)
                return result.TransactionNumber;
            return null;
        }
    }
}
