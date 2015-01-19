using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;

namespace EPiServer.Business.Commerce.Payment.PayEx.Facades
{
    internal class VerificationManager : IVerificationManager
    {
        private readonly IVerificationFacade _verificationFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;
        private readonly IPayExSettings _payExSettings;
        private readonly ILogger _logger;

        public VerificationManager(IVerificationFacade verificationFacade, IHasher hasher, IResultParser resultParser, IPayExSettings payExSettings, ILogger logger)
        {
            _verificationFacade = verificationFacade;
            _hasher = hasher;
            _resultParser = resultParser;
            _payExSettings = payExSettings;
            _logger = logger;
        }

        public ConsumerLegalAddressResult GetConsumerLegalAddress(string socialSecurityNumber, string countryCode)
        {
            _logger.LogInfo(string.Format("Calling GetConsumerLegalAddress for SocialSecurityNumber:{0}. CountryCode:{1}.", socialSecurityNumber, countryCode));

            string hash = _hasher.Create(_payExSettings.AccountNumber, socialSecurityNumber, countryCode, _payExSettings.EncryptionKey);
            string xmlResult = _verificationFacade.GetConsumerLegalAddress(_payExSettings.AccountNumber, socialSecurityNumber, countryCode, hash);

            ConsumerLegalAddressResult result = _resultParser.Deserialize<ConsumerLegalAddressResult>(xmlResult);
            if (result.Status.Success)
                _logger.LogInfo(string.Format("Successfully called GetConsumerLegalAddress for SocialSecurityNumber:{0}. CountryCode:{1}. Result:{2}", socialSecurityNumber, countryCode, xmlResult));
            else
                _logger.LogError(string.Format("Error when calling GetConsumerLegalAddress for SocialSecurityNumber:{0}. CountryCode:{1}. Result:{2}", socialSecurityNumber, countryCode, xmlResult));
            return result;
        }
    }
}
