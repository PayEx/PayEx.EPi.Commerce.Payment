﻿using System;
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Facades;
using Epinova.PayExProvider.Factories;
using Epinova.PayExProvider.Payment;
using StructureMap.Configuration.DSL;

namespace Epinova.PayExProvider.IoC
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
