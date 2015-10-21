using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    [XmlRoot("payex")]
    public class TransactionResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("addressDetails")]
        public AddressDetailCollection AddressCollection { get; set; }

        [XmlElement("invoice")]
        public InvoiceDetails Invoice { get; set; }

        public string CustomerName => AddressCollection.MainAddress != null ? AddressCollection.MainAddress.CustomerName : string.Empty;

        public string Address => AddressCollection.MainAddress != null ? AddressCollection.MainAddress.Address : string.Empty;

        public string PostNumber => AddressCollection.MainAddress != null ? AddressCollection.MainAddress.PostNumber : string.Empty;

        public string City => AddressCollection.MainAddress != null ? AddressCollection.MainAddress.City : string.Empty;

        public string Country => AddressCollection.MainAddress != null ? AddressCollection.MainAddress.Country : string.Empty;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
