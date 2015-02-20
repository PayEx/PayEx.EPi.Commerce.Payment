using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    public class ErrorDetails
    {
        [XmlElement("transactionErrorCode")]
        public string TransactionErrorCode { get; set; }
    }
}
