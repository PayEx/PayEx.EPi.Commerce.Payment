
using System.Xml.Serialization;

namespace Epinova.PayExProvider.Models.Result
{
    [XmlRoot("payex")]
    public class TransactionResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PostNumber { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
