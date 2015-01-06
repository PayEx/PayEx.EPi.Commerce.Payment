using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Business.Commerce.Payment.PayEx.Factories
{
    internal class PaymentMethodFactory : IPaymentMethodFactory
    {
        private readonly IPaymentManager _paymentManager;
        private readonly IParameterReader _parameterReader;
        private readonly ILogger _logger;
        private readonly ICartActions _cartActions;
        private readonly IVerificationManager _verificationManager;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;

        public PaymentMethodFactory(IPaymentManager paymentManager, IParameterReader parameterReader, ILogger logger, ICartActions cartActions, IVerificationManager verificationManager, 
            IOrderNumberGenerator orderNumberGenerator, IAdditionalValuesFormatter additionalValuesFormatter)
        {
            _paymentManager = paymentManager;
            _parameterReader = parameterReader;
            _logger = logger;
            _cartActions = cartActions;
            _verificationManager = verificationManager;
            _orderNumberGenerator = orderNumberGenerator;
            _additionalValuesFormatter = additionalValuesFormatter;
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
                    return new DirectBankDebit(payment, _paymentManager, _parameterReader, _logger, _cartActions, _orderNumberGenerator, _additionalValuesFormatter);
                case "PayEx_GiftCard":
                    return new GiftCard(payment, _paymentManager, _parameterReader, _logger, _cartActions, _orderNumberGenerator, _additionalValuesFormatter);
                case "PayEx_Invoice":
                    return new Invoice(payment, _verificationManager, _paymentManager, _parameterReader, _cartActions, _orderNumberGenerator, _additionalValuesFormatter);
                case "PayEx_InvoiceLedger":
                    return new InvoiceLedger(payment, _paymentManager, _parameterReader, _logger, _cartActions, _orderNumberGenerator, _additionalValuesFormatter);
                case "PayEx_PartPayment":
                    return new PartPayment(payment, _paymentManager, _parameterReader, _logger, _cartActions, _orderNumberGenerator, _additionalValuesFormatter);
                case "PayEx_PayPal":
                    return new PayPal(payment, _paymentManager, _parameterReader, _logger, _cartActions, _orderNumberGenerator, _additionalValuesFormatter);
                default:
                    return new CreditCard(payment, _paymentManager, _parameterReader, _logger, _cartActions, _verificationManager, _orderNumberGenerator, _additionalValuesFormatter);
            }
        }
    }
}
