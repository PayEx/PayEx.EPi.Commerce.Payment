using PayEx.EPi.Commerce.Payment.Contracts;

namespace PayEx.EPi.Commerce.Payment.Facades
{
    internal class Verification : IVerificationFacade
    {
        private PxVerification.PxVerificationSoapClient _client;

        private PxVerification.PxVerificationSoapClient Client => _client ?? (_client = new PxVerification.PxVerificationSoapClient());

        public string GetConsumerLegalAddress(long accountNumber, string socialSecurityNumber, string countryCode, string hash)
        {
            return Client.GetConsumerLegalAddress(accountNumber, socialSecurityNumber, countryCode, hash);
        }
    }
}
