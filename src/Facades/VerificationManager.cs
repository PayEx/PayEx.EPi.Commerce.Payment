using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;

namespace EPiServer.Business.Commerce.Payment.PayEx.Facades
{
    internal class VerificationManager : IVerificationManager
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

        public CustomerDetails GetConsumerLegalAddress(string socialSecurityNumber, string countryCode)
        {
            string hash = _hasher.Create(PayExSettings.Instance.AccountNumber, socialSecurityNumber, countryCode,
                PayExSettings.Instance.EncryptionKey);
            string xmlResult = _verificationFacade.GetConsumerLegalAddress(PayExSettings.Instance.AccountNumber, socialSecurityNumber, countryCode, hash);
            return null;
        }
    }
}
