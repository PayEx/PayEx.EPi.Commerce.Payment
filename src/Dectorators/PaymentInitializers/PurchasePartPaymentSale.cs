using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;

namespace Epinova.PayExProvider.Dectorators.PaymentInitializers
{
    public class PurchasePartPaymentSale : IPaymentInitializer
    {
        private readonly IPaymentManager _paymentManager;

        public PurchasePartPaymentSale(IPaymentManager paymentManager)
        {
            _paymentManager = paymentManager;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            CustomerDetails customerDetails = new CustomerDetails
            {
                SocialSecurityNumber = "590719-5662",
                FirstName = "Eva Dagmar Kristina",
                LastName = "Tannerdal",
                StreetAddress = "Gunbritt Boden p12",
                City = "Småbyen",
                CoAddress = string.Empty,
                CountryCode = "SE",
                Email = "eva@tannerdal.se",
                IpAddress = "1.0.0.27",
                MobilePhone = "9876543212",
                PostNumber = "29620"
            };
            _paymentManager.PurchasePartPaymentSale(orderRef, customerDetails);
            return new PaymentInitializeResult { Success = true };
        }
    }
}
