﻿using System;
using System.IO;
using System.Xml.Serialization;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using log4net;

namespace EPiServer.Business.Commerce.Payment.PayEx.Payment
{
    internal class ResultParser : IResultParser
    {
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public T Deserialize<T>(string xml) where T : class
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            TextReader textReader = new StringReader(xml);
            object obj = xmlSerializer.Deserialize(textReader);
            if (obj != null)
                return (T)obj;

            Log.Fatal(string.Format("Could not deserialize XML result from PayEx to object of type:{0}! xml:{1}", typeof(T).Name, xml));
            throw new Exception(string.Format("Could not deserialize XML result from PayEx to object of type:{0}! xml:{1}", typeof(T).Name, xml));
        }
    }
}
