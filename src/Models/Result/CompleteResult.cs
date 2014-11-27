using System.Xml.Serialization;
using Epinova.PayExProvider.Payment;

namespace Epinova.PayExProvider.Models.Result
{
    [XmlRoot("payex")]
    public class CompleteResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("transactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }

        [XmlElement("transactionNumber")]
        public string TransactionNumber { get; set; }

        [XmlElement("paymentMethod")]
        public string PaymentMethod { get; set; }

        [XmlElement("errorDetails")]
        public ErrorDetails ErrorDetails { get; set; }

        public bool GetTransactionDetails
        {
            get { return TransactionStatus == TransactionStatus.Initialize; }
        }

        public bool Success
        {
            get { return Status.Success && TransactionStatus != TransactionStatus.Failure; }
        }
    }
}
