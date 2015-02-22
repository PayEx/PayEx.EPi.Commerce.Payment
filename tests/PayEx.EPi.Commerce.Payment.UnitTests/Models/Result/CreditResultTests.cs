using NUnit.Framework;
using PayEx.EPi.Commerce.Payment.Models.Result;
using PayEx.EPi.Commerce.Payment.Payment;

namespace PayEx.EPi.Commerce.Payment.UnitTests.Models.Result
{
    public class CreditResultTests
    {
        [TestCase(TransactionStatus.Initialize, true, false)]
        [TestCase(TransactionStatus.Authorize, true, false)]
        [TestCase(TransactionStatus.Capture, true, false)]
        [TestCase(TransactionStatus.Credit, true, true)]
        [TestCase(TransactionStatus.Initialize, false, false)]
        [TestCase(TransactionStatus.Authorize, false, false)]
        [TestCase(TransactionStatus.Credit, false, false)]
        [TestCase(TransactionStatus.Capture, false, false)]
        internal void Success_ReturnsCorrectResult(TransactionStatus transactionStatus, bool statusSuccess, bool expected)
        {
            Status status = Factory.CreateStatus(statusSuccess);

            CreditResult creditResult = new CreditResult { Status = status, TransactionStatus = transactionStatus };
            Assert.AreEqual(expected, creditResult.Success);
        }
    }
}
