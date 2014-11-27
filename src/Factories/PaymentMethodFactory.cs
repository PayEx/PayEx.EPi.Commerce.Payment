using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Models.PaymentMethods;
using Mediachase.Commerce.Orders.Dto;

namespace Epinova.PayExProvider.Factories
{
    public class PaymentMethodFactory : IPaymentMethodFactory
    {
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ILogger _logger;
        private readonly ICartActions _cartActions;

        public PaymentMethodFactory(IPaymentManager paymentManager, IParameterReader parameterReader, ILogger logger, ICartActions cartActions)
        {
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _logger = logger;
            _cartActions = cartActions;
        }

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
                    return new CreditCard(payment, _paymentManager, _parameterReader, _logger, _cartActions);
            }
        }
    }
}
