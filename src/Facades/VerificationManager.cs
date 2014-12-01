using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Facades
{
    public class VerificationManager : IVerificationManager
    {
        private readonly IVerificationFacade _verificationFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;

        public VerificationManager(IVerificationFacade verificationFacade, IHasher hasher, IResultParser resultParser)
        {
            _verificationFacade = verificationFacade;
            _hasher = hasher;
            _resultParser = resultParser;
        }

        public InvoiceData GetConsumerLegalAddress(string socialSecurityNumber, string countryCode)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, socialSecurityNumber, countryCode,
                PayExSettings.Instance.EncryptionKey);
            string xmlResult = _verificationFacade.GetConsumerLegalAddress(PayExSettings.Instance.AccountNumber, socialSecurityNumber, countryCode, hash);
            return null;
        }
    }
}
