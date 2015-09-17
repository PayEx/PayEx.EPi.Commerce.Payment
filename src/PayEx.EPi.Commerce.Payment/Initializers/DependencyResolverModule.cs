using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Initialization;
using PayEx.EPi.Commerce.Payment.Commerce;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Facades;
using PayEx.EPi.Commerce.Payment.Factories;
using PayEx.EPi.Commerce.Payment.Payment;
using StructureMap;

namespace PayEx.EPi.Commerce.Payment.Initializers
{
    [ModuleDependency(typeof(CommerceInitialization))]
    [InitializableModule]
    internal class DependencyResolverModule : IConfigurableModule
    {
        private IContainer _container;

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            _container = context.Container;

            _container.Configure(x =>
            {
                x.For<IPayExSettings>().Use(() => PayExSettings.Instance);

                x.For<IOrderNumberGenerator>().Use<OrderNumberGenerator>();
                x.For<IAdditionalValuesFormatter>().Use<AdditionalValuesFormatter>();
                x.For<ICartActions>().Use<CartActions>();
                x.For<IPaymentActions>().Use<PaymentActions>();
                x.For<IParameterReader>().Use<ParameterReader>();
                x.For<IPaymentMethodFactory>().Use<PaymentMethodFactory>();
                x.For<IHasher>().Use<Hash>();
                x.For<IOrderFacade>().Use<Order>();
                x.For<IPaymentManager>().Use<PaymentManager>();
                x.For<IResultParser>().Use<ResultParser>();
                x.For<IVerificationManager>().Use<VerificationManager>();
                x.For<IVerificationFacade>().Use<Verification>();
                x.For<IPayExService>().Use<PayExService>();
            });
        }

        public void Initialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }


        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
