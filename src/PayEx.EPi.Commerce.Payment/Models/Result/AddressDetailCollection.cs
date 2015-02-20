using System.Collections.Generic;
using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment.Models.Result
{
    public class AddressDetailCollection
    {
        [XmlElement("addressDetail")]
        public List<AddressDetail> AddressDetails { get; set; }

        public AddressDetail MainAddress
        {
            get
            {
                if (AddressDetails != null && AddressDetails.Count > 0)
                    return AddressDetails[0];
                return null;
            }
        }
    }
}
