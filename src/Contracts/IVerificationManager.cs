using EPiServer.Business.Commerce.Payment.PayEx.Models;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IVerificationManager
    {
        CustomerDetails GetConsumerLegalAddress(string socialSecurityNumber, string countryCode);
    }
}
