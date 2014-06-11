using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider
{
    public class PaymentManager : IPaymentManager
    {
        private readonly IOrderFacade _orderFacade;
        private readonly ISettings _settings;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;

        public PaymentManager(IOrderFacade orderFacade, ISettings settings, IHasher hasher, IResultParser resultParser)
        {
            _orderFacade = orderFacade;
            _settings = settings;
            _hasher = hasher;
            _resultParser = resultParser;
        }

        public string Initialize(PaymentInformation payment)
        {
            payment.AddSettings(_settings);

            string hash = _hasher.Create(payment);
            string xmlResult = _orderFacade.Initialize(payment, hash);

            InitializeResult result = _resultParser.ParseInitializeXml(xmlResult);

            if (result.Success)
                return result.ReturnUrl;
            return null;
        }

        public string CompleteOrder(string orderRef)
        {
            string hash = _hasher.Create(_settings.AccountNumber, orderRef, _settings.EncryptionKey);
            string xmlResult = _orderFacade.Complete(_settings.AccountNumber, orderRef, hash);

            TransactionResult result = _resultParser.ParseTransactionXml(xmlResult);
            if (result.Success && result.TransactionStatus == TransactionStatus.Authorize)
                return result.TransactionNumber;
            return null;
        }

        public string Capture(int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues)
        {
            string hash = _hasher.Create(_settings.AccountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, _settings.EncryptionKey);
            string xmlResult = _orderFacade.Capture(_settings.AccountNumber, transactionNumber, amount, orderId, vatAmount,
                additionalValues, hash);

            TransactionResult result = _resultParser.ParseTransactionXml(xmlResult);
            if (result.Success && result.TransactionStatus == TransactionStatus.Capture)
                return result.TransactionNumber;
            return null;
        }
    }
}
