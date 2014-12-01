using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IVerificationManager
    {
        InvoiceData GetConsumerLegalAddress(string socialSecurityNumber, string countryCode);
    }
}
