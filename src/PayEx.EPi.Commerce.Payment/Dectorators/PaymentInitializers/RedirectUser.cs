using System.Web;
using log4net;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers
{
    internal class RedirectUser : IPaymentInitializer
    {
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            PaymentInitializeResult result = new PaymentInitializeResult();
            Log.Info($"Begin redirect to PayEx for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}.");
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                Log.Info($"Redirecting user PayEx for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}. ReturnUrl: {returnUrl}");
                HttpContext.Current.Response.Redirect(returnUrl, true);
                result.Success = true;
                return result;
            }

            Log.Error($"Could not redirect user to PayEx for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}. ReturnUrl was empty!");
            return result;
        }
    }
}
