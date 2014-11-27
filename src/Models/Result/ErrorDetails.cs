using System.Xml.Serialization;

namespace Epinova.PayExProvider.Models.Result
{
    public class ErrorDetails
    {
        [XmlElement("transactionErrorCode")]
        public string TransactionErrorCode { get; set; }
    }
}
