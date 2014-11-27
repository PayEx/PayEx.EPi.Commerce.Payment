
using System;
using System.Xml.Serialization;

namespace Epinova.PayExProvider.Payment
{
    [Serializable]
    public enum TransactionStatus
    {
        [XmlEnum("0")]
        Sale = 0,

        [XmlEnum("1")]
        Initialize = 1,

        [XmlEnum("2")]
        Credit = 2,

        [XmlEnum("3")]
        Authorize = 3,

        [XmlEnum("4")]
        Cancel = 4,

        [XmlEnum("5")]
        Failure = 5,

        [XmlEnum("6")]
        Capture = 6
    }
}
