using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Models.PaymentMethods;
using System;

namespace Epinova.PayExProvider.Dectorators.PaymentInitializers
{
    public class PurchaseInvoiceSale : IPaymentInitializer
    {
        private readonly IPaymentManager _paymentManager;

        public PurchaseInvoiceSale(IPaymentManager paymentManager)
        {
            _paymentManager = paymentManager;
        }

        public PaymentInitializeResult Initialize(PaymentMethod currentPayment, string orderNumber, string returnUrl, string orderRef)
        {
            InvoiceData invoiceData = new InvoiceData(true); //TODO
            _paymentManager.PurchaseInvoiceSale(orderRef, invoiceData); // Check result
            return new PaymentInitializeResult {Success = true};
        }
    }
}
