using System;
using System.IO;
using System.Web;
using System.Xml.Serialization;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Dectorators.AdditionalValuesFormatters
{
    public class FinancingInvoiceAdditionalValuesFormatter : IAdditionalValuesFormatter
    {
        private readonly IAdditionalValuesFormatter _additionalValuesFormatter;
        private readonly IFinancialInvoicingOrderLineFormatter _financialInvoicingOrderLineFormatter;
        private const string FinancinginvoiceOrderlinesParmeter = "FINANCINGINVOICE_ORDERLINES={0}";

        public FinancingInvoiceAdditionalValuesFormatter(IAdditionalValuesFormatter additionalValuesFormatter, IFinancialInvoicingOrderLineFormatter financialInvoicingOrderLineFormatter)
        {
            _additionalValuesFormatter = additionalValuesFormatter;
            _financialInvoicingOrderLineFormatter = financialInvoicingOrderLineFormatter;
        }

        public string Format(PayExPayment payExPayment)
        {
            var additionalValues = string.Empty;
            
            if (_additionalValuesFormatter != null)
                additionalValues = _additionalValuesFormatter.Format(payExPayment);

            if (!string.IsNullOrWhiteSpace(additionalValues))
                additionalValues += "&";
            else
                additionalValues += "";

            if (additionalValues.IndexOf(FinancinginvoiceOrderlinesParmeter, 
                StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                additionalValues = additionalValues.Replace(FinancinginvoiceOrderlinesParmeter,
                    string.Format(FinancinginvoiceOrderlinesParmeter, GenerateOrderLinesString(payExPayment)));
            }
            else if (_financialInvoicingOrderLineFormatter.IncludeOrderLines)
                additionalValues += string.Format(FinancinginvoiceOrderlinesParmeter,
                    GenerateOrderLinesString(payExPayment));

            return additionalValues;
        }

        private string GenerateOrderLinesString(PayExPayment payExPayment)
        {
            var onlineInvoice = new OnlineInvoice();
            onlineInvoice.OrderLines.AddRange(_financialInvoicingOrderLineFormatter.CreateOrderLines(payExPayment));
            return ConvertOnlineInvoiceToString(onlineInvoice);
        }

        public static string ConvertOnlineInvoiceToString(OnlineInvoice onlineInvoice)
        {
            string result;
            var xmlSerializer = new XmlSerializer(onlineInvoice.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, onlineInvoice);
                result = textWriter.ToString();
            }

            result = HttpUtility.UrlEncode(result);
            return result;
        }
    }
}
