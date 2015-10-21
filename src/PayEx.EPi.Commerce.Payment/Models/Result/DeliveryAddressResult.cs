using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    [XmlRoot("payex")]
    public class DeliveryAddressResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("firstName")]
        public string FirstName { get; set; }

        [XmlElement("lastName")]
        public string LastName { get; set; }

        [XmlElement("address1")]
        public string Address { get; set; }

        [XmlElement("address2")]
        public string Address2 { get; set; }

        [XmlElement("address3")]
        public string Address3 { get; set; }

        [XmlElement("postalCode")]
        public string PostalCode { get; set; }

        [XmlElement("city")]
        public string City { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }

        [XmlElement("phone")]
        public string Phone { get; set; }

        [XmlElement("eMail")]
        public string Email { get; set; }

        public string CustomerName => string.Join(" ", FirstName, LastName);

        public bool Success => Status.Success;
    }
}
