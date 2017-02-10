using System;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IPaymentInitializer
    {
        PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef, Action<string> redirectAction);
    }
}
