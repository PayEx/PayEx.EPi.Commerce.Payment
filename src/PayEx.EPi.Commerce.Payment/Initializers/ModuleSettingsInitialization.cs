using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging.Compatibility;

namespace PayEx.EPi.Commerce.Payment.Initializers
{
    [InitializableModule]
    [ModuleDependency(typeof(DependencyResolverModule))]
    internal class ModuleSettingsInitialization : IInitializableModule
    {
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public void Initialize(InitializationEngine context)
        {
            if (string.IsNullOrWhiteSpace(PayExSettings.Instance.AccountNumber.ToString()))
                Log.Error("The PayEx payment provider will not work unless you supply an Account Number in the payment provider module settings!");

            if (string.IsNullOrWhiteSpace(PayExSettings.Instance.EncryptionKey))
                Log.Error("The PayEx payment provider will not work unless you supply an encryption key in the payment provider module settings!");
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
