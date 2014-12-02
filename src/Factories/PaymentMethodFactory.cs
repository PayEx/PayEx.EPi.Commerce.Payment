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
        private readonly IVerificationManager _verificationManager;

        public PaymentMethodFactory(IPaymentManager paymentManager, IParameterReader parameterReader, ILogger logger, ICartActions cartActions, IVerificationManager verificationManager)
        {
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _logger = logger;
            _cartActions = cartActions;
            _verificationManager = verificationManager;
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
                    return new DirectBankDebit(payment, _paymentManager, _parameterReader, _logger, _cartActions);
                case "PayEx_GiftCard":
                    return new GiftCard(payment, _paymentManager, _parameterReader, _logger, _cartActions);
                case "PayEx_Invoice":
                    return new Invoice(payment, _verificationManager, _paymentManager, _parameterReader, _cartActions);
                case "PayEx_InvoiceLedger":
                    return new InvoiceLedger(payment, _paymentManager, _parameterReader, _logger, _cartActions);
                case "PayEx_PartPayment":
                    return new PartPayment(payment);
                case "PayEx_PayPal":
                    return new PayPal(payment);
                default:
                    return new CreditCard(payment, _paymentManager, _parameterReader, _logger, _cartActions, _verificationManager);
            }
        }
    }
}
