using Epinova.PayExProvider.Contracts;
using System.IO;
using System.Xml.Serialization;

namespace Epinova.PayExProvider.Payment
{
    public class ResultParser : IResultParser
    {
        public T Deserialize<T>(string xml) where T : class
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            TextReader textReader = new StringReader(xml);
            object obj = xmlSerializer.Deserialize(textReader);
            if (obj != null)
                return (T)obj;
            return null;
        }
    }
}
