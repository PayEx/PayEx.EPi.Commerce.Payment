using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    [XmlRoot("payex")]
    public class InvoiceLinkResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }

        public bool Success => Status.Success;
    }
}
