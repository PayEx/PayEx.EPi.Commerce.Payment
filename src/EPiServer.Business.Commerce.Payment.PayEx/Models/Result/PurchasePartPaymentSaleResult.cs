using System.Xml.Serialization;
using EPiServer.Business.Commerce.Payment.PayEx.Payment;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.Result
{
    [XmlRoot("payex")]
    public class PurchasePartPaymentSaleResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("transactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }

        [XmlElement("transactionNumber")]
        public string TransactionNumber { get; set; }

        [XmlElement("paymentMethod")]
        public string PaymentMethod { get; set; }
    }
}
