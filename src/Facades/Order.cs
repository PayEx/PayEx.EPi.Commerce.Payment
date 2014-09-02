
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Facades
{
    public class Order : IOrderFacade
    {
        private PxOrder.PxOrderSoapClient _client;

        private PxOrder.PxOrderSoapClient Client
        {
            get
            {
                if (_client == null)
                    _client = new PxOrder.PxOrderSoapClient();
                return _client;
            }
        }

        public string Initialize(PaymentInformation payment, string hash)
        {
            return Client.Initialize8(payment.AccountNumber, payment.PurchaseOperation, payment.Price, payment.PriceArgList, payment.Currency, payment.Vat, payment.OrderId,
                                      payment.ProductNumber, payment.Description, payment.ClientIpAddress, payment.ClientIdentifier, payment.AdditionalValues,
                                      string.Empty, payment.ReturnUrl, payment.View, payment.AgreementRef, payment.CancelUrl, payment.ClientLanguage, hash);
        }

        public string Complete(long accountNumber, string orderRef, string hash)
        {
            return Client.Complete(accountNumber, orderRef, hash);
        }

        public string Capture(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues, string hash)
        {
            return Client.Capture5(accountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, hash);
        }
    }
}
