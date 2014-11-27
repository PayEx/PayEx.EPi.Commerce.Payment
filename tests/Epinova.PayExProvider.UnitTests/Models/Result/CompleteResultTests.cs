using Epinova.PayExProvider.Models.Result;
using Epinova.PayExProvider.Payment;
using NUnit.Framework;

namespace Epinova.PayExProvider.UnitTests.Models.Result
{
    [TestFixture]
    public class CompleteResultTests
    {
        [TestCase(TransactionStatus.Initialize, true, true)]
        [TestCase(TransactionStatus.Authorize, true, true)]
        [TestCase(TransactionStatus.Initialize, false, false)]
        [TestCase(TransactionStatus.Authorize, false, false)]
        [TestCase(TransactionStatus.Failure, true, false)]
        [TestCase(TransactionStatus.Failure, false, false)]
        public void Success_StatusSuccessIsTrue_ReturnsCorrectResult(TransactionStatus transactionStatus, bool statusSuccess, bool expected)
        {
            Status status;
            if (statusSuccess)
                status = new Status { Description = "OK", ErrorCode = "OK" };
            else
                status = new Status { Description = "NotOK", ErrorCode = "NotOK" };

            CompleteResult completeResult = new CompleteResult { Status = status, TransactionStatus = transactionStatus };
            Assert.AreEqual(expected, completeResult.Success);
        }

        [TestCase(TransactionStatus.Initialize, true)]
        [TestCase(TransactionStatus.Authorize, false)]
        public void GetTransactionDetails_ReturnsCorrectResult(TransactionStatus transactionStatus, bool expected)
        {
            CompleteResult completeResult = new CompleteResult { TransactionStatus = transactionStatus };
            Assert.AreEqual(expected, completeResult.GetTransactionDetails);
        }
    }
}
