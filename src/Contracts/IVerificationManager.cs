using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IVerificationManager
    {
        CustomerDetails GetConsumerLegalAddress(string socialSecurityNumber, string countryCode);
    }
}
