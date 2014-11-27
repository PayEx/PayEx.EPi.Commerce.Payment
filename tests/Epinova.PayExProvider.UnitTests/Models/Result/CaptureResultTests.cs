using Epinova.PayExProvider.Models.Result;
using Epinova.PayExProvider.Payment;
using NUnit.Framework;

namespace Epinova.PayExProvider.UnitTests.Models.Result
{
    [TestFixture]
    public class CaptureResultTests
    {
        [TestCase(TransactionStatus.Initialize, true, false)]
        [TestCase(TransactionStatus.Authorize, true, false)]
        [TestCase(TransactionStatus.Capture, true, true)]
        [TestCase(TransactionStatus.Initialize, false, false)]
        [TestCase(TransactionStatus.Authorize, false, false)]
        [TestCase(TransactionStatus.Capture, false, false)]
        public void Success_ReturnsCorrectResult(TransactionStatus transactionStatus, bool statusSuccess, bool expected)
        {
            Status status = Factory.CreateStatus(statusSuccess);

            CaptureResult captureResult = new CaptureResult { Status = status, TransactionStatus = transactionStatus };
            Assert.AreEqual(expected, captureResult.Success);
        }
    }
}
