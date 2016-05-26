using EPiServer.Logging.Compatibility;
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
            Log.InfoFormat("Generating order number for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            if (string.IsNullOrWhiteSpace(orderNumber))
                orderNumber = _orderNumberGenerator.Generate(currentPayment.Cart);

            Log.InfoFormat("Generated order number:{0} for payment with ID:{1} belonging to order with ID: {2}", orderNumber, currentPayment.Payment.Id, currentPayment.OrderGroupId);
            currentPayment.Payment.OrderNumber = orderNumber;

            if (!string.IsNullOrWhiteSpace(currentPayment.Payment.Description))
            {
                if (currentPayment.Payment.Description.Contains("{0}"))
                {
                    Log.InfoFormat("Including the ordernumber in the payment description for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
                    currentPayment.Payment.Description = string.Format(currentPayment.Payment.Description, orderNumber);
                    Log.InfoFormat("Payment description is set to:{0} for payment with ID:{1} belonging to order with ID: {2}", currentPayment.Payment.Description, currentPayment.Payment.Id, currentPayment.OrderGroupId);
                }
            }

            Log.InfoFormat("Finished generating order number for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            return _initializer.Initialize(currentPayment, orderNumber, returnUrl, orderRef);
        }
    }
}
