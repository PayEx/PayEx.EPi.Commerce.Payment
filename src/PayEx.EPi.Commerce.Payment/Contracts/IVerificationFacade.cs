
namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IVerificationFacade
    {
        string GetConsumerLegalAddress(long accountNumber, string socialSecurityNumber, string countryCode, string hash);
    }
}
