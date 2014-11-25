using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Facades
{
    public class VerificationManager
    {
        private readonly IVerificationFacade _verificationFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;

        public VerificationManager()
        {
            _verificationFacade = new Verification();
            _hasher = new Hash();
            _resultParser = new ResultParser();
        }

        public InvoiceData GetConsumerLegalAddress(string socialSecurityNumber, string countryCode)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, socialSecurityNumber, countryCode,
                PayExSettings.Instance.EncryptionKey);
            string result = _verificationFacade.GetConsumerLegalAddress(PayExSettings.Instance.AccountNumber, socialSecurityNumber, countryCode, hash);
            return null;
        }
    }
}
