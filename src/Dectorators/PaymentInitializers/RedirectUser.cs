using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;
using System.Web;

namespace Epinova.PayExProvider.Dectorators.PaymentInitializers
{
    public class RedirectUser : IPaymentInitializer
    {
        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl)
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
