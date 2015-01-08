
using System.Xml.Serialization;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.Result
{
    public class Status
    {
        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("errorCode")]
        public string ErrorCode { get; set; }

        public bool Success
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(ErrorCode))
                    return false;

                string successStatusCode = StatusCode.OK.ToString();
                return Description.Equals(successStatusCode) && ErrorCode.Equals(successStatusCode);
            }
        }
    }
}
