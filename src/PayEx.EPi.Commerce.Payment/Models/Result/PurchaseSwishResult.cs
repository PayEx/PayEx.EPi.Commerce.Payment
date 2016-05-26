using System.Xml.Serialization;
using PayEx.EPi.Commerce.Payment.Payment;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    class PurchaseSwishResult
    {
        [XmlElement("errorCode")]
        public string ErrorCode { get; set; }

        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("transactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }

        [XmlElement("transactionNumber")]
        public string TransactionNumber { get; set; }

        [XmlElement("paymentMethod")]
        public string PaymentMethod { get; set; }

        [XmlElement("productNumber")]
        public string ProductNumber { get; set; }
        
        [XmlElement("launchUrl")]
        public string LaunchUrl { get; set; }

    }
}
