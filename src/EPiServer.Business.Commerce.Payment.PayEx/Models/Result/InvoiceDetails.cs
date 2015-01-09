using System.Xml.Serialization;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.Result
{
    public class InvoiceDetails
    {
        [XmlElement("customerName")]
        public string CustomerName { get; set; }

        [XmlElement("customerStreetAddress")]
        public string CustomerStreetAddress { get; set; }

        [XmlElement("customerPostNumber")]
        public string CustomerPostNumber { get; set; }

        [XmlElement("customerCity")]
        public string CustomerCity { get; set; }

        [XmlElement("customerCountry")]
        public string CustomerCountry { get; set; }

        [XmlElement("customerEmailAddress")]
        public string CustomerEmail { get; set; }
    }
}
