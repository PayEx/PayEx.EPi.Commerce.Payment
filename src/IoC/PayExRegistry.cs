using System.Web;
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Facades;
using Epinova.PayExProvider.Payment;
using StructureMap.Configuration.DSL;

namespace Epinova.PayExProvider.IoC
{
    public class PayExRegistry : Registry
    {
        public PayExRegistry()
        {
            For<ILogger>().Use<Logger>();
            For<IOrderNote>().Use<OrderNote>();
            For<IPurchaseOrder>().Use<PurchaseOrder>();
            For<IHasher>().Use<Hash>();
            For<IOrderFacade>().Use<Order>();
            For<IPaymentManager>().Use<PaymentManager>();
            For<IResultParser>().Use<ResultParser>();
            For<ISettings>().Use<Settings>();
            For<HttpContextBase>().Use(() => new HttpContextWrapper(HttpContext.Current));
        }
    }
}
