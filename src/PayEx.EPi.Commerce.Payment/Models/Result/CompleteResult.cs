using System.Xml.Serialization;
using PayEx.EPi.Commerce.Payment.Payment;

namespace PayEx.EPi.Commerce.Payment.Models.Result
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

        public bool GetTransactionDetails => TransactionStatus == TransactionStatus.Initialize;

        public bool Success => Status.Success && TransactionStatus != TransactionStatus.Failure;
    }
}
