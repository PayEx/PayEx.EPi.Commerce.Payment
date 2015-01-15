using EPiServer.Business.Commerce.Payment.PayEx.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Facades;
using EPiServer.Business.Commerce.Payment.PayEx.Factories;
using EPiServer.Business.Commerce.Payment.PayEx.Payment;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Initialization;
using StructureMap;

namespace EPiServer.Business.Commerce.Payment.PayEx.Initializers
{
    [ModuleDependency(typeof(CommerceInitialization))]
    [InitializableModule]
    public class DependencyResolverModule : IConfigurableModule
    {
        private IContainer _container;

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            _container = context.Container;

            _container.Configure(x =>
            {
                x.For<IOrderNumberGenerator>().Use<OrderNumberGenerator>();
                x.For<IAdditionalValuesFormatter>().Use<AdditionalValuesFormatter>();
                x.For<ICartActions>().Use<CartActions>();
                x.For<IPaymentActions>().Use<PaymentActions>();
                x.For<IParameterReader>().Use<ParameterReader>();
                x.For<IPaymentMethodFactory>().Use<PaymentMethodFactory>();
                x.For<ILogger>().Use<Logger>();
                x.For<IHasher>().Use<Hash>();
                x.For<IOrderFacade>().Use<Order>();
                x.For<IPaymentManager>().Use<PaymentManager>();
                x.For<IResultParser>().Use<ResultParser>();
                x.For<IVerificationManager>().Use<VerificationManager>();
                x.For<IVerificationFacade>().Use<Verification>();
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
