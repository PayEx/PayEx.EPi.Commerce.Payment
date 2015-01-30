using System.Web;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using log4net;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers
{
    internal class RedirectUser : IPaymentInitializer
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
            }

            Log.ErrorFormat("Could not redirect user to PayEx for payment with ID:{0} belonging to order with ID: {1}. ReturnUrl was empty!", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            return result;
        }
    }
}
