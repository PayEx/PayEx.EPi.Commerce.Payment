using Mediachase.Commerce.Orders;
using Newtonsoft.Json;

namespace PayEx.EPi.Commerce.Payment.Models
{
    public class Address
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }

        public string Fullname
        {
            get { return FirstName + " " + LastName; }
        }

        public void Populate(OrderAddress orderAddress)
        {
            FirstName = orderAddress.FirstName;
            LastName = orderAddress.LastName;
            Line1 = orderAddress.Line1;
            Line2 = orderAddress.Line2;
            Line3 = string.Empty;
            PostCode = orderAddress.PostalCode;
            City = orderAddress.City;
            State = orderAddress.State;
            Country = orderAddress.CountryName;
            CountryCode = orderAddress.CountryCode;
            Mobile = orderAddress.DaytimePhoneNumber;
            Email = orderAddress.Email;
            Phone = string.Empty;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
