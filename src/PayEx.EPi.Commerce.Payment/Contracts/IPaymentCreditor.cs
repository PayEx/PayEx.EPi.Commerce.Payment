
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IPaymentCreditor
    {
        bool Credit(PaymentMethod currentPayment);
    }
}
