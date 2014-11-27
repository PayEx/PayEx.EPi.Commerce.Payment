using System.Xml.Serialization;
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Models.Result
{
    [XmlRoot("payex")]
    public class CreditResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("transactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }

        [XmlElement("transactionNumber")]
        public string TransactionNumber { get; set; }

        public bool Success
        {
            get { return Status.Success && TransactionStatus == TransactionStatus.Credit; }
        }
    }
}
