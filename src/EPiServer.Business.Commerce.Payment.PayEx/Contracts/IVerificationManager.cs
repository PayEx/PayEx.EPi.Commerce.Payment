using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IVerificationManager
    {
        ConsumerLegalAddressResult GetConsumerLegalAddress(string socialSecurityNumber, string countryCode);
    }
}
