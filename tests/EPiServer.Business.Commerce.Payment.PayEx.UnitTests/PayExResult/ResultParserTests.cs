using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using EPiServer.Business.Commerce.Payment.PayEx.Payment;
using Moq;
using NUnit.Framework;

namespace EPiServer.Business.Commerce.Payment.PayEx.UnitTests.PayExResult
{
    [TestFixture]
    public class ResultParserTests
    {
        Mock<ILogger> _loggerMock = new Mock<ILogger>();

        [Test]
        public void Deserialize_InitializeResultXml_ReturnsInitializeResultObject()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            InitializeResult result = resultParser.Deserialize<InitializeResult>(Factory.InitializeResult);

            Assert.IsNotNull(result);
        }

        [Test]
        public void Deserialize_InitializeIsSuccessful_ReturnsSuccessTrue()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            InitializeResult result = resultParser.Deserialize<InitializeResult>(Factory.InitializeResult);

            Assert.IsNotNull(result.Status);
        }

        [Test]
        public void Deserialize_InitializeIsSuccessful_ReturnsNonEmptyOrderRef()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            InitializeResult result = resultParser.Deserialize<InitializeResult>(Factory.InitializeResult);

            Assert.IsTrue(result.OrderRef != Guid.Empty);
        }

        [Test]
        public void Deserialize_InitializeIsSuccessful_ReturnsNonEmptyRedirectUrl()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            InitializeResult result = resultParser.Deserialize<InitializeResult>(Factory.InitializeResult);

            Assert.IsNotNullOrEmpty(result.RedirectUrl);
        }

        [Test]
        public void Deserialize_InitializeIsUnsuccessful_ReturnsSuccessFalse()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            CompleteResult result = resultParser.Deserialize<CompleteResult>(Factory.InitializeResultError);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Deserialize_CompleteResultXml_ReturnsCompleteResultObject()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            CompleteResult result = resultParser.Deserialize<CompleteResult>(Factory.CompleteResult);

            Assert.IsNotNull(result);
        }

        [Test]
        public void Deserialize_CompleteIsSuccessful_ReturnsSuccessTrue()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            CompleteResult result = resultParser.Deserialize<CompleteResult>(Factory.CompleteResult);

            Assert.IsNotNull(result.Status);
        }

        [Test]
        public void Deserialize_CompleteIsSuccessful_ReturnsNonEmptyTransactionNumber()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            CompleteResult result = resultParser.Deserialize<CompleteResult>(Factory.CompleteResult);

            Assert.IsNotNullOrEmpty(result.TransactionNumber);
        }

        [Test]
        public void Deserialize_CompleteIsSuccessful_ReturnsNonEmptyPaymentMethod()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            CompleteResult result = resultParser.Deserialize<CompleteResult>(Factory.CompleteResult);

            Assert.IsNotNullOrEmpty(result.PaymentMethod);
        }

        [Test]
        public void Deserialize_CompleteIsUnsuccessful_ReturnsSuccessFalse()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            CompleteResult result = resultParser.Deserialize<CompleteResult>(Factory.CompleteResultError);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Deserialize_CompleteIsUnsuccessful_ReturnsNonEmptyTransactionErrorCode()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            CompleteResult result = resultParser.Deserialize<CompleteResult>(Factory.CompleteResultError);

            Assert.IsNotNullOrEmpty(result.ErrorDetails.TransactionErrorCode);
        }

        [Test]
        public void Deserialize_GetTransactionDetailsResultXml_ReturnsTransactionResultObject()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            TransactionResult result = resultParser.Deserialize<TransactionResult>(Factory.TransactionResult);

            Assert.IsNotNull(result);
        }

        [Test]
        public void Deserialize_GetTransactionDetailsIsSuccessful_ReturnsSuccessTrue()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            TransactionResult result = resultParser.Deserialize<TransactionResult>(Factory.TransactionResult);

            Assert.IsNotNull(result.Status);
        }

        [Test]
        public void Deserialize_GetTransactionDetailsIsSuccessful_ReturnsAddressCollection()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            TransactionResult result = resultParser.Deserialize<TransactionResult>(Factory.TransactionResult);

            Assert.IsNotNull(result.AddressCollection);
        }

        [Test]
        public void Deserialize_GetTransactionDetailsIsSuccessful_ReturnsAnAddressCollection()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            TransactionResult result = resultParser.Deserialize<TransactionResult>(Factory.TransactionResult);

            Assert.IsNotNull(result.AddressCollection.MainAddress);
        }

        [Test]
        public void Deserialize_GetTransactionDetailsIsSuccessful_ReturnsNonEmptyCustomerName()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            TransactionResult result = resultParser.Deserialize<TransactionResult>(Factory.TransactionResult);

            Assert.IsNotNullOrEmpty(result.CustomerName);
        }

        [Test]
        public void Deserialize_GetTransactionDetailsIsSuccessful_ReturnsNonEmptyAddress()
        {
            ResultParser resultParser = new ResultParser(_loggerMock.Object);
            TransactionResult result = resultParser.Deserialize<TransactionResult>(Factory.TransactionResult);

            Assert.IsNotNullOrEmpty(result.Address);
        }
    }
}
