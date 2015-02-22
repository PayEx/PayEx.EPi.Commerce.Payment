using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PayEx.EPi.Commerce.Payment.Models.Result
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

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
