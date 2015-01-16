using Newtonsoft.Json;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models
{
    public class CustomerDetails
    {
        public string SocialSecurityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress { get; set; }
        public string CoAddress { get; set; }
        public string PostNumber { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }

        private string _ipAddress;
        public string IpAddress
        {
            get
            {
                if (_ipAddress == "::1") // PayEx does not accept IPv6
                    return "127.0.0.1";
                return _ipAddress;

            }
            set { _ipAddress = value; }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
