using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;
using System;

namespace Epinova.PayExProvider.Dectorators.PaymentInitializers
{
    public class GenerateOrderNumber : IPaymentInitializer
    {
        private readonly IPaymentInitializer _initializer;

        public GenerateOrderNumber(IPaymentInitializer initializer)
        {
            _initializer = initializer;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                orderNumber = Generate(currentPayment.OrderGroupId);

            currentPayment.Payment.OrderNumber = orderNumber;

            if (!string.IsNullOrWhiteSpace(currentPayment.Payment.Description))
                currentPayment.Payment.Description = string.Format(currentPayment.Payment.Description, orderNumber);

            return _initializer.Initialize(currentPayment, orderNumber, returnUrl);
        }

        private string Generate(int orderGroupId)
        {
            string str = new Random().Next(1000, 9999).ToString();
            return string.Format("{0}{1}", orderGroupId, str);
        }
    }
}
