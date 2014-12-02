using System;
using EPiServer.Business.Commerce.Payment.PayEx.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Facades;
using EPiServer.Business.Commerce.Payment.PayEx.Factories;
using EPiServer.Business.Commerce.Payment.PayEx.Payment;
using StructureMap.Configuration.DSL;

namespace EPiServer.Business.Commerce.Payment.PayEx.IoC
{
    public class PayExRegistry : Registry
    {
        public PayExRegistry(Func<IPayExSettings> payExSettingsInstance)
        {
            For<ICartActions>().Use<CartActions>();
            For<IParameterReader>().Use<ParameterReader>();
            For<IPayExSettings>().Use(payExSettingsInstance);
            For<IPaymentMethodFactory>().Use<PaymentMethodFactory>();
            For<ILogger>().Use<Logger>();
            For<IPurchaseOrder>().Use<PurchaseOrder>();
            For<IHasher>().Use<Hash>();
            For<IOrderFacade>().Use<Order>();
            For<IPaymentManager>().Use<PaymentManager>();
            For<IResultParser>().Use<ResultParser>();
            For<IVerificationManager>().Use<VerificationManager>();
            For<IVerificationFacade>().Use<Verification>();
        }
    }
}
