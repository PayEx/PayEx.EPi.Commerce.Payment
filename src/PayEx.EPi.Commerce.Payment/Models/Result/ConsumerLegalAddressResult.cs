using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    [XmlRoot("payex")]
    public class ConsumerLegalAddressResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("firstName")]
        public string FirstName { get; set; }

        [XmlElement("lastName")]
        public string LastName { get; set; }

        [XmlElement("address1")]
        public string Address { get; set; }

        [XmlElement("postNumber")]
        public string PostNumber { get; set; }

        [XmlElement("city")]
        public string City { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
