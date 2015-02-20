using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IVerificationManager
    {
        ConsumerLegalAddressResult GetConsumerLegalAddress(string socialSecurityNumber, string countryCode);
    }
}
