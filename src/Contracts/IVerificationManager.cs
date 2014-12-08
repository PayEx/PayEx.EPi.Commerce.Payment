using EPiServer.Business.Commerce.Payment.PayEx.Models;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IVerificationManager
    {
        CustomerDetails GetConsumerLegalAddress(string socialSecurityNumber, string countryCode);
    }
}
