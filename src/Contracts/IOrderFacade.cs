
namespace Epinova.PayExProvider.Contracts
{
    public interface IOrderFacade
    {
        string Initialize(long accountNumber, string purchaseOperation, long price, string priceArgList, string currency,
            int vat, string orderId, string productNumber, string description,
            string clientIpAddress, string clientIdentifier, string additionalValues, string externalId,
            string returnUrl, string view, string agreementRef, string cancelUrl, string clientLanguage, string hash);

        string Complete(long accountNumber, string orderRef, string hash);

        string Capture(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount,
            string additionalValues, string hash);
    }
}
