using System;
using Epinova.PayExProvider.Payment;
using NUnit.Framework;

namespace Epinova.PayExProvider.UnitTests.PayExResult
{
    [TestFixture]
    public class ResultParserTests
    {
        [Test]
        public void ParseInitializeXml_InitializeIsSuccessful_SuccessIsTrue()
        {
            ResultParser resultParser = new ResultParser();
            ResultBase result = resultParser.ParseInitializeXml(XmlInput.InitializeResultSuccess);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ParseInitializeXml_InitializeIsSuccessful_OrderRefExists()
        {
            ResultParser resultParser = new ResultParser();
            InitializeResult result = resultParser.ParseInitializeXml(XmlInput.InitializeResultSuccess);

            Assert.AreNotEqual(result.OrderRef, Guid.Empty);
        }

        [Test]
        public void ParseInitializeXml_InitializeIsSuccessful_ReturnsUrlExists()
        {
            ResultParser resultParser = new ResultParser();
            InitializeResult result = resultParser.ParseInitializeXml(XmlInput.InitializeResultSuccess);

            Assert.IsNotNullOrEmpty(result.ReturnUrl);
        }

        [Test]
        public void ParseInitializeXml_InitializeError_SuccessIsFalse()
        {
            ResultParser resultParser = new ResultParser();
            InitializeResult result = resultParser.ParseInitializeXml(XmlInput.InitializeResultError);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void ParseInitializeXml_InitializeError_NoOrderRef()
        {
            ResultParser resultParser = new ResultParser();
            InitializeResult result = resultParser.ParseInitializeXml(XmlInput.InitializeResultError);

            Assert.AreEqual(result.OrderRef, Guid.Empty);
        }

        [Test]
        public void ParseInitializeXml_InitializeError_ErrorCodeNotEmpty()
        {
            ResultParser resultParser = new ResultParser();
            InitializeResult result = resultParser.ParseInitializeXml(XmlInput.InitializeResultError);

            Assert.IsNotNullOrEmpty(result.ErrorCode);
        }

        [Test]
        public void ParseInitializeXml_InitializeError_DescriptionNotEmpty()
        {
            ResultParser resultParser = new ResultParser();
            InitializeResult result = resultParser.ParseInitializeXml(XmlInput.InitializeResultError);

            Assert.IsNotNullOrEmpty(result.Description);
        }

        [Test]
        public void ParseCompleteXml_CompleteIsSuccessful_SuccessIsTrue()
        {
            ResultParser resultParser = new ResultParser();
            ResultBase result = resultParser.ParseTransactionXml(XmlInput.CompleteResultSuccess);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ParseCompleteXml_CompleteIsSuccessful_TransactionNumberNotEmpty()
        {
            ResultParser resultParser = new ResultParser();
            TransactionResult result = resultParser.ParseTransactionXml(XmlInput.CompleteResultSuccess);

            Assert.IsNotNullOrEmpty(result.TransactionNumber);
        }

        [Test]
        public void ParseCompleteXml_CompleteIsSuccessful_TransactionStatusIsAuthorize()
        {
            ResultParser resultParser = new ResultParser();
            TransactionResult result = resultParser.ParseTransactionXml(XmlInput.CompleteResultSuccess);

            Assert.AreEqual(result.TransactionStatus, TransactionStatus.Authorize);
        }

        [Test]
        public void ParseCompleteXml_CompleteError_ErrorCodeNotEmpty()
        {
            ResultParser resultParser = new ResultParser();
            TransactionResult result = resultParser.ParseTransactionXml(XmlInput.CompleteResultError);

            Assert.IsNotNullOrEmpty(result.ErrorCode);
        }

        [Test]
        public void ParseCompleteXml_CompleteError_DescriptionNotEmpty()
        {
            ResultParser resultParser = new ResultParser();
            TransactionResult result = resultParser.ParseTransactionXml(XmlInput.CompleteResultError);

            Assert.IsNotNullOrEmpty(result.Description);
        }
    }
}
