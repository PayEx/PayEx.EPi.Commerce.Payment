using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;
using Epinova.PayExProvider.Models.Result;

namespace Epinova.PayExProvider.Dectorators.PaymentInitializers
{
    public class GetConsumerLegalAddress : IPaymentInitializer
    {
        private readonly IVerificationManager _verificationManager;

        public GetConsumerLegalAddress(IPaymentInitializer paymentInitializer, IVerificationManager verificationManager)
        {
            _verificationManager = verificationManager;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl)
        {
            var a = _verificationManager.GetConsumerLegalAddress("195907195662", "SE");
            return new PaymentInitializeResult { Success = true };
        }
    }
}
