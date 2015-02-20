using System.Runtime.Serialization;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus.Configurator;
using System;

namespace EPiServer.Business.Commerce.Payment.PayEx
{
    [Serializable]
    public class ExtendedPayExPayment : PayExPayment
    {
        private static MetaClass _metaClass;

        public static MetaClass ExtendedPayExPaymentMetaClass
        {
            get
            {
                if (_metaClass == null)
                    _metaClass = MetaClass.Load(OrderContext.MetaDataContext, "ExtendedPayExPayment");
                return _metaClass;
            }
        }

        public string SocialSecurityNumber
        {
            get
            {
                return GetString("SocialSecurityNumber");
            }
            set
            {
                this["SocialSecurityNumber"] = value;
            }
        }

        public string FirstName
        {
            get
            {
                return GetString("FirstName");
            }
            set
            {
                this["FirstName"] = value;
            }
        }

        public string LastName
        {
            get
            {
                return GetString("LastName");
            }
            set
            {
                this["LastName"] = value;
            }
        }

        public string StreetAddress
        {
            get
            {
                return GetString("StreetAddress");
            }
            set
            {
                this["StreetAddress"] = value;
            }
        }

        public string CoAddress
        {
            get
            {
                return GetString("CoAddress");
            }
            set
            {
                this["CoAddress"] = value;
            }
        }

        public string City
        {
            get
            {
                return GetString("City");
            }
            set
            {
                this["City"] = value;
            }
        }
        
        public string CountryCode
        {
            get
            {
                return GetString("CountryCode");
            }
            set
            {
                this["CountryCode"] = value;
            }
        }

        public string Email
        {
            get
            {
                return GetString("Email");
            }
            set
            {
                this["Email"] = value;
            }
        }

        public string MobilePhone
        {
            get
            {
                return GetString("MobilePhone");
            }
            set
            {
                this["MobilePhone"] = value;
            }
        }

        public string PostNumber
        {
            get
            {
                return GetString("PostNumber");
            }
            set
            {
                this["PostNumber"] = value;
            }
        }

        public ExtendedPayExPayment()
            : base(ExtendedPayExPaymentMetaClass)
        {
            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName;
        }

        public ExtendedPayExPayment(string clientIpAddress, string productNumber, string returnUrl, string description, string socialSecurityNumber, string countryCode, string email, string mobilePhone)
            : base(ExtendedPayExPaymentMetaClass)
        {
            ValidateParameter(clientIpAddress, "clientIpAddress");
            ValidateParameter(productNumber, "productNumber");
            ValidateParameter(returnUrl, "returnUrl");
            ValidateParameter(description, "description");
            ValidateParameter(socialSecurityNumber, "socialSecurityNumber");
            ValidateParameter(countryCode, "countryCode");
            ValidateParameter(email, "email");
            ValidateParameter(mobilePhone, "mobilePhone");

            ClientIpAddress = clientIpAddress;
            Description = description;
            ProductNumber = productNumber;
            CancelUrl = string.Empty;
            ReturnUrl = returnUrl;
            SocialSecurityNumber = socialSecurityNumber;
            CountryCode = countryCode;
            Email = email;
            MobilePhone = mobilePhone;

            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName;
        }

        public ExtendedPayExPayment(string clientIpAddress, string productNumber, string returnUrl, string description, string socialSecurityNumber, string firstname, string lastname,
            string streetAddress, string coAddress, string city, string postNumber, string countryCode, string email, string mobilePhone)
            : base(ExtendedPayExPaymentMetaClass)
        {
            ValidateParameter(clientIpAddress, "clientIpAddress");
            ValidateParameter(productNumber, "productNumber");
            ValidateParameter(returnUrl, "returnUrl");
            ValidateParameter(description, "description");
            ValidateParameter(socialSecurityNumber, "socialSecurityNumber");
            ValidateParameter(firstname, "firstname");
            ValidateParameter(lastname, "lastname");
            ValidateParameter(streetAddress, "streetAddress");
            ValidateParameter(city, "city");
            ValidateParameter(postNumber, "postNumber");
            ValidateParameter(countryCode, "countryCode");
            ValidateParameter(email, "email");
            ValidateParameter(mobilePhone, "mobilePhone");

            ClientIpAddress = clientIpAddress;
            Description = description;
            ProductNumber = productNumber;
            CancelUrl = string.Empty;
            ReturnUrl = returnUrl;
            SocialSecurityNumber = socialSecurityNumber;
            FirstName = firstname;
            LastName = lastname;
            StreetAddress = streetAddress;
            CoAddress = coAddress ?? string.Empty;
            City = city;
            PostNumber = postNumber;
            CountryCode = countryCode;
            Email = email;
            MobilePhone = mobilePhone;

            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName;
        }

        protected ExtendedPayExPayment(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName;
        }

        private void ValidateParameter(string parameter, string parameterName)
        {
            if (parameter == null || string.IsNullOrWhiteSpace(parameter))
                throw new ArgumentException(parameterName + "cannot be null or empty", parameterName);
        }
    }
}
