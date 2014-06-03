
using Epinova.PayExProvider.Contracts;

namespace Epinova.PayExProvider.Facades
{
    public class Order : IOrderFacade
    {
        private readonly PxOrder.PxOrderSoapClient _client;

        public Order()
        {
            _client = new PxOrder.PxOrderSoapClient();
        }

        public string Initialize(long accountNumber, string purchaseOperation, long price, string priceArgList, string currency, int vat, string orderId, string productNumber, string description,
            string clientIpAddress, string clientIdentifier, string additionalValues, string externalId, string returnUrl, string view, string agreementRef, string cancelUrl, string clientLanguage, string hash)
        {
            return _client.Initialize8(accountNumber, purchaseOperation, price, priceArgList, currency, vat, orderId,
                                      productNumber, description, clientIpAddress, clientIdentifier, additionalValues,
                                      externalId, returnUrl, view, agreementRef, cancelUrl, clientLanguage, hash);
        }

        public string Complete(long accountNumber, string orderRef, string hash)
        {
            return _client.Complete(accountNumber, orderRef, hash);
        }

        public string Capture(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues, string hash)
        {
            return _client.Capture5(accountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, hash);
        }
    }
}
