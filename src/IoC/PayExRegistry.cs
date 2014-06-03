using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Facades;
using Epinova.PayExProvider.PayExResult;
using Epinova.PayExProvider.Util;
using StructureMap.Configuration.DSL;

namespace Epinova.PayExProvider.IoC
{
    public class PayExRegistry : Registry
    {
        public PayExRegistry()
        {
            For<IHasher>().Use<HashUtil>();
            For<IOrderFacade>().Use<Order>();
            For<IPaymentManager>().Use<PaymentManager>();
            For<IResultParser>().Use<ResultParser>();
            For<ISettings>().Use<Settings>();
        }
    }
}
