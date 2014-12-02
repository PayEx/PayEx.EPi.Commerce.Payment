
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface IVerificationFacade
    {
        string GetConsumerLegalAddress(long accountNumber, string socialSecurityNumber, string countryCode, string hash);
    }
}
