
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Facades
{
    public class Order : IOrderFacade
    {
        private readonly PxOrder.PxOrderSoapClient _client;

        public Order()
        {
            _client = new PxOrder.PxOrderSoapClient();
        }

        public string Initialize(PaymentInformation payment, string hash)
        {
            return _client.Initialize8(payment.AccountNumber, payment.PurchaseOperation, payment.Price, payment.PriceArgList, payment.Currency, payment.Vat, payment.OrderId,
                                      payment.ProductNumber, payment.Description, payment.ClientIpAddress, payment.ClientIdentifier, payment.AdditionalValues,
                                      string.Empty, payment.ReturnUrl, payment.View, payment.AgreementRef, payment.CancelUrl, payment.ClientLanguage, hash);
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
