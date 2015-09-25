using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    [XmlRoot("payex")]
    public class LegalAddressResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("streetAddress")]
        public string StreetAddress { get; set; }

        [XmlElement("zipCode")]
        public string ZipCode { get; set; }

        [XmlElement("city")]
        public string City { get; set; }

        [XmlElement("countryCode")]
        public string CountryCode { get; set; }

        [XmlElement("coAddress")]
        public string CoAddress { get; set; }

        public bool Success
        {
            get { return Status.Success; }
        }
    }
}
