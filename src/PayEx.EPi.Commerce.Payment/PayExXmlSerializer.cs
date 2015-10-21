using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment
{
    public static class PayExXmlSerializer
    {
        public static string Serialize<T>(T arg)
        {
            string result;
            var settings = new XmlWriterSettings
            {
                Indent = false,
                NewLineHandling = NewLineHandling.None
            };
            var xmlSerializer = new XmlSerializer(arg.GetType());

            using (var textWriter = new StringWriter())
            {
                using (var writer = XmlWriter.Create(textWriter, settings))
                {
                    xmlSerializer.Serialize(writer, arg);
                    result = textWriter.ToString();
                }
            }

            result = Uri.EscapeDataString(RemoveLineEndings(result));
            return result;
        }

        private static string RemoveLineEndings(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            var lineSeparator = ((char)0x2028).ToString();
            var paragraphSeparator = ((char)0x2029).ToString();

            return
                value.Replace("\r\n", string.Empty)
                    .Replace("\n", string.Empty)
                    .Replace("\r", string.Empty)
                    .Replace(lineSeparator, string.Empty)
                    .Replace(paragraphSeparator, string.Empty);
        }
    }
}
