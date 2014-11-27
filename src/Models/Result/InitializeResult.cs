﻿using System;
using System.Xml.Serialization;

namespace Epinova.PayExProvider.Models.Result
{
    [XmlRoot("payex")]
    public class InitializeResult
    {
        [XmlElement("status")]
        public Status Status { get; set; }

        [XmlElement("orderRef")]
        public Guid OrderRef { get; set; }

        [XmlElement("redirectUrl")]
        public string RedirectUrl { get; set; }
    }
}
