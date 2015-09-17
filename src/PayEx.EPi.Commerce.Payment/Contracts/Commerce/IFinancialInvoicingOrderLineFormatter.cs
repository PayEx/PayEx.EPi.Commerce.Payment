
using System.Collections.Generic;
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Contracts.Commerce
{
    public interface IFinancialInvoicingOrderLineFormatter
    {
        bool IncludeOrderLines { get; set; }
        string RestProductName { get; }
        int RestVatRate { get; }

        List<OnlineInvoiceOrderLine> CreateOrderLines(PayExPayment payExPayment);
    }
}
