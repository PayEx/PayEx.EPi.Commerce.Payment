using EPiServer.Logging.Compatibility;
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
            Log.InfoFormat("Calling GetConsumerLegalAddress for SocialSecurityNumber:{0}. CountryCode:{1}.", socialSecurityNumber, countryCode);

            string hash = _hasher.Create(_payExSettings.AccountNumber, socialSecurityNumber, countryCode, _payExSettings.EncryptionKey);
            string xmlResult = _verificationFacade.GetConsumerLegalAddress(_payExSettings.AccountNumber, socialSecurityNumber, countryCode, hash);

            ConsumerLegalAddressResult result = _resultParser.Deserialize<ConsumerLegalAddressResult>(xmlResult);
            if (result.Status.Success)
                Log.InfoFormat("Successfully called GetConsumerLegalAddress for SocialSecurityNumber:{0}. CountryCode:{1}. Result:{2}", socialSecurityNumber, countryCode, xmlResult);
            else
                Log.ErrorFormat("Error when calling GetConsumerLegalAddress for SocialSecurityNumber:{0}. CountryCode:{1}. Result:{2}", socialSecurityNumber, countryCode, xmlResult);
            return result;
        }
    }
}
