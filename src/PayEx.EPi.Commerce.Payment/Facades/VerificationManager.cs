using log4net;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Facades
{
    internal class VerificationManager : IVerificationManager
    {
        private readonly IVerificationFacade _verificationFacade;
        private readonly IHasher _hasher;
        private readonly IResultParser _resultParser;
        private readonly IPayExSettings _payExSettings;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public VerificationManager(IVerificationFacade verificationFacade, IHasher hasher, IResultParser resultParser, IPayExSettings payExSettings)
        {
            _verificationFacade = verificationFacade;
            _hasher = hasher;
            _resultParser = resultParser;
            _payExSettings = payExSettings;
        }

        public ConsumerLegalAddressResult GetConsumerLegalAddress(string socialSecurityNumber, string countryCode)
        {
            Log.Info($"Calling GetConsumerLegalAddress for SocialSecurityNumber:{socialSecurityNumber}. CountryCode:{countryCode}.");

            var hash = _hasher.Create(_payExSettings.AccountNumber, socialSecurityNumber, countryCode, _payExSettings.EncryptionKey);
            var xmlResult = _verificationFacade.GetConsumerLegalAddress(_payExSettings.AccountNumber, socialSecurityNumber, countryCode, hash);

            var result = _resultParser.Deserialize<ConsumerLegalAddressResult>(xmlResult);
            if (result.Status.Success)
                Log.Info($"Successfully called GetConsumerLegalAddress for SocialSecurityNumber:{socialSecurityNumber}. CountryCode:{countryCode}. Result:{xmlResult}");
            else
                Log.Error($"Error when calling GetConsumerLegalAddress for SocialSecurityNumber:{socialSecurityNumber}. CountryCode:{countryCode}. Result:{xmlResult}");
            return result;
        }
    }
}
