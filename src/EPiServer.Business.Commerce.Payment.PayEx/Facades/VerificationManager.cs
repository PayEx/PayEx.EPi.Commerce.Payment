using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;

namespace EPiServer.Business.Commerce.Payment.PayEx.Facades
{
    internal class VerificationManager : IVerificationManager
    {
        private readonly IVerificationFacade _verificationFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;
        private readonly IPayExSettings _payExSettings;

        public VerificationManager(IVerificationFacade verificationFacade, IHasher hasher, IResultParser resultParser, IPayExSettings payExSettings)
        {
            _verificationFacade = verificationFacade;
            _hasher = hasher;
            _resultParser = resultParser;
            _payExSettings = payExSettings;
        }

        public CustomerDetails GetConsumerLegalAddress(string socialSecurityNumber, string countryCode)
        {
            string hash = _hasher.Create(_payExSettings.AccountNumber, socialSecurityNumber, countryCode,
                PayExSettings.Instance.EncryptionKey);
            string xmlResult = _verificationFacade.GetConsumerLegalAddress(_payExSettings.AccountNumber, socialSecurityNumber, countryCode, hash);
            return null;
        }
    }
}
