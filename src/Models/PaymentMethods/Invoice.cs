﻿
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Dectorators.PaymentInitializers;
using Epinova.PayExProvider.Facades;

namespace Epinova.PayExProvider.Models.PaymentMethods
{
    public class Invoice : PaymentMethod
    {
        public Invoice() { }

        public Invoice(Mediachase.Commerce.Orders.Payment payment)
            : base(payment) { }

        public override PaymentInitializeResult Initialize()
        {
            IPaymentInitializer initializer = new GenerateOrderNumber(
                 new InitializePayment(
                 new RedirectUser(), new PaymentManager(), new ParameterReader(), new CartActions(new Logger())));
            return initializer.Initialize(this, null, null);
        }

        public override PaymentCompleteResult Complete(string orderRef)
        {
            throw new System.NotImplementedException();
        }

        public override bool Capture()
        {
            throw new System.NotImplementedException();
        }

        public override bool Credit()
        {
            throw new System.NotImplementedException();
        }
    }
}
