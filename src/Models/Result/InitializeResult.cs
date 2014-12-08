using System;
using System.Xml.Serialization;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.Result
{
    [XmlRoot("payex")]
    public class InitializeResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("orderRef")]
        public Guid OrderRef { get; set; }

        [XmlElement("redirectUrl")]
        public string RedirectUrl { get; set; }
    }
}
