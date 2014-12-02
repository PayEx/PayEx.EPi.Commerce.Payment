using System.Xml.Serialization;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.Result
{
    public class ErrorDetails
    {
        [XmlElement("transactionErrorCode")]
        public string TransactionErrorCode { get; set; }
    }
}
