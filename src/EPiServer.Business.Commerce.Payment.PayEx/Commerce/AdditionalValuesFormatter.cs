using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;

namespace EPiServer.Business.Commerce.Payment.PayEx.Commerce
{
    internal class AdditionalValuesFormatter : IAdditionalValuesFormatter
    {
        /// <summary>
        /// Formats an additionalValues query string to be included in the PayEx payment initialization as described in the 
        /// PayEx documentation: http://www.payexpim.com/technical-reference/pxorder/initialize8/
        /// </summary>
        /// <param name="payExPayment">The payment being initialized</param>
        /// <returns>A query string of formatted additional values. Example: INVOICE_INVOICETEXT=value&INVOICE_MEDIADISTRIBUTION=value…</returns>
        public string Format(PayExPayment payExPayment)
        {
            return string.Empty;
        }
    }
}
