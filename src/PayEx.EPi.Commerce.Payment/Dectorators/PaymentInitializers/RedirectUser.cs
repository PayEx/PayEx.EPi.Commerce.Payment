using System.Web;
using EPiServer.Logging.Compatibility;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers
{
    internal class RedirectUser : IRedirectUser
    {
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            PaymentInitializeResult result = new PaymentInitializeResult();
            Log.InfoFormat("Begin redirect to PayEx for payment with ID:{0} belonging to order with ID: {1}.", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                Log.InfoFormat("Redirecting user PayEx for payment with ID:{0} belonging to order with ID: {1}. ReturnUrl: {2}", currentPayment.Payment.Id, currentPayment.OrderGroupId, returnUrl);
                HttpContext.Current.Response.Redirect(returnUrl, true);
                result.Success = true;
                return result;
            }

            Log.ErrorFormat("Could not redirect user to PayEx for payment with ID:{0} belonging to order with ID: {1}. ReturnUrl was empty!", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            return result;
        }
    }
}
