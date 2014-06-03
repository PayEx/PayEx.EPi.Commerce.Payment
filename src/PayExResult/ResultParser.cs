using System;
using System.Xml;
using Epinova.PayExProvider.Contracts;

namespace Epinova.PayExProvider.PayExResult
{
    public class ResultParser : IResultParser
    {
        private const string SuccessCode = "OK";

        public InitializeResult ParseInitializeXml(string xml)
        {
            string errorCode = GetErrorCode(xml);
            string description = GetDescription(xml);
            string orderRef = GetOrderRef(xml);

            bool success = errorCode.Equals(SuccessCode) && description.Equals(SuccessCode);

            InitializeResult result = new InitializeResult(success);

            if (success)
            {
                result.ReturnUrl = GetRedirectUrl(xml);

                Guid orderReference;
                if (Guid.TryParse(orderRef, out orderReference))
                    result.OrderRef = orderReference;
            }
            else
            {
                result.ErrorCode = errorCode;
                result.Description = description;
            }

            return result;
        }

        public TransactionResult ParseTransactionXml(string xml)
        {
            string errorCode = GetErrorCode(xml);
            string description = GetDescription(xml);

            bool success = errorCode.Equals(SuccessCode) && description.Equals(SuccessCode);

            TransactionResult result = new TransactionResult(success);

            if (success)
            {
                result.TransactionStatus = GetTransactionStatus(xml);
                result.TransactionNumber = GetTransactionNumber(xml);
            }
            else
            {
                result.ErrorCode = errorCode;
                result.Description = description;
            }

            return result;
        }

        private string GetTransactionNumber(string xml)
        {
            return ParseXml(xml, "/payex/transactionNumber");
        }

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
            if (transactionStatus.Equals("3"))
                return TransactionStatus.Authorize;
            if (transactionStatus.Equals("6"))
                return TransactionStatus.Capture;
            return TransactionStatus.Other;
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
