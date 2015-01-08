using System;
using System.Runtime.Serialization;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus.Configurator;

namespace EPiServer.Business.Commerce.Payment.PayEx
{
    [Serializable]
    public class PayExPayment : Mediachase.Commerce.Orders.Payment, IPayExPayment
    {
        private static MetaClass _metaClass;

        public static MetaClass PayExPaymentMetaClass
        {
            get
            {
                if (_metaClass == null)
                    _metaClass = MetaClass.Load(OrderContext.MetaDataContext, "PayExPayment");
                return _metaClass;
            }
        }

        public string OrderNumber
        {
            get
            {
                return GetString("OrderNumber");
            }
            set
            {
                this["OrderNumber"] = value;
            }
        }

        public string PayExOrderRef
        {
            get
            {
                return GetString("PayExOrderRef");
            }
            set
            {
                this["PayExOrderRef"] = value;
            }
        }

        public string Description
        {
            get
            {
                return GetString("Description");
            }
            set
            {
                this["Description"] = value;
            }
        }

        public string ProductNumber
        {
            get
            {
                return GetString("ProductNumber");
            }
            set
            {
                this["ProductNumber"] = value;
            }
        }

        public string ClientIpAddress
        {
            get
            {
                return GetString("ClientIpAddress");
            }
            set
            {
                this["ClientIpAddress"] = value;
            }
        }

        public string ClientUserAgent
        {
            get
            {
                return GetString("ClientUserAgent");
            }
            set
            {
                this["ClientUserAgent"] = value;
            }
        }

        public string CancelUrl
        {
            get
            {
                return GetString("CancelUrl");
            }
            set
            {
                this["CancelUrl"] = value;
            }
        }

        public string ReturnUrl
        {
            get
            {
                return GetString("ReturnUrl");
            }
            set
            {
                this["ReturnUrl"] = value;
            }
        }

        public string CustomerId
        {
            get
            {
                return GetString("CustomerId");
            }
            set
            {
                this["CustomerId"] = value;
            }
        }

        public string AgreementReference
        {
            get
            {
                return GetString("AgreementReference");
            }
            set
            {
                this["AgreementReference"] = value;
            }
        }

        public PayExPayment()
            : base(PayExPaymentMetaClass)
        {
            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName;
        }

        protected PayExPayment(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName;
        }
    }
}
