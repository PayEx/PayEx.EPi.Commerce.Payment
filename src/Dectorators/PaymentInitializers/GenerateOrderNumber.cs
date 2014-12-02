using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers
{
    public class GenerateOrderNumber : IPaymentInitializer
    {
        private readonly IPaymentInitializer _initializer;

        public GenerateOrderNumber(IPaymentInitializer initializer)
        {
            _initializer = initializer;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                orderNumber = Generate(currentPayment.OrderGroupId);

            currentPayment.Payment.OrderNumber = orderNumber;

            if (!string.IsNullOrWhiteSpace(currentPayment.Payment.Description))
                currentPayment.Payment.Description = string.Format(currentPayment.Payment.Description, orderNumber);

            return _initializer.Initialize(currentPayment, orderNumber, returnUrl, orderRef);
        }

        private string Generate(int orderGroupId)
        {
            string str = new Random().Next(1000, 9999).ToString();
            return string.Format("{0}{1}", orderGroupId, str);
        }
    }
}
