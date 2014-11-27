using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Payment
{
    public class ResultParser : IResultParser
    {
        private const string SuccessCode = "OK";

        public T Deserialize<T>(string xml) where T : class
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            TextReader textReader = new StringReader(xml);
            object obj = xmlSerializer.Deserialize(textReader);
            if (obj != null)
                return (T)obj;
            return null;
        }

        public TransactionResult ParseTransactionXml(string xml)
        {
            string errorCode = GetErrorCode(xml);
            string description = GetDescription(xml);
            TransactionStatus status = GetTransactionStatus(xml);

            bool success = errorCode.Equals(SuccessCode) && description.Equals(SuccessCode) && status != TransactionStatus.Failure;

            TransactionResult result = new TransactionResult(success);

            if (success)
            {
                result.TransactionStatus = status;
                result.TransactionNumber = GetTransactionNumber(xml);
                result.PaymentMethod = GetPaymentMethod(xml);
            }
            else
            {
                result.ErrorCode = errorCode;
                result.Description = description;
                result.TransactionStatus = status;
                //result.TransactionErrorCode = GetTransactionErrorCode(xml);
                result = TryAddInvoiceInformation(result, xml);
            }

            return result;
        }

        public InvoiceData ParseInvoiceData(string xml)
        {
            string errorCode = GetErrorCode(xml);
            string description = GetDescription(xml);

            return null;
        }

        private TransactionResult TryAddInvoiceInformation(TransactionResult result, string xml)
        {
            string customerName = ParseXml(xml, "/payex/customerName");
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                result.CustomerName = customerName;
                result.Address = ParseXml(xml, "/payex/customerStreetAddress");
                result.PostNumber = ParseXml(xml, "/payex/customerPostNumber");
                result.City = ParseXml(xml, "/payex/customerCity");
                result.Country = ParseXml(xml, "/payex/customerCountry");
            }
            return result;
        }

        private string GetPaymentMethod(string xml)
        {
            return ParseXml(xml, "/payex/paymentMethod");
        }

        private string GetTransactionNumber(string xml)
        {
            return ParseXml(xml, "/payex/transactionNumber");
        }

        //private TransactionErrorCode GetTransactionErrorCode(string xml)
        //{
        //    string transactionErrorCode = ParseXml(xml, "/payex/errorDetails/transactionErrorCode");
        //    if (transactionErrorCode.Equals("CardNotAcceptedForThisPurchase"))
        //        return TransactionErrorCode.CardNotAcceptedForThisPurchase;
        //    return TransactionErrorCode.Other;
        //}

        private string GetRedirectUrl(string xml)
        {
            return ParseXml(xml, "/payex/redirectUrl");
        }

        private string GetOrderRef(string xml)
        {
            return ParseXml(xml, "/payex/orderRef");
        }

        private string GetErrorCode(string xml)
        {
            return ParseXml(xml, "/payex/status/errorCode");
        }

        private string GetDescription(string xml)
        {
            return ParseXml(xml, "/payex/status/description");
        }

        private TransactionStatus GetTransactionStatus(string xml)
        {
            string transactionStatus = ParseXml(xml, "/payex/transactionStatus");
            if (transactionStatus.Equals("1"))
                return TransactionStatus.Initialize;
            if (transactionStatus.Equals("2"))
                return TransactionStatus.Credit;
            if (transactionStatus.Equals("3"))
                return TransactionStatus.Authorize;
            if (transactionStatus.Equals("6"))
                return TransactionStatus.Capture;
            if (transactionStatus.Equals("5"))
                return TransactionStatus.Failure;
           // return TransactionStatus.Other;
            return TransactionStatus.Initialize;
        }

        private string ParseXml(string xmlText, string node)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlText);
            var myNode = doc.SelectSingleNode(node);
            if (myNode != null)
                return myNode.InnerText;
            return string.Empty;
        }
    }
}
