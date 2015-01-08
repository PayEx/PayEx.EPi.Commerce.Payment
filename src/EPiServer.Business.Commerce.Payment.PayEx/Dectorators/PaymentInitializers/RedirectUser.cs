using System.Web;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentInitializers
{
    internal class RedirectUser : IPaymentInitializer
    {
        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            PaymentInitializeResult result = new PaymentInitializeResult();
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                HttpContext.Current.Response.Redirect(returnUrl, true);
                result.Success = true;
            }

            return result;
        }
    }
}
