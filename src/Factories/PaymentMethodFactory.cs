using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models.PaymentMethods;
using Mediachase.Commerce.Orders.Dto;

namespace Epinova.PayExProvider.Factories
{
    public class PaymentMethodFactory : IPaymentMethodFactory
    {
        public PaymentMethod Create(Mediachase.Commerce.Orders.Payment payment)
        {
            if (!(payment is PayExPayment))
                return null;

            PaymentMethodDto paymentMethodDto =
                Mediachase.Commerce.Orders.Managers.PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            string systemKeyword = paymentMethodDto.PaymentMethod.FindByPaymentMethodId(payment.PaymentMethodId).SystemKeyword;

            switch (systemKeyword)
            {
                case "PayEx_DirectBankDebit":
                    return new DirectBankDebit(payment);
                case "PayEx_GiftCard":
                    return new GiftCard(payment);
                case "PayEx_Invoice":
                    return new Invoice(payment);
                case "PayEx_InvoiceLedger":
                    return new InvoiceLedger(payment);
                case "PayEx_PartPayment":
                    return new PartPayment(payment);
                case "PayEx_PayPal":
                    return new PayPal(payment);
                default:
                    return new CreditCard(payment);
            }
        }
    }
}
