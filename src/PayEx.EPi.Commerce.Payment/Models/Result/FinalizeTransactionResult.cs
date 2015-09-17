using PayEx.EPi.Commerce.Payment.Payment;
using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    [XmlRoot("payex")]
    public class FinalizeTransactionResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("transactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }

        [XmlElement("transactionNumber")]
        public string TransactionNumber { get; set; }

        [XmlElement("orderId")]
        public string OrderId { get; set; }

        [XmlElement("orderRef")]
        public string OrderRef { get; set; }

        [XmlElement("productId")]
        public string ProductId { get; set; }

        [XmlElement("paymentMethod")]
        public string PaymentMethod { get; set; }

        [XmlElement("productNumber")]
        public string ProductNumber { get; set; }

        public bool Success
        {
            get { return Status.Success; }
        }
    }
}
