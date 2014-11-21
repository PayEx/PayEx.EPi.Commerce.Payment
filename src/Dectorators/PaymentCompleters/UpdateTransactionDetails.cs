using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Dectorators.PaymentCompleters
{
    public class UpdateTransactionDetails : IPaymentCompleter
    {
        public PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef)
        {
            return new PaymentCompleteResult {Success = true};
        }
    }
}
