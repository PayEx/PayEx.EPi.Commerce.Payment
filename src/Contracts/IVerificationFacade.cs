
namespace Epinova.PayExProvider.Contracts
{
    public interface IVerificationFacade
    {
        string GetConsumerLegalAddress(long accountNumber, string socialSecurityNumber, string countryCode, string hash);
    }
}
