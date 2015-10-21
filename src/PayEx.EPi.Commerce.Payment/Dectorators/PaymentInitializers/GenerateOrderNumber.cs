using log4net;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers
{
    internal class GenerateOrderNumber : IPaymentInitializer
    {
        private readonly IPaymentInitializer _initializer;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public GenerateOrderNumber(IPaymentInitializer initializer, IOrderNumberGenerator orderNumberGenerator)
        {
            _initializer = initializer;
            _orderNumberGenerator = orderNumberGenerator;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            Log.Info($"Generating order number for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            if (string.IsNullOrWhiteSpace(orderNumber))
                orderNumber = _orderNumberGenerator.Generate(currentPayment.Cart);

            Log.Info($"Generated order number:{orderNumber} for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            currentPayment.Payment.OrderNumber = orderNumber;

            if (!string.IsNullOrWhiteSpace(currentPayment.Payment.Description))
            {
                if (currentPayment.Payment.Description.Contains("{0}"))
                {
                    Log.Info($"Including the ordernumber in the payment description for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
                    currentPayment.Payment.Description = string.Format(currentPayment.Payment.Description, orderNumber);
                    Log.Info($"Payment description is set to:{currentPayment.Payment.Description} for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
                }
            }

            Log.Info($"Finished generating order number for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            return _initializer.Initialize(currentPayment, orderNumber, returnUrl, orderRef);
        }
    }
}
