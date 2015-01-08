
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IVerificationFacade
    {
        string GetConsumerLegalAddress(long accountNumber, string socialSecurityNumber, string countryCode, string hash);
    }
}
